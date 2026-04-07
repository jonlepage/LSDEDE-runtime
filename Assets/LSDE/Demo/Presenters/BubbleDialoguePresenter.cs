using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        [SerializeField]
        [Tooltip(
            "The action executor that maps action IDs to game effects "
                + "(camera shake, movement, etc.)."
        )]
        private DemoActionExecutor _actionExecutor;

        [SerializeField]
        [Tooltip(
            "Character ID used as fallback when a CHOICE block has no assigned character. "
                + "The choice bubble will appear on this character. "
                + "Leave empty to auto-select the first choice without displaying UI."
        )]
        private string _fallbackChoiceCharacterId = lsdeCharacter.l4;

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

            // Play bounce animation on the character when they start speaking
            var bounceAnimation = characterMarker.GetComponent<CharacterBounceAnimation>();
            if (bounceAnimation != null)
            {
                bounceAnimation.PlayBounce();
            }

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
            BlockCharacter resolvedCharacter,
            IReadOnlyList<RuntimeChoiceItem> visibleChoices,
            Action<string> selectChoiceAndAdvance
        )
        {
            // Clear click advancer — choices use their own buttons, not click-anywhere
            _dialogueClickAdvancer.ClearAllPendingAdvances();

            if (visibleChoices.Count == 0)
            {
                Debug.LogWarning(
                    $"{LogPrefix} CHOICE  {choiceBlock.Label} — no visible choices. Skipping."
                );
                return;
            }

            // Resolve the character to display choices on.
            // Priority: 1) resolvedCharacter from engine (context.Character)
            //           2) static metadata from blueprint editor (Metadata.Characters)
            //           3) configurable fallback character (_fallbackChoiceCharacterId)
            // The fallback handles scenes like simpleAction where the CHOICE block
            // follows an ACTION block and neither the engine nor the metadata provide
            // a character. The dev configures which character hosts "unattached" choices.
            var choiceCharacter =
                resolvedCharacter ?? choiceBlock.Metadata?.Characters?.FirstOrDefault();

            // Find the character marker in the scene
            DialogueCharacterMarker characterMarker = null;

            if (choiceCharacter != null)
            {
                characterMarker = _characterRegistry.FindMarkerByCharacterId(choiceCharacter.Id);
            }

            // Fallback: use the configurable default character for unattached choice blocks
            if (characterMarker == null && !string.IsNullOrEmpty(_fallbackChoiceCharacterId))
            {
                characterMarker = _characterRegistry.FindMarkerByCharacterId(
                    _fallbackChoiceCharacterId
                );

                if (characterMarker != null)
                {
                    Debug.Log(
                        $"{LogPrefix} CHOICE  {choiceBlock.Label} — using fallback character "
                            + $"'{_fallbackChoiceCharacterId}' for choice display."
                    );
                }
            }

            if (characterMarker == null)
            {
                Debug.LogWarning(
                    $"{LogPrefix} CHOICE  {choiceBlock.Label} — no character found in scene. "
                        + "Auto-selecting first choice."
                );
                selectChoiceAndAdvance(visibleChoices[0].Uuid);
                return;
            }

            var bubbleController =
                characterMarker.BubbleAnchorPoint.GetComponentInChildren<SpeechBubbleController>(
                    true
                );

            if (bubbleController == null)
            {
                Debug.LogWarning(
                    $"{LogPrefix} CHOICE  {choiceBlock.Label} — no SpeechBubbleController on "
                        + $"character '{characterMarker.LsdeCharacterId}'. Auto-selecting first choice."
                );
                selectChoiceAndAdvance(visibleChoices[0].Uuid);
                return;
            }

            var blockUuid = choiceBlock.Uuid;
            var characterName =
                choiceCharacter?.Name ?? choiceCharacter?.Id ?? characterMarker.LsdeCharacterId;

            // Track this bubble for cleanup in PresentBlockCleanup
            _activeBubblesByBlockUuid[blockUuid] = bubbleController;

            // Build the localized choice list for the bubble
            var choiceDisplayItems = new List<(string uuid, string localizedText)>();
            foreach (var choice in visibleChoices)
            {
                var localizedText = LsdeUtils.GetLocalizedText(choice.DialogueText);
                choiceDisplayItems.Add((choice.Uuid, localizedText ?? choice.Label ?? "???"));
            }

            // Show choice buttons in the bubble — the bubble handles layout and interaction
            bubbleController.ShowChoices(characterName, choiceDisplayItems, selectChoiceAndAdvance);

            Debug.Log(
                $"{LogPrefix} CHOICE  {choiceBlock.Label} — {characterName}: "
                    + $"{visibleChoices.Count} choices displayed"
            );
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
        public void PresentActionBlock(
            ActionBlock actionBlock,
            Action resolveAndAdvance,
            Action<object> rejectAndAdvance
        )
        {
            var actions = actionBlock.Actions;
            var actionCount = actions?.Count ?? 0;

            Debug.Log($"{LogPrefix} ACTION  {actionBlock.Label} — {actionCount} actions");

            // No actions or no executor: resolve immediately (nothing to execute)
            if (actions == null || actionCount == 0 || _actionExecutor == null)
            {
                if (_actionExecutor == null && actionCount > 0)
                {
                    Debug.LogWarning(
                        $"{LogPrefix} No IActionExecutor assigned on BubbleDialoguePresenter — "
                            + "resolving action block immediately without execution."
                    );
                }
                resolveAndAdvance();
                return;
            }

            // Start parallel execution of all actions (Unity equivalent of Promise.all).
            // Each action runs as an independent coroutine. When all complete,
            // we resolve (success) or reject (failure) and advance the engine.
            StartCoroutine(
                ExecuteAllActionsInParallelCoroutine(actions, resolveAndAdvance, rejectAndAdvance)
            );
        }

        /// <summary>
        /// Execute all actions from an ACTION block in parallel and wait for all to complete.
        /// Unity equivalent of <c>Promise.all</c> from the TypeScript reference.
        ///
        /// Each action is started as an independent coroutine via <see cref="_actionExecutor"/>.
        /// A shared completion counter tracks progress. When all actions complete successfully,
        /// <paramref name="resolveAndAdvance"/> is called. If any action throws an exception,
        /// <paramref name="rejectAndAdvance"/> is called with the first error encountered.
        /// </summary>
        /// <param name="actions">The list of actions to execute in parallel.</param>
        /// <param name="resolveAndAdvance">Called when all actions complete successfully.</param>
        /// <param name="rejectAndAdvance">Called if any action fails.</param>
        private IEnumerator ExecuteAllActionsInParallelCoroutine(
            List<ExportAction> actions,
            Action resolveAndAdvance,
            Action<object> rejectAndAdvance
        )
        {
            int totalActionCount = actions.Count;
            int completedActionCount = 0;
            bool hasAnyActionFailed = false;
            object firstEncounteredError = null;

            // Start all action coroutines in parallel — they run concurrently
            foreach (var action in actions)
            {
                StartCoroutine(
                    ExecuteSingleActionWithCompletionTracking(
                        action,
                        onActionCompleted: () =>
                        {
                            completedActionCount++;
                        },
                        onActionFailed: (error) =>
                        {
                            if (!hasAnyActionFailed)
                            {
                                hasAnyActionFailed = true;
                                firstEncounteredError = error;
                            }
                            completedActionCount++;
                        }
                    )
                );
            }

            // Yield until all actions have reported completion (success or failure)
            while (completedActionCount < totalActionCount)
            {
                yield return null;
            }

            // All actions finished — resolve or reject based on outcome
            if (hasAnyActionFailed)
            {
                Debug.LogError($"{LogPrefix} Action block failed: {firstEncounteredError}");
                rejectAndAdvance(firstEncounteredError);
            }
            else
            {
                resolveAndAdvance();
            }
        }

        /// <summary>
        /// Wrapper coroutine that executes a single action via <see cref="_actionExecutor"/>
        /// and reports completion or failure through callbacks.
        ///
        /// Unity does not allow <c>try/catch</c> around <c>yield return</c>, so this method
        /// manually advances the <see cref="IEnumerator"/> with <c>MoveNext()</c> inside a
        /// <c>try/catch</c> block. This is a standard Unity pattern for exception-safe
        /// coroutine delegation — it prevents one failing action from crashing the
        /// entire parallel batch.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="onActionCompleted">Called when the action completes successfully.</param>
        /// <param name="onActionFailed">Called with the error if the action throws an exception.</param>
        private IEnumerator ExecuteSingleActionWithCompletionTracking(
            ExportAction action,
            Action onActionCompleted,
            Action<object> onActionFailed
        )
        {
            IEnumerator actionCoroutine;

            try
            {
                actionCoroutine = _actionExecutor.ExecuteAction(action);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"{LogPrefix} Action '{action.ActionId}' threw during setup: "
                        + exception.Message
                );
                onActionFailed(exception);
                yield break;
            }

            // Manually advance the enumerator with MoveNext() inside try/catch.
            // Unity forbids try/catch around "yield return", but MoveNext() is a
            // regular method call that can be wrapped safely.
            while (true)
            {
                bool hasMoreSteps;
                try
                {
                    hasMoreSteps = actionCoroutine.MoveNext();
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"{LogPrefix} Action '{action.ActionId}' threw during execution: "
                            + exception.Message
                    );
                    onActionFailed(exception);
                    yield break;
                }

                if (!hasMoreSteps)
                {
                    break;
                }

                yield return actionCoroutine.Current;
            }

            onActionCompleted();
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

            // Reset camera to normal follow mode (clears shake, resumes follow).
            // This ensures the camera returns to tracking the player after
            // a scene that left it paused on a non-player character.
            if (_actionExecutor != null)
            {
                _actionExecutor.ResetCameraState();
            }

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
