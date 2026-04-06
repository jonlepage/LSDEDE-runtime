using System;
using System.Collections.Generic;
using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Abstraction layer between LSDEDE runtime handlers and the game's rendering system.
    /// Handlers delegate all presentation and flow control to this interface — they never
    /// render or call Next() directly. The presenter decides when to advance the dialogue
    /// (immediately for console mode, on player click for visual mode).
    /// This pattern matches the official LSDEDE handler design where Next is deferred.
    /// </summary>
    public interface IDialoguePresenter
    {
        /// <summary>
        /// Present a DIALOG block — display character dialogue text.
        /// The presenter is responsible for calling <paramref name="advanceToNextBlock"/>
        /// when it is ready to advance (e.g. immediately for console, on player click for UI).
        /// </summary>
        /// <param name="dialogBlock">The dialog block being executed.</param>
        /// <param name="resolvedCharacter">The character resolved by the game (may be null if unavailable).</param>
        /// <param name="localizedText">The dialogue text in the current locale.</param>
        /// <param name="advanceToNextBlock">Callback to advance the engine to the next block. Must be called exactly once.</param>
        void PresentDialogueBlock(
            DialogBlock dialogBlock,
            BlockCharacter resolvedCharacter,
            string localizedText,
            Action advanceToNextBlock
        );

        /// <summary>
        /// Present a CHOICE block — display available choices for the player.
        /// The presenter calls <paramref name="selectChoiceAndAdvance"/> with the chosen UUID
        /// when the player makes a selection. This encapsulates both SelectChoice and Next.
        /// </summary>
        /// <param name="choiceBlock">The choice block being executed.</param>
        /// <param name="visibleChoices">Choices filtered by visibility (Visible != false).</param>
        /// <param name="selectChoiceAndAdvance">Callback that selects a choice by UUID and advances the engine. Must be called exactly once.</param>
        void PresentChoiceBlock(
            ChoiceBlock choiceBlock,
            IReadOnlyList<RuntimeChoiceItem> visibleChoices,
            Action<string> selectChoiceAndAdvance
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
        /// Present an ACTION block — execute game actions and control flow via callbacks.
        /// The presenter is responsible for executing all actions in the block (in parallel),
        /// then calling <paramref name="resolveAndAdvance"/> on success or
        /// <paramref name="rejectAndAdvance"/> on failure.
        /// This mirrors the DIALOG/CHOICE pattern where the presenter controls when to advance.
        /// </summary>
        /// <param name="actionBlock">The action block being executed.</param>
        /// <param name="resolveAndAdvance">
        /// Callback that resolves the action (success, follows the "then" port) and advances
        /// the engine to the next block. Must be called exactly once on the success path.
        /// </param>
        /// <param name="rejectAndAdvance">
        /// Callback that rejects the action (failure, follows the "catch" port) and advances
        /// the engine. Must be called exactly once on the failure path. Pass the error/exception.
        /// </param>
        void PresentActionBlock(
            ActionBlock actionBlock,
            Action resolveAndAdvance,
            Action<object> rejectAndAdvance
        );

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
