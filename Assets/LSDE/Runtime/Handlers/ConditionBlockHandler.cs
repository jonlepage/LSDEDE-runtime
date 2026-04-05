using System;
using System.Collections.Generic;
using System.Linq;
using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Global handler for CONDITION blocks. Reads pre-evaluated condition groups,
    /// resolves the routing result, and delegates presentation to <see cref="IDialoguePresenter"/>.
    /// Supports both switch mode (first match) and dispatcher mode (all matches fire async).
    /// </summary>
    public class ConditionBlockHandler
    {
        private readonly IDialoguePresenter _dialoguePresenter;

        /// <summary>
        /// Create a new condition block handler.
        /// </summary>
        /// <param name="dialoguePresenter">The presenter that will display condition evaluation results.</param>
        public ConditionBlockHandler(IDialoguePresenter dialoguePresenter)
        {
            _dialoguePresenter =
                dialoguePresenter ?? throw new ArgumentNullException(nameof(dialoguePresenter));
        }

        /// <summary>
        /// Handle a CONDITION block dispatched by the LSDEDE runtime.
        /// Matches the <see cref="BlockHandler{ConditionBlock, IConditionContext}"/> delegate signature.
        /// </summary>
        /// <param name="arguments">Block handler arguments containing the block, context, and next callback.</param>
        /// <returns>A cleanup action called when the engine leaves this block, or null.</returns>
        public Action HandleConditionBlock(
            BlockHandlerArgs<ConditionBlock, IConditionContext> arguments
        )
        {
            var block = arguments.Block;
            var context = arguments.Context;
            var conditionGroups = context.ConditionGroups;
            bool isDispatcherMode = block.NativeProperties?.EnableDispatcher == true;

            // Collect all groups whose conditions evaluated to true
            List<int> matchedPortIndices = conditionGroups
                .Where(group => group.Result == true)
                .Select(group => group.PortIndex)
                .ToList();

            // Switch mode: route to first matching group's port, or -1 for default/false
            // Dispatcher mode: fire all matching groups as async tracks
            object resolvedResult = isDispatcherMode
                ? (object)matchedPortIndices
                : (object)(matchedPortIndices.Count > 0 ? matchedPortIndices[0] : -1);

            _dialoguePresenter.PresentConditionBlock(block, conditionGroups, resolvedResult);

            context.Resolve(resolvedResult);
            arguments.Next();

            return null;
        }
    }
}
