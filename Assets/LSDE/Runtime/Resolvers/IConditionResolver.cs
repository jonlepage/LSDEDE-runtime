using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Evaluates game-state conditions for both choice visibility and condition block pre-evaluation.
    /// The engine handles <c>choice:</c> conditions internally via choice history —
    /// this resolver only receives game-state conditions (inventory, flags, variables, etc.).
    /// </summary>
    public interface IConditionResolver
    {
        /// <summary>
        /// Evaluate a single game-state condition against the current game state.
        /// </summary>
        /// <param name="condition">
        /// The condition to evaluate. Use <see cref="ExportCondition.Key"/>,
        /// <see cref="ExportCondition.Operator"/>, and <see cref="ExportCondition.Value"/>
        /// to determine the result.
        /// </param>
        /// <returns>True if the condition is met, false otherwise.</returns>
        bool EvaluateCondition(ExportCondition condition);
    }
}
