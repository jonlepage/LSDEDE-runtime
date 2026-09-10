using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSDE.Runtime;
using LsdeDialogEngine;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Implementation of <see cref="IDialoguePresenter"/> that writes every dialogue event to the
    /// Unity console and advances immediately. A plain C# class — no Unity lifecycle needed.
    ///
    /// <para>Useful to read a whole scene's flow in one pass, and as the smallest complete
    /// integration: it shows what the engine asks of a game and nothing else. It deliberately
    /// ignores <c>timeout</c>, <c>waitInput</c> and <c>delay</c> — there is nothing to look at, so
    /// there is nothing to wait for. <see cref="BubbleDialoguePresenter"/> is where those matter.</para>
    /// </summary>
    public class ConsoleDialoguePresenter : IDialoguePresenter
    {
        private const string LogPrefix = "[LSDE]";

        /// <inheritdoc />
        public void PresentDialogueBlock(
            string presentationKey,
            BlueprintBlock block,
            Card resolvedCharacter,
            string localizedText,
            Action advanceToNextBlock
        )
        {
            var speakerName = resolvedCharacter != null ? resolvedCharacter.Name : "———";
            var speakerId = resolvedCharacter != null ? resolvedCharacter.Id : "nobody";

            Debug.Log(
                $"{LogPrefix} DIALOG  {LsdeUtils.GetBlockLabel(block)}\n"
                    + $"{LogPrefix}   Speaker: {speakerName} ({speakerId})\n"
                    + $"{LogPrefix}   \"{localizedText ?? "—"}\""
            );

            // Console mode: advance at once, no player interaction needed.
            advanceToNextBlock();
        }

        /// <inheritdoc />
        public void PresentChoiceBlock(
            string presentationKey,
            BlueprintBlock block,
            Card resolvedCharacter,
            IReadOnlyList<Card> cast,
            IReadOnlyList<RuntimeChoiceItem> offeredOptions,
            Action<string> selectChoiceAndAdvance
        )
        {
            var totalOptionCount = block.Options?.Count ?? 0;
            var logBuilder = new StringBuilder();
            logBuilder.AppendLine(
                $"{LogPrefix} CHOICE  {LsdeUtils.GetBlockLabel(block)} — "
                    + $"{offeredOptions.Count}/{totalOptionCount} offered"
            );

            for (int optionIndex = 0; optionIndex < offeredOptions.Count; optionIndex++)
            {
                var option = offeredOptions[optionIndex];
                var optionText = LsdeText.Localized(option.Text);
                var pickedMarker = optionIndex == 0 ? " (auto-selected)" : "";

                logBuilder.AppendLine(
                    $"{LogPrefix}   -> {option.Id}: \"{optionText ?? "—"}\"{pickedMarker}"
                );
            }

            Debug.Log(logBuilder.ToString().TrimEnd());

            // Console mode: take the first offered option. The id IS the exit port.
            if (offeredOptions.Count > 0)
            {
                selectChoiceAndAdvance(offeredOptions[0].Id);
            }
        }

        /// <inheritdoc />
        public void PresentConditionBlock(
            BlueprintBlock block,
            IReadOnlyList<RuntimeConditionCase> cases
        )
        {
            var nativeProperties = LsdeUtils.GetNativeProperties(block);
            var modeLabel =
                nativeProperties.PortPerCase == true
                    ? " [portPerCase: the first true case takes its own port]"
                    : " [every case must hold → out, else default]";

            var logBuilder = new StringBuilder();
            logBuilder.AppendLine(
                $"{LogPrefix} CONDITION  {LsdeUtils.GetBlockLabel(block)} — "
                    + $"{cases.Count} case(s){modeLabel}"
            );

            AppendCases(logBuilder, cases);

            Debug.Log(logBuilder.ToString().TrimEnd());
        }

        /// <inheritdoc />
        public void PresentRouterBlock(
            BlueprintBlock block,
            IReadOnlyList<RuntimeConditionCase> cases,
            IReadOnlyList<string> launchedPorts
        )
        {
            var logBuilder = new StringBuilder();
            logBuilder.AppendLine(
                $"{LogPrefix} ROUTER  {LsdeUtils.GetBlockLabel(block)} — "
                    + $"{cases.Count} case(s), all evaluated"
            );

            AppendCases(logBuilder, cases);

            logBuilder.Append(
                $"{LogPrefix}   launching: {string.Join(" → ", launchedPorts)} "
                    + "(continuation last)"
            );

            Debug.Log(logBuilder.ToString());
        }

        /// <inheritdoc />
        public void PresentActionBlock(
            string presentationKey,
            BlueprintBlock block,
            IReadOnlyList<ActionCall> calls,
            Action resolveAndAdvance,
            Action<object> rejectAndAdvance
        )
        {
            var logBuilder = new StringBuilder();
            logBuilder.AppendLine(
                $"{LogPrefix} ACTION  {LsdeUtils.GetBlockLabel(block)} — {calls?.Count ?? 0} call(s)"
            );

            if (calls != null)
            {
                foreach (var call in calls)
                {
                    var functionName = string.IsNullOrEmpty(call.Fn)
                        ? "«no function picked»"
                        : call.Fn;
                    logBuilder.AppendLine(
                        $"{LogPrefix}   -> {functionName}({LsdeActionArgs.Describe(call.Args)})"
                    );
                }
            }

            Debug.Log(logBuilder.ToString().TrimEnd());

            // Console mode: nothing runs, so nothing can fail.
            resolveAndAdvance();
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
        public void PresentBeforeBlock(BlueprintBlock block, NativeProperties nativeProperties)
        {
            if (nativeProperties == null)
            {
                return;
            }

            var notes = new List<string>();
            if (nativeProperties.Delay.HasValue)
            {
                notes.Add($"delay={nativeProperties.Delay.Value}ms");
            }
            if (nativeProperties.Timeout.HasValue)
            {
                notes.Add($"timeout={nativeProperties.Timeout.Value}ms");
            }
            if (nativeProperties.WaitInput == true)
            {
                notes.Add("waitInput");
            }
            if (nativeProperties.IsAsync == true)
            {
                notes.Add("isAsync");
            }
            if (nativeProperties.WaitForBlocks != null && nativeProperties.WaitForBlocks.Count > 0)
            {
                notes.Add($"waitForBlocks=[{string.Join(", ", nativeProperties.WaitForBlocks)}]");
            }
            if (nativeProperties.InPortPerCharacter == true)
            {
                notes.Add("inPortPerCharacter");
            }
            if (nativeProperties.PortPerCharacter == true)
            {
                notes.Add("portPerCharacter");
            }
            if (nativeProperties.SkipIfMissingActor == true)
            {
                notes.Add("skipIfMissingActor");
            }

            if (notes.Count > 0)
            {
                Debug.Log(
                    $"{LogPrefix}   before: {LsdeUtils.GetBlockLabel(block)} "
                        + $"[{string.Join(", ", notes)}]"
                );
            }
        }

        /// <inheritdoc />
        public void PresentBlockCleanup(string presentationKey, BlueprintBlock block)
        {
            Debug.Log($"{LogPrefix}   cleanup: {LsdeUtils.GetBlockLabel(block)}");
        }

        /// <inheritdoc />
        public void PresentSceneComplete(
            IReadOnlyList<string> visitedBlockLabels,
            IReadOnlyDictionary<string, IReadOnlyList<string>> choiceHistory
        )
        {
            Debug.Log($"{LogPrefix} Visited: {string.Join(", ", visitedBlockLabels)}");

            if (choiceHistory.Count > 0)
            {
                var historyBuilder = new StringBuilder();
                historyBuilder.AppendLine($"{LogPrefix} Choice history:");
                foreach (var entry in choiceHistory)
                {
                    historyBuilder.AppendLine(
                        $"{LogPrefix}   {entry.Key} -> [{string.Join(", ", entry.Value)}]"
                    );
                }
                Debug.Log(historyBuilder.ToString().TrimEnd());
            }
        }

        /// <summary>
        /// Append one line per test of every case, with the pre-evaluated result.
        /// </summary>
        private static void AppendCases(
            StringBuilder logBuilder,
            IReadOnlyList<RuntimeConditionCase> cases
        )
        {
            foreach (var conditionCase in cases)
            {
                var tests =
                    conditionCase.When != null && conditionCase.When.Count > 0
                        ? string.Join(
                            " ",
                            conditionCase.When.Select(test =>
                                $"{test.Join ?? ""} {test.Dict}.{test.Entry} {test.Op} {test.Value}".Trim()
                            )
                        )
                        : "(no test — always true)";

                logBuilder.AppendLine(
                    $"{LogPrefix}   [{conditionCase.Port}] {tests} -> {conditionCase.Result}"
                );
            }
        }
    }
}
