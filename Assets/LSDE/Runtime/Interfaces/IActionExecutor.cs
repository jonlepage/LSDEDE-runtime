using System.Collections;
using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Executes a single game call identified by its <see cref="ActionCall.Fn"/>.
    /// The presenter calls this for each call in an ACTION block, running them
    /// in parallel (Unity equivalent of <c>Promise.all</c> from the TypeScript reference).
    ///
    /// Implementations map <see cref="ActionCall.Fn"/> values to game-specific
    /// coroutines (camera movements, character movements, sound effects, etc.).
    /// Use the generated <c>LsdedeDemoTsBlueprintIds.Functions</c> constants for type-safe
    /// switch/case matching — never retype the strings.
    ///
    /// This interface follows the same pattern as <see cref="ICharacterResolver"/> and
    /// <see cref="IConditionResolver"/> — a Runtime interface with a Demo implementation.
    /// </summary>
    public interface IActionExecutor
    {
        /// <summary>
        /// Execute a single call and yield until it completes.
        /// The returned <see cref="IEnumerator"/> is started as a coroutine by the presenter.
        /// Yield <c>WaitForSeconds</c> for timed effects, or <c>yield break</c> for instant ones.
        /// Throw an exception to signal failure (the presenter catches it and rejects, which
        /// routes the flow to the block's <c>catch</c> port).
        /// </summary>
        /// <param name="call">
        /// The call to execute. <see cref="ActionCall.Fn"/> says which game effect to trigger and
        /// <see cref="ActionCall.Args"/> carries the arguments <b>BY NAME</b>, as declared in the
        /// project's <c>FunctionDefinition.Params</c>.
        ///
        /// <para>Two things to know about that bag: an argument the writer left empty is simply
        /// ABSENT — never assume every declared parameter is there — and a JSON number arrives as
        /// <c>long</c> or <c>double</c> depending on how it was written, so read it with
        /// <c>Convert.ToSingle</c> rather than casting.</para>
        /// </param>
        /// <returns>A coroutine enumerator that yields until the effect is complete.</returns>
        IEnumerator ExecuteAction(ActionCall call);
    }
}
