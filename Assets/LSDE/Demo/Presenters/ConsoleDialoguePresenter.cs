using System.Collections.Generic;
using System.Text;
using LSDE.Runtime;
using LsdeDialogEngine;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Phase 1 implementation of <see cref="IDialoguePresenter"/> that outputs
    /// all dialogue events to the Unity console via Debug.Log.
    /// This is a plain C# class (not a MonoBehaviour) — it has no Unity lifecycle needs.
    /// Replace this with a UI-based presenter in Phase 2 for bubble text rendering.
    /// </summary>
    public class ConsoleDialoguePresenter : IDialoguePresenter
    {
        private const string LogPrefix = "[LSDE]";

        /// <inheritdoc />
        public void PresentDialogueBlock(
            DialogBlock dialogBlock,
            BlockCharacter resolvedCharacter,
            string localizedText
        )
        {
            var characterName = resolvedCharacter?.Name ?? "???";
            var characterId = resolvedCharacter?.Id ?? "unknown";
            var emotion = resolvedCharacter?.Emotion ?? "";
            var emotionSuffix = string.IsNullOrEmpty(emotion) ? "" : $" [{emotion}]";

            Debug.Log(
                $"{LogPrefix} DIALOG  {dialogBlock.Label}\n"
                    + $"{LogPrefix}   Character: {characterName} ({characterId}){emotionSuffix}\n"
                    + $"{LogPrefix}   \"{localizedText ?? "—"}\""
            );
        }

        /// <inheritdoc />
        public void PresentChoiceBlock(
            ChoiceBlock choiceBlock,
            IReadOnlyList<RuntimeChoiceItem> visibleChoices
        )
        {
            var totalChoiceCount = choiceBlock.Choices?.Count ?? 0;
            var logBuilder = new StringBuilder();
            logBuilder.AppendLine(
                $"{LogPrefix} CHOICE  {choiceBlock.Label} — "
                    + $"{visibleChoices.Count}/{totalChoiceCount} choices visible"
            );

            for (int choiceIndex = 0; choiceIndex < visibleChoices.Count; choiceIndex++)
            {
                var choice = visibleChoices[choiceIndex];
                var choiceText = LsdeUtils.GetLocalizedText(choice.DialogueText);
                var choiceLabel = choice.Label ?? choice.Uuid.Substring(0, 8);
                var activeMarker = choiceIndex == 0 ? " (auto-selected)" : "";

                logBuilder.AppendLine(
                    $"{LogPrefix}   -> {choiceLabel}: \"{choiceText ?? "—"}\"{activeMarker}"
                );
            }

            Debug.Log(logBuilder.ToString().TrimEnd());
        }

        /// <inheritdoc />
        public void PresentConditionBlock(
            ConditionBlock conditionBlock,
            IReadOnlyList<RuntimeConditionGroup> conditionGroups,
            object resolvedResult
        )
        {
            var isDispatcher = conditionBlock.NativeProperties?.EnableDispatcher == true;
            var modeLabel = isDispatcher ? " [DISPATCHER]" : "";

            var logBuilder = new StringBuilder();
            logBuilder.AppendLine(
                $"{LogPrefix} CONDITION  {conditionBlock.Label} — "
                    + $"{conditionGroups.Count} groups{modeLabel}"
            );

            for (int groupIndex = 0; groupIndex < conditionGroups.Count; groupIndex++)
            {
                var group = conditionGroups[groupIndex];
                foreach (var condition in group.Conditions)
                {
                    logBuilder.AppendLine(
                        $"{LogPrefix}   [case {groupIndex}] port:{group.PortIndex} "
                            + $"key:{condition.Key} {condition.Operator} {condition.Value} "
                            + $"-> {group.Result}"
                    );
                }
            }

            logBuilder.Append($"{LogPrefix}   Result: {resolvedResult}");
            Debug.Log(logBuilder.ToString());
        }

        /// <inheritdoc />
        public void PresentActionBlock(ActionBlock actionBlock)
        {
            var actions = actionBlock.Actions;
            var logBuilder = new StringBuilder();
            logBuilder.AppendLine(
                $"{LogPrefix} ACTION  {actionBlock.Label} — {actions.Count} actions"
            );

            foreach (var action in actions)
            {
                var parameters = string.Join(", ", action.Params);
                logBuilder.AppendLine($"{LogPrefix}   -> {action.ActionId}({parameters})");
            }

            Debug.Log(logBuilder.ToString().TrimEnd());
        }

        /// <inheritdoc />
        public void PresentSceneEnter(ISceneHandle sceneHandle)
        {
            Debug.Log($"{LogPrefix} === Scene Enter === running={sceneHandle.IsRunning()}");
        }

        /// <inheritdoc />
        public void PresentSceneExit()
        {
            Debug.Log($"{LogPrefix} === Scene Exit ===");
        }

        /// <inheritdoc />
        public void PresentBeforeBlock(BlueprintBlock block)
        {
            var delay = block.NativeProperties?.Delay;
            if (delay.HasValue)
            {
                Debug.Log(
                    $"{LogPrefix}   before: {LsdeUtils.GetBlockLabel(block)} delay={delay.Value}s"
                );
            }
        }

        /// <inheritdoc />
        public void PresentBlockCleanup(BlueprintBlock block)
        {
            Debug.Log($"{LogPrefix}   cleanup: {LsdeUtils.GetBlockLabel(block)}");
        }

        /// <inheritdoc />
        public void PresentSceneComplete(
            IReadOnlyList<string> visitedBlockLabels,
            IReadOnlyDictionary<string, IReadOnlyList<string>> choiceHistory
        )
        {
            var visitedList = string.Join(", ", visitedBlockLabels);
            Debug.Log($"{LogPrefix} Visited: {visitedList}");

            if (choiceHistory.Count > 0)
            {
                var historyBuilder = new StringBuilder();
                historyBuilder.AppendLine($"{LogPrefix} Choice History:");
                foreach (var entry in choiceHistory)
                {
                    var selectedChoices = string.Join(", ", entry.Value);
                    historyBuilder.AppendLine($"{LogPrefix}   {entry.Key} -> [{selectedChoices}]");
                }
                Debug.Log(historyBuilder.ToString().TrimEnd());
            }
        }
    }
}
