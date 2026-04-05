using System.Collections.Generic;
using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Abstraction layer between LSDEDE runtime handlers and the game's rendering system.
    /// Handlers delegate all presentation logic to this interface — they never render directly.
    /// Phase 1 uses a console implementation (Debug.Log). Phase 2+ will swap in UI-based presenters.
    /// </summary>
    public interface IDialoguePresenter
    {
        /// <summary>
        /// Present a DIALOG block — display character dialogue text.
        /// </summary>
        /// <param name="dialogBlock">The dialog block being executed.</param>
        /// <param name="resolvedCharacter">The character resolved by the game (may be null if unavailable).</param>
        /// <param name="localizedText">The dialogue text in the current locale.</param>
        void PresentDialogueBlock(
            DialogBlock dialogBlock,
            BlockCharacter resolvedCharacter,
            string localizedText
        );

        /// <summary>
        /// Present a CHOICE block — display available choices for the player.
        /// </summary>
        /// <param name="choiceBlock">The choice block being executed.</param>
        /// <param name="visibleChoices">Choices filtered by visibility (Visible != false).</param>
        void PresentChoiceBlock(
            ChoiceBlock choiceBlock,
            IReadOnlyList<RuntimeChoiceItem> visibleChoices
        );

        /// <summary>
        /// Present a CONDITION block — display condition evaluation results.
        /// </summary>
        /// <param name="conditionBlock">The condition block being executed.</param>
        /// <param name="conditionGroups">All condition groups with pre-evaluated results.</param>
        /// <param name="resolvedResult">The resolved routing result (int for switch, List&lt;int&gt; for dispatcher).</param>
        void PresentConditionBlock(
            ConditionBlock conditionBlock,
            IReadOnlyList<RuntimeConditionGroup> conditionGroups,
            object resolvedResult
        );

        /// <summary>
        /// Present an ACTION block — display action execution details.
        /// </summary>
        /// <param name="actionBlock">The action block being executed.</param>
        void PresentActionBlock(ActionBlock actionBlock);

        /// <summary>
        /// Called when a scene starts executing.
        /// </summary>
        /// <param name="sceneHandle">The scene handle that just started.</param>
        void PresentSceneEnter(ISceneHandle sceneHandle);

        /// <summary>
        /// Called when a scene finishes executing.
        /// </summary>
        void PresentSceneExit();

        /// <summary>
        /// Called before each block is executed (from OnBeforeBlock handler).
        /// </summary>
        /// <param name="block">The block about to be executed.</param>
        void PresentBeforeBlock(BlueprintBlock block);

        /// <summary>
        /// Called when a block's cleanup function fires (engine moves to next block).
        /// </summary>
        /// <param name="block">The block being cleaned up.</param>
        void PresentBlockCleanup(BlueprintBlock block);

        /// <summary>
        /// Called when a scene completes — display visited blocks and choice history summary.
        /// </summary>
        /// <param name="visitedBlockLabels">Ordered list of visited block labels.</param>
        /// <param name="choiceHistory">Map of choice block UUID to selected choice UUIDs.</param>
        void PresentSceneComplete(
            IReadOnlyList<string> visitedBlockLabels,
            IReadOnlyDictionary<string, IReadOnlyList<string>> choiceHistory
        );
    }
}
