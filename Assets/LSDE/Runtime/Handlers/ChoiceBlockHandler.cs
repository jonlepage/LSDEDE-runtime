using System;
using System.Collections.Generic;
using System.Linq;
using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Global handler for CHOICE blocks. Filters visible choices,
    /// auto-selects the first visible one (Phase 1 — no player input),
    /// and delegates presentation to <see cref="IDialoguePresenter"/>.
    /// </summary>
    public class ChoiceBlockHandler
    {
        private readonly IDialoguePresenter _dialoguePresenter;

        /// <summary>
        /// Create a new choice block handler.
        /// </summary>
        /// <param name="dialoguePresenter">The presenter that will display choice options.</param>
        public ChoiceBlockHandler(IDialoguePresenter dialoguePresenter)
        {
            _dialoguePresenter =
                dialoguePresenter ?? throw new ArgumentNullException(nameof(dialoguePresenter));
        }

        /// <summary>
        /// Handle a CHOICE block dispatched by the LSDEDE runtime.
        /// Matches the <see cref="BlockHandler{ChoiceBlock, IChoiceContext}"/> delegate signature.
        /// </summary>
        /// <param name="arguments">Block handler arguments containing the block, context, and next callback.</param>
        /// <returns>A cleanup action called when the engine leaves this block, or null.</returns>
        public Action HandleChoiceBlock(BlockHandlerArgs<ChoiceBlock, IChoiceContext> arguments)
        {
            var block = arguments.Block;
            var context = arguments.Context;

            // Visible != false means: true (visible) or null (no filter installed = treat as visible)
            List<RuntimeChoiceItem> visibleChoices = context
                .Choices.Where(choice => choice.Visible != false)
                .ToList();

            _dialoguePresenter.PresentChoiceBlock(block, visibleChoices);

            // Phase 1: auto-select the first visible choice to simulate player input.
            // In a real game, the presenter would wait for player interaction,
            // then call SelectChoice + Next from a UI callback.
            if (visibleChoices.Count > 0)
            {
                context.SelectChoice(visibleChoices[0].Uuid);
            }

            arguments.Next();

            return () => _dialoguePresenter.PresentBlockCleanup(block);
        }
    }
}
