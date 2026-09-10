using System;
using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Global handler for ACTION blocks. Composes resolve+next and reject+next callbacks,
    /// then delegates execution to the presenter — the presenter controls when to advance,
    /// just like DIALOG and CHOICE.
    /// </summary>
    public class ActionBlockHandler
    {
        private readonly IDialoguePresenter _dialoguePresenter;

        /// <summary>
        /// Create a new action block handler.
        /// </summary>
        /// <param name="dialoguePresenter">The presenter that will run the calls.</param>
        public ActionBlockHandler(IDialoguePresenter dialoguePresenter)
        {
            _dialoguePresenter =
                dialoguePresenter ?? throw new ArgumentNullException(nameof(dialoguePresenter));
        }

        /// <summary>
        /// Handle an ACTION block dispatched by the LSDEDE runtime.
        /// </summary>
        /// <param name="arguments">Block, context, and the Next callback.</param>
        /// <returns>A cleanup action called when the engine LEAVES this block.</returns>
        public Action HandleActionBlock(BlockHandlerArgs<BlueprintBlock, IActionContext> arguments)
        {
            var block = arguments.Block;
            var context = arguments.Context;

            // Exactly one of these fires when every call is over. Resolve leaves by "then";
            // Reject leaves by "catch", or by "then" when the designer drew no error branch —
            // stopping the scene on an unhandled failure would strand the player mid-dialogue.
            Action resolveAndAdvance = () =>
            {
                context.Resolve();
                arguments.Next();
            };

            Action<object> rejectAndAdvance = error =>
            {
                context.Reject(error);
                arguments.Next();
            };

            var presentationKey = LsdePresentationKey.Next(block);

            _dialoguePresenter.PresentActionBlock(
                presentationKey,
                block,
                context.Calls,
                resolveAndAdvance,
                rejectAndAdvance
            );

            return () => _dialoguePresenter.PresentBlockCleanup(presentationKey, block);
        }
    }
}
