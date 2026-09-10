using System;
using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Global handler for DIALOG blocks. Reads the resolved actor and the localized text,
    /// then delegates presentation to <see cref="IDialoguePresenter"/>.
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
        /// </summary>
        /// <param name="arguments">Block, context, and the Next callback.</param>
        /// <returns>A cleanup action called when the engine LEAVES this block.</returns>
        public Action HandleDialogBlock(BlockHandlerArgs<BlueprintBlock, IDialogContext> arguments)
        {
            var block = arguments.Block;
            var context = arguments.Context;
            var character = context.Character;

            // The engine hands the RAW string over and never looks inside it: {{@l3}} and the
            // like are the game's own markers, in the game's own keys. LsdeText is where this
            // game answers both "which language" and "what does {{@l3}} mean".
            var localizedText = LsdeText.Localized(block.Text);

            // The natives live in block.Props next to the writer's own properties;
            // GetNativeProperties is what tells them apart.
            var nativeProperties = LsdeUtils.GetNativeProperties(block);

            // With portPerCharacter the block grows one EXIT port per actor card id, and the game
            // says which one to take. `out` stays the fallback for an actor with no port drawn.
            if (nativeProperties.PortPerCharacter == true && character != null)
            {
                context.ResolveCharacterPort(character.Id);
            }

            // One block can be on screen twice at once (inPortPerCharacter), so visuals are
            // keyed by DISPATCH, not by block id. The cleanup below captures the same key.
            var presentationKey = LsdePresentationKey.Next(block);

            // Pass Next to the presenter — the presenter decides when to advance:
            // immediately, on a player click, or when a `timeout` runs out after the reveal.
            _dialoguePresenter.PresentDialogueBlock(
                presentationKey,
                block,
                character,
                localizedText,
                arguments.Next
            );

            return () => _dialoguePresenter.PresentBlockCleanup(presentationKey, block);
        }
    }
}
