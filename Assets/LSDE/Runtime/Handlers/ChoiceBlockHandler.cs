using System;
using System.Collections.Generic;
using System.Linq;
using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Global handler for CHOICE blocks. Filters visible choices and delegates
    /// presentation and selection to <see cref="IDialoguePresenter"/>.
    /// The presenter receives a composed callback that encapsulates both
    /// SelectChoice and Next — it decides when to invoke it (immediately or on player input).
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

            // Compose a callback that encapsulates both SelectChoice and Next.
            // The presenter calls this with the chosen UUID when the player decides.
            // Console presenter calls it immediately; UI presenter waits for button click.
            Action<string> selectChoiceAndAdvance = (choiceUuid) =>
            {
                context.SelectChoice(choiceUuid);
                arguments.Next();
            };

            _dialoguePresenter.PresentChoiceBlock(
                block,
                context.Character,
                visibleChoices,
                selectChoiceAndAdvance
            );

            return () => _dialoguePresenter.PresentBlockCleanup(block);
        }
    }
}
