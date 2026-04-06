using System;
using System.Collections;
using System.Collections.Generic;
using LSDE.Runtime;
using LsdeDialogEngine;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Visual implementation of <see cref="IDialoguePresenter"/> that displays
    /// speech bubbles above characters in the 3D scene.
    /// Supports multiple simultaneous bubbles for multi-track parallel dialogue.
    ///
    /// Uses <see cref="DialogueCharacterRegistry"/> to locate characters and
    /// <see cref="DialogueClickAdvancer"/> to wait for player clicks before advancing.
    ///
    /// For async blocks (<c>NativeProperties.IsAsync == true</c>), the presenter
    /// auto-advances without waiting for a click. If a timeout is specified,
    /// a coroutine handles the delayed auto-advance.
    ///
    /// For block types that don't need visual rendering (CONDITION, ACTION),
    /// this presenter falls back to console logging.
    /// </summary>
    public class BubbleDialoguePresenter : MonoBehaviour, IDialoguePresenter
    {
        [SerializeField]
        [Tooltip("The character registry that maps LSDE character IDs to scene GameObjects.")]
        private DialogueCharacterRegistry _characterRegistry;

        [SerializeField]
        [Tooltip("The click advancer that stores the pending Next callback.")]
        private DialogueClickAdvancer _dialogueClickAdvancer;

        private const string LogPrefix = "[LSDE]";

        /// <summary>
        /// Conversion factor from blueprint timeout/delay values (milliseconds) to seconds.
        /// Blueprint JSON stores durations in milliseconds (e.g. 2000 for 2 seconds).
        /// Unity's WaitForSeconds expects seconds.
        /// </summary>
        private const double MillisecondsToSeconds = 1000.0;

        /// <summary>
        /// All currently visible speech bubbles, keyed by their dialogue block UUID.
        /// Multiple bubbles can be active simultaneously during parallel multi-track dialogue.
        /// In single-track mode, only one entry exists at a time.
        /// </summary>
        private readonly Dictionary<string, SpeechBubbleController> _activeBubblesByBlockUuid =
            new Dictionary<string, SpeechBubbleController>();

        /// <summary>
        /// Active timeout coroutines keyed by block UUID.
        /// Stored so they can be cancelled if the block is cleaned up before the timeout fires
        /// (e.g. player clicks to advance before the timeout, or scene exits).
        /// </summary>
        private readonly Dictionary<string, Coroutine> _activeTimeoutCoroutinesByBlockUuid =
            new Dictionary<string, Coroutine>();

        /// <summary>
        /// Warm up all speech bubbles at startup so TextMeshPro initializes
        /// its font atlas during scene load. This avoids the lag spike that occurs
        /// when the first bubble is shown via SetActive for the first time.
        /// Bubbles can stay disabled in the Editor (less visual clutter) —
        /// they are activated and hidden (alpha=0) here at runtime.
        /// </summary>
        private void Start()
        {
            var allBubbleControllers = FindObjectsByType<SpeechBubbleController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

            foreach (var bubbleController in allBubbleControllers)
            {
                bubbleController.WarmUp();
            }
        }

        /// <inheritdoc />
        public void PresentDialogueBlock(
            DialogBlock dialogBlock,
            BlockCharacter resolvedCharacter,
            string localizedText,
            Action advanceToNextBlock
        )
        {
            // Do NOT hide previously active bubbles — in multi-track mode,
            // multiple bubbles must coexist. Cleanup is per-block via PresentBlockCleanup.

            if (resolvedCharacter == null)
            {
                Debug.LogWarning(
                    $"{LogPrefix} No character resolved for block '{dialogBlock.Label}'. "
                        + "Advancing immediately."
                );
                advanceToNextBlock();
                return;
            }

            var characterMarker = _characterRegistry.FindMarkerByCharacterId(resolvedCharacter.Id);

            if (characterMarker == null)
            {
                Debug.LogWarning(
                    $"{LogPrefix} Character '{resolvedCharacter.Id}' not found in scene. "
                        + "Advancing immediately."
                );
                advanceToNextBlock();
                return;
            }

            // Find the SpeechBubbleController on this character's bubble anchor
            var bubbleController =
                characterMarker.BubbleAnchorPoint.GetComponentInChildren<SpeechBubbleController>(
                    true
                );

            if (bubbleController == null)
            {
                Debug.LogWarning(
                    $"{LogPrefix} No SpeechBubbleController found on character '{resolvedCharacter.Id}'. "
                        + "Advancing immediately."
                );
                advanceToNextBlock();
                return;
            }

            var characterName = resolvedCharacter.Name ?? resolvedCharacter.Id;
            var blockUuid = dialogBlock.Uuid;

            // Track this bubble in the active dictionary
            _activeBubblesByBlockUuid[blockUuid] = bubbleController;

            // Show the bubble — fade-in and typewriter play in parallel
            bubbleController.ShowDialogue(characterName, localizedText);

            Debug.Log(
                $"{LogPrefix} DIALOG  {dialogBlock.Label} — {characterName}: \"{TruncateText(localizedText, 50)}\""
            );

            // Determine flow control based on NativeProperties.
            // Async blocks without waitInput auto-advance (immediately or after timeout).
            // Sync blocks and async blocks with waitInput wait for player click.
            bool isAsyncBlock = dialogBlock.NativeProperties?.IsAsync == true;
            bool needsPlayerInput =
                dialogBlock.NativeProperties?.WaitInput == true || !isAsyncBlock;
            double? timeoutMilliseconds = dialogBlock.NativeProperties?.Timeout;

            if (needsPlayerInput)
            {
                // Sync block, or async block with waitInput: wait for player click.
                _dialogueClickAdvancer.SetPendingAdvance(
                    blockUuid,
                    advanceToNextBlock,
                    bubbleController
                );

                // If a timeout is also specified, start a timeout coroutine as fallback.
                // Whichever fires first (click or timeout) advances the block.
                // The other is cleaned up in PresentBlockCleanup.
                if (timeoutMilliseconds.HasValue && timeoutMilliseconds.Value > 0)
                {
                    var timeoutInSeconds = (float)(
                        timeoutMilliseconds.Value / MillisecondsToSeconds
                    );
                    var timeoutCoroutine = StartCoroutine(
                        AutoAdvanceAfterTimeoutCoroutine(
                            blockUuid,
                            advanceToNextBlock,
                            timeoutInSeconds
                        )
                    );
                    _activeTimeoutCoroutinesByBlockUuid[blockUuid] = timeoutCoroutine;
                }
            }
            else
            {
                // Async block without waitInput: auto-advance.
                // The bubble remains on screen until the engine calls PresentBlockCleanup.
                if (timeoutMilliseconds.HasValue && timeoutMilliseconds.Value > 0)
                {
                    // Timeout specified: wait that duration before calling next().
                    var timeoutInSeconds = (float)(
                        timeoutMilliseconds.Value / MillisecondsToSeconds
                    );
                    var timeoutCoroutine = StartCoroutine(
                        AutoAdvanceAfterTimeoutCoroutine(
                            blockUuid,
                            advanceToNextBlock,
                            timeoutInSeconds
                        )
                    );
                    _activeTimeoutCoroutinesByBlockUuid[blockUuid] = timeoutCoroutine;
                }
                else
                {
                    // No timeout: advance immediately.
                    advanceToNextBlock();
                }
            }
        }

        /// <summary>
        /// Coroutine that waits for the specified duration then auto-advances the block.
        /// Used for async blocks without waitInput (auto-advance after display time)
        /// and as a fallback timeout for sync/waitInput blocks (player can still click earlier).
        /// </summary>
        /// <param name="blockUuid">The UUID of the block being timed.</param>
        /// <param name="advanceToNextBlock">The engine's Next callback.</param>
        /// <param name="timeoutInSeconds">How long to wait before auto-advancing.</param>
        private IEnumerator AutoAdvanceAfterTimeoutCoroutine(
            string blockUuid,
            Action advanceToNextBlock,
            float timeoutInSeconds
        )
        {
            yield return new WaitForSeconds(timeoutInSeconds);

            // Remove the timeout tracking entry since this coroutine completed naturally
            _activeTimeoutCoroutinesByBlockUuid.Remove(blockUuid);

            // Clear this block from the click advancer if it was registered there.
            // This prevents a stale entry from being invoked by a later click.
            _dialogueClickAdvancer.ClearPendingAdvanceForBlock(blockUuid);

            advanceToNextBlock();
        }

        /// <inheritdoc />
        public void PresentChoiceBlock(
            ChoiceBlock choiceBlock,
            IReadOnlyList<RuntimeChoiceItem> visibleChoices,
            Action<string> selectChoiceAndAdvance
        )
        {
            // Phase 2b: auto-select first choice (no UI buttons yet).
            // Phase 3 will display choice buttons in the bubble.
            Debug.Log(
                $"{LogPrefix} CHOICE  {choiceBlock.Label} — {visibleChoices.Count} visible (auto-selecting first)"
            );

            // Clear click advancer — choices don't use click-anywhere
            _dialogueClickAdvancer.ClearAllPendingAdvances();

            if (visibleChoices.Count > 0)
            {
                selectChoiceAndAdvance(visibleChoices[0].Uuid);
            }
        }

        /// <inheritdoc />
        public void PresentConditionBlock(
            ConditionBlock conditionBlock,
            IReadOnlyList<RuntimeConditionGroup> conditionGroups,
            object resolvedResult
        )
        {
            // Conditions are invisible routing — console log only
            Debug.Log($"{LogPrefix} CONDITION  {conditionBlock.Label} — result: {resolvedResult}");
        }

        /// <inheritdoc />
        public void PresentActionBlock(ActionBlock actionBlock)
        {
            // Actions are side effects — console log only for now
            Debug.Log(
                $"{LogPrefix} ACTION  {actionBlock.Label} — {actionBlock.Actions.Count} actions"
            );
        }

        /// <inheritdoc />
        public void PresentSceneEnter(ISceneHandle sceneHandle)
        {
            Debug.Log($"{LogPrefix} === Scene Enter === running={sceneHandle.IsRunning()}");
        }

        /// <inheritdoc />
        public void PresentSceneExit()
        {
            HideAllActiveBubbles();
            CancelAllActiveTimeoutCoroutines();
            _dialogueClickAdvancer.ClearAllPendingAdvances();
            Debug.Log($"{LogPrefix} === Scene Exit ===");
        }

        /// <inheritdoc />
        public void PresentBeforeBlock(BlueprintBlock block)
        {
            // No visual representation needed for before-block
        }

        /// <inheritdoc />
        public void PresentBlockCleanup(BlueprintBlock block)
        {
            var blockUuid = block.Uuid;

            // Hide only this block's bubble (if it exists and is still visible).
            // Other parallel tracks' bubbles remain on screen undisturbed.
            if (_activeBubblesByBlockUuid.TryGetValue(blockUuid, out var bubbleController))
            {
                if (bubbleController != null && bubbleController.IsVisible)
                {
                    bubbleController.HideDialogue();
                }
                _activeBubblesByBlockUuid.Remove(blockUuid);
            }

            // Cancel any running timeout coroutine for this block.
            // This prevents a stale timeout from calling advanceToNextBlock a second time
            // after the block has already been advanced (e.g. by player click).
            if (
                _activeTimeoutCoroutinesByBlockUuid.TryGetValue(blockUuid, out var timeoutCoroutine)
            )
            {
                if (timeoutCoroutine != null)
                {
                    StopCoroutine(timeoutCoroutine);
                }
                _activeTimeoutCoroutinesByBlockUuid.Remove(blockUuid);
            }

            // Clear this block's pending advance from the click advancer
            _dialogueClickAdvancer.ClearPendingAdvanceForBlock(blockUuid);
        }

        /// <inheritdoc />
        public void PresentSceneComplete(
            IReadOnlyList<string> visitedBlockLabels,
            IReadOnlyDictionary<string, IReadOnlyList<string>> choiceHistory
        )
        {
            var visitedList = string.Join(", ", visitedBlockLabels);
            Debug.Log($"{LogPrefix} Visited: {visitedList}");
        }

        /// <summary>
        /// Hide all currently active speech bubbles and clear the tracking dictionary.
        /// Used during scene exit when all dialogue must be dismissed at once.
        /// </summary>
        private void HideAllActiveBubbles()
        {
            foreach (var bubbleController in _activeBubblesByBlockUuid.Values)
            {
                if (bubbleController != null && bubbleController.IsVisible)
                {
                    bubbleController.HideDialogue();
                }
            }
            _activeBubblesByBlockUuid.Clear();
        }

        /// <summary>
        /// Cancel all running timeout coroutines and clear the tracking dictionary.
        /// Used during scene exit to prevent stale timeouts from firing after cleanup.
        /// </summary>
        private void CancelAllActiveTimeoutCoroutines()
        {
            foreach (var timeoutCoroutine in _activeTimeoutCoroutinesByBlockUuid.Values)
            {
                if (timeoutCoroutine != null)
                {
                    StopCoroutine(timeoutCoroutine);
                }
            }
            _activeTimeoutCoroutinesByBlockUuid.Clear();
        }

        private static string TruncateText(string text, int maximumLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maximumLength)
            {
                return text ?? "";
            }
            return text.Substring(0, maximumLength) + "...";
        }
    }
}
