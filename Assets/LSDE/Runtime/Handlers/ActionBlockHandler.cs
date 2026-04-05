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
        /// Matches the <see cref="BlockHandler{ActionBlock, IActionContext}"/> delegate signature.
        /// </summary>
        /// <param name="arguments">Block handler arguments containing the block, context, and next callback.</param>
        /// <returns>A cleanup action called when the engine leaves this block, or null.</returns>
        public Action HandleActionBlock(BlockHandlerArgs<ActionBlock, IActionContext> arguments)
        {
            var block = arguments.Block;
            var context = arguments.Context;

            _dialoguePresenter.PresentActionBlock(block);

            // Resolve follows the "then" port (success path).
            // Use context.Reject(error) for the "catch" port if action fails.
            context.Resolve();
            arguments.Next();

            return () => _dialoguePresenter.PresentBlockCleanup(block);
        }
    }
}
