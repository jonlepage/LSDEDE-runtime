using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// The game's single state evaluator. The engine uses it for two things: tagging option
    /// visibility before a CHOICE, and pre-evaluating the cases of a CONDITION or a ROUTER.
    ///
    /// <para>A test on the reserved <c>choice</c> dictionary never reaches here — the engine
    /// answers those from the scene's own choice history.</para>
    ///
    /// <para>The engine never reads a dictionary itself, never implements an operator and never
    /// knows what <c>carrot</c> holds. It hands over a test and expects true or false.</para>
    /// </summary>
    public interface IConditionResolver
    {
        /// <summary>
        /// Evaluate a single test against the current game state.
        /// </summary>
        /// <param name="test">
        /// What to compare. <see cref="ConditionTest.Dict"/> is the dictionary id
        /// (<c>inventory</c>, <c>party</c>), <see cref="ConditionTest.Entry"/> the entry read in
        /// it, <see cref="ConditionTest.Op"/> the comparison (see <c>ConditionOperator</c>) and
        /// <see cref="ConditionTest.Value"/> the right-hand side — a <c>bool</c>, a number
        /// (<c>long</c> or <c>double</c>) or a <c>string</c>, following the dictionary's own type.
        ///
        /// <para><see cref="ConditionTest.Join"/> is NOT read here: the engine chains the tests
        /// itself, left to right, with no operator precedence.</para>
        /// </param>
        /// <returns>True if the test holds, false otherwise.</returns>
        bool EvaluateCondition(ConditionTest test);
    }
}
