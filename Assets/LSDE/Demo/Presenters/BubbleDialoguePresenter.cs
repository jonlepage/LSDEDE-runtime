using System;
using System.Collections.Generic;
using LSDE.Runtime;
using LsdeDialogEngine;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Visual implementation of <see cref="IDialoguePresenter"/> that displays
    /// speech bubbles above characters in the 3D scene.
    /// Uses <see cref="DialogueCharacterRegistry"/> to locate characters and
    /// <see cref="DialogueClickAdvancer"/> to wait for player clicks before advancing.
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
        /// Tracks which bubble is currently visible so we can hide it
        /// before showing a new one (protection against rapid block chaining).
        /// </summary>
        private SpeechBubbleController _activeBubbleController;

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
            // Hide any previously active bubble (handles rapid block chaining
            // where cleanup hasn't fired yet)
            HideActiveBubble();

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
            bubbleController.ShowDialogue(characterName, localizedText);
            _activeBubbleController = bubbleController;

            // Store the advance callback — the player clicks to invoke it
            _dialogueClickAdvancer.SetPendingAdvance(advanceToNextBlock);

            Debug.Log(
                $"{LogPrefix} DIALOG  {dialogBlock.Label} — {characterName}: \"{TruncateText(localizedText, 50)}\""
            );
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
            _dialogueClickAdvancer.ClearPendingAdvance();

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
            HideActiveBubble();
            _dialogueClickAdvancer.ClearPendingAdvance();
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
            HideActiveBubble();
            _dialogueClickAdvancer.ClearPendingAdvance();
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

        private void HideActiveBubble()
        {
            if (_activeBubbleController != null && _activeBubbleController.IsVisible)
            {
                _activeBubbleController.HideDialogue();
            }
            _activeBubbleController = null;
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
