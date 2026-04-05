using System;
using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Global handler for DIALOG blocks. Resolves the active character,
    /// extracts localized text, and delegates presentation to <see cref="IDialoguePresenter"/>.
    /// Never contains rendering logic directly.
    /// </summary>
    public class DialogBlockHandler
    {
        private readonly IDialoguePresenter _dialoguePresenter;

        /// <summary>
        /// Create a new dialog block handler.
        /// </summary>
        /// <param name="dialoguePresenter">The presenter that will display dialogue content.</param>
        public DialogBlockHandler(IDialoguePresenter dialoguePresenter)
        {
            _dialoguePresenter =
                dialoguePresenter ?? throw new ArgumentNullException(nameof(dialoguePresenter));
        }

        /// <summary>
        /// Handle a DIALOG block dispatched by the LSDEDE runtime.
        /// Matches the <see cref="BlockHandler{DialogBlock, IDialogContext}"/> delegate signature.
        /// </summary>
        /// <param name="arguments">Block handler arguments containing the block, context, and next callback.</param>
        /// <returns>A cleanup action called when the engine leaves this block, or null.</returns>
        public Action HandleDialogBlock(BlockHandlerArgs<DialogBlock, IDialogContext> arguments)
        {
            var block = arguments.Block;
            var context = arguments.Context;
            var character = context.Character;
            var localizedText = LsdeUtils.GetLocalizedText(block.DialogueText);

            // When portPerCharacter is enabled, the handler must tell the engine
            // which character port to follow for routing to the next block.
            if (block.NativeProperties?.PortPerCharacter == true && character != null)
            {
                context.ResolveCharacterPort(character.Uuid);
            }

            // Pass Next to the presenter — the presenter decides when to advance.
            // Console presenter calls it immediately; UI presenter waits for player click.
            _dialoguePresenter.PresentDialogueBlock(
                block,
                character,
                localizedText,
                arguments.Next
            );

            return () => _dialoguePresenter.PresentBlockCleanup(block);
        }
    }
}
