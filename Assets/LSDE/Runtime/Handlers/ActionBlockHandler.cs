using System;
using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Global handler for ACTION blocks. Presents action details
    /// and resolves via the success path ("then" port).
    /// The game's actual action execution (camera moves, sounds, etc.)
    /// will be implemented in the presenter or a dedicated action executor.
    /// </summary>
    public class ActionBlockHandler
    {
        private readonly IDialoguePresenter _dialoguePresenter;

        /// <summary>
        /// Create a new action block handler.
        /// </summary>
        /// <param name="dialoguePresenter">The presenter that will display action details.</param>
        public ActionBlockHandler(IDialoguePresenter dialoguePresenter)
        {
            _dialoguePresenter =
                dialoguePresenter ?? throw new ArgumentNullException(nameof(dialoguePresenter));
        }

        /// <summary>
        /// Handle an ACTION block dispatched by the LSDEDE runtime.
        /// Composes resolve+next and reject+next callbacks, then delegates execution
        /// to the presenter — the presenter controls when to advance, just like
        /// DIALOG (advanceToNextBlock) and CHOICE (selectChoiceAndAdvance).
        /// </summary>
        /// <param name="arguments">Block handler arguments containing the block, context, and next callback.</param>
        /// <returns>A cleanup action called when the engine leaves this block, or null.</returns>
        public Action HandleActionBlock(BlockHandlerArgs<ActionBlock, IActionContext> arguments)
        {
            var block = arguments.Block;
            var context = arguments.Context;

            // Compose callbacks that encapsulate context resolution and engine advancement.
            // The presenter calls exactly one of these when all actions complete.
            // This mirrors the CHOICE pattern where selectChoiceAndAdvance wraps
            // context.SelectChoice(uuid) + arguments.Next().
            Action resolveAndAdvance = () =>
            {
                context.Resolve();
                arguments.Next();
            };

            Action<object> rejectAndAdvance = (error) =>
            {
                context.Reject(error);
                arguments.Next();
            };

            _dialoguePresenter.PresentActionBlock(block, resolveAndAdvance, rejectAndAdvance);

            return () => _dialoguePresenter.PresentBlockCleanup(block);
        }
    }
}
