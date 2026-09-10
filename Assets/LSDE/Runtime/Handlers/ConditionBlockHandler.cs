using System;
using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Global handler for CONDITION blocks — a logging hook, and nothing more.
    ///
    /// <para>Once <c>OnResolveCondition</c> is installed the engine has already evaluated every
    /// case and already knows the exit port, so <c>OnCondition</c> becomes optional: the handler
    /// exists to let a game watch the routing, or to override it with
    /// <c>context.Resolve( port )</c> where the port is a NAME — <c>out</c>, <c>default</c>, or a
    /// case port like <c>K1</c>. This demo watches and never overrides.</para>
    ///
    /// <para>A condition has exactly two modes. Without <c>portPerCase</c>, every case must hold
    /// → <c>out</c>, otherwise <c>default</c>. With <c>portPerCase: true</c>, the first case that
    /// holds takes its own port. The third mode of v1 — the dispatcher, which fired every matching
    /// case at once — is gone from the condition block: what replaced it is the ROUTER, a block
    /// type of its own with no handler at all. See <see cref="RouterBlockObserver"/>.</para>
    /// </summary>
    public class ConditionBlockHandler
    {
        private readonly IDialoguePresenter _dialoguePresenter;

        /// <summary>
        /// Create a new condition block handler.
        /// </summary>
        /// <param name="dialoguePresenter">The presenter that will report the evaluation.</param>
        public ConditionBlockHandler(IDialoguePresenter dialoguePresenter)
        {
            _dialoguePresenter =
                dialoguePresenter ?? throw new ArgumentNullException(nameof(dialoguePresenter));
        }

        /// <summary>
        /// Handle a CONDITION block dispatched by the LSDEDE runtime.
        /// </summary>
        /// <param name="arguments">Block, context, and the Next callback.</param>
        /// <returns>Null — a condition renders nothing, so there is nothing to clean up.</returns>
        public Action HandleConditionBlock(
            BlockHandlerArgs<BlueprintBlock, IConditionContext> arguments
        )
        {
            _dialoguePresenter.PresentConditionBlock(arguments.Block, arguments.Context.Cases);

            // No Resolve call: the port is already picked. Calling it would OVERRIDE the engine.
            arguments.Next();

            return null;
        }
    }
}
