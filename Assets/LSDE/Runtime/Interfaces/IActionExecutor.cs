using System.Collections;
using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Executes a single game action identified by its <see cref="ExportAction.ActionId"/>.
    /// The presenter calls this for each action in an ACTION block, running them
    /// in parallel (Unity equivalent of <c>Promise.all</c> from the TypeScript reference).
    ///
    /// Implementations map <see cref="ExportAction.ActionId"/> values to game-specific
    /// coroutines (camera movements, character movements, sound effects, etc.).
    /// Use <see cref="lsdeActionId"/> constants for type-safe switch/case matching.
    ///
    /// This interface follows the same pattern as <see cref="ICharacterResolver"/> and
    /// <see cref="IConditionResolver"/> — a Runtime interface with a Demo implementation.
    /// </summary>
    public interface IActionExecutor
    {
        /// <summary>
        /// Execute a single action and yield until it completes.
        /// The returned <see cref="IEnumerator"/> is started as a coroutine by the presenter.
        /// Yield <c>WaitForSeconds</c> for timed actions, or <c>yield break</c> for instant ones.
        /// Throw an exception to signal failure (the presenter will catch it and reject).
        /// </summary>
        /// <param name="action">
        /// The action to execute. Use <see cref="ExportAction.ActionId"/> to determine
        /// which game effect to trigger, and <see cref="ExportAction.Params"/> for parameters.
        /// Parameter order matches the <see cref="ActionSignature"/> definition in the LSDE project.
        /// </param>
        /// <returns>A coroutine enumerator that yields until the action effect is complete.</returns>
        IEnumerator ExecuteAction(ExportAction action);
    }
}
