using System;
using System.Collections.Generic;
using System.Linq;
using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Global handler for CHOICE blocks. Keeps the options the engine tagged as offered and
    /// delegates presentation and selection to <see cref="IDialoguePresenter"/>.
    /// The presenter receives a composed callback that encapsulates both
    /// SelectChoice and Next — it decides when to invoke it (immediately, or on player input).
    /// </summary>
    public class ChoiceBlockHandler
    {
        private readonly IDialoguePresenter _dialoguePresenter;

        /// <summary>
        /// Create a new choice block handler.
        /// </summary>
        /// <param name="dialoguePresenter">The presenter that will display the options.</param>
        public ChoiceBlockHandler(IDialoguePresenter dialoguePresenter)
        {
            _dialoguePresenter =
                dialoguePresenter ?? throw new ArgumentNullException(nameof(dialoguePresenter));
        }

        /// <summary>
        /// Handle a CHOICE block dispatched by the LSDEDE runtime.
        /// </summary>
        /// <param name="arguments">Block, context, and the Next callback.</param>
        /// <returns>A cleanup action called when the engine LEAVES this block.</returns>
        public Action HandleChoiceBlock(BlockHandlerArgs<BlueprintBlock, IChoiceContext> arguments)
        {
            var block = arguments.Block;
            var context = arguments.Context;

            // Every option comes tagged, never pre-filtered. Visible == false is "hidden",
            // null is "unknown" — no resolver could answer its test — which is NOT hidden.
            // Greying the refused ones out instead of dropping them is equally legitimate.
            List<RuntimeChoiceItem> offeredOptions = context
                .Options.Where(option => option.Visible != false)
                .ToList();

            // Compose a callback that encapsulates both SelectChoice and Next. The option id IS
            // the exit port, so selecting is what tells the engine where to go.
            Action<string> selectChoiceAndAdvance = optionId =>
            {
                context.SelectChoice(optionId);
                arguments.Next();
            };

            var presentationKey = LsdePresentationKey.Next(block);

            _dialoguePresenter.PresentChoiceBlock(
                presentationKey,
                block,
                context.Character,
                context.Actors,
                offeredOptions,
                selectChoiceAndAdvance
            );

            return () => _dialoguePresenter.PresentBlockCleanup(presentationKey, block);
        }
    }
}
