using System.Globalization;
using LSDE.Runtime;
using LsdeDialogEngine;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Demo implementation of <see cref="IConditionResolver"/> that evaluates blueprint
    /// conditions against the game state held in <see cref="DemoGameState"/>.
    ///
    /// Ported from the TypeScript reference <c>evaluate-game-condition.ts</c>.
    /// Each condition has a dot-separated key (e.g. <c>inventory.carrot</c>),
    /// an operator (e.g. <c>&gt;=</c>), and a target value (e.g. <c>1</c>).
    /// The first segment of the key identifies the dictionary group:
    /// <list type="bullet">
    ///   <item><c>inventory</c> — numeric quantity check via <see cref="DemoGameState.GetItemQuantity"/></item>
    ///   <item><c>party</c> — boolean membership check via <see cref="DemoGameState.IsInParty"/></item>
    ///   <item>default — generic numeric variable via <see cref="DemoGameState.GetVariable"/></item>
    /// </list>
    ///
    /// In a real game, replace <see cref="DemoGameState"/> with your own game state system.
    /// The pattern stays the same: split the key, dispatch to the right store, compare.
    /// </summary>
    public class DemoConditionResolver : MonoBehaviour, IConditionResolver
    {
        private const string LogPrefix = "[LSDE Condition]";

        [SerializeField]
        [Tooltip(
            "Reference to the game state that holds inventory, party, and variables. "
                + "The resolver queries this state to evaluate blueprint conditions."
        )]
        private DemoGameState _gameState;

        /// <inheritdoc />
        public bool EvaluateCondition(ExportCondition condition)
        {
            Debug.Log(
                $"{LogPrefix} Evaluating: {condition.Key} {condition.Operator} {condition.Value}"
            );

            if (_gameState == null)
            {
                Debug.LogError(
                    $"{LogPrefix} DemoGameState reference is missing! "
                        + "Assign it in the Inspector. Returning true by default."
                );
                return true;
            }

            // Split "inventory.carrot" → dictionaryGroup = "inventory", itemKey = "carrot"
            int dotIndex = condition.Key.IndexOf('.');
            string dictionaryGroup = dotIndex >= 0 ? condition.Key.Substring(0, dotIndex) : "";
            string itemKey = dotIndex >= 0 ? condition.Key.Substring(dotIndex + 1) : condition.Key;

            bool result = EvaluateByDictionaryGroup(
                dictionaryGroup,
                itemKey,
                condition.Key,
                condition.Operator,
                condition.Value
            );

            Debug.Log($"{LogPrefix}   → result: {result}");

            return result;
        }

        /// <summary>
        /// Dispatch the condition evaluation to the appropriate game state store
        /// based on the dictionary group extracted from the condition key.
        /// </summary>
        private bool EvaluateByDictionaryGroup(
            string dictionaryGroup,
            string itemKey,
            string fullKey,
            string comparisonOperator,
            string targetValueString
        )
        {
            switch (dictionaryGroup)
            {
                case "inventory":
                {
                    int quantity = _gameState.GetItemQuantity(itemKey);
                    Debug.Log(
                        $"{LogPrefix}   inventory.{itemKey} = {quantity} (checking {comparisonOperator} {targetValueString})"
                    );
                    return EvaluateNumericComparison(
                        quantity,
                        comparisonOperator,
                        targetValueString,
                        fullKey
                    );
                }

                case "party":
                {
                    bool isMember = _gameState.IsInParty(itemKey);
                    bool expectedTrue = targetValueString == "true" || targetValueString == "1";
                    Debug.Log(
                        $"{LogPrefix}   party.{itemKey} = {isMember} (expected {expectedTrue})"
                    );

                    switch (comparisonOperator)
                    {
                        case "=":
                        case "==":
                            return isMember == expectedTrue;
                        case "!=":
                            return isMember != expectedTrue;
                        default:
                            Debug.LogWarning(
                                $"{LogPrefix} Unknown party operator: \"{comparisonOperator}\" in key \"{fullKey}\""
                            );
                            return false;
                    }
                }

                default:
                {
                    // Generic variable lookup — uses the full key (e.g. "variables.score")
                    float variableValue = _gameState.GetVariable(fullKey);
                    Debug.Log(
                        $"{LogPrefix}   variable {fullKey} = {variableValue} (checking {comparisonOperator} {targetValueString})"
                    );
                    return EvaluateNumericComparison(
                        variableValue,
                        comparisonOperator,
                        targetValueString,
                        fullKey
                    );
                }
            }
        }

        /// <summary>
        /// Compare a numeric game value against a target using the specified operator.
        /// Supports: <c>&gt;=</c>, <c>&gt;</c>, <c>&lt;=</c>, <c>&lt;</c>, <c>==</c>/<c>=</c>, <c>!=</c>.
        /// Blueprint values are always strings (e.g. <c>"1"</c>), parsed with invariant culture
        /// to avoid locale issues (dot-decimal format).
        /// </summary>
        /// <param name="currentValue">The current game state value.</param>
        /// <param name="comparisonOperator">The comparison operator from the blueprint.</param>
        /// <param name="targetValueString">The target value as a string from the blueprint.</param>
        /// <param name="conditionKey">The full condition key, for warning messages.</param>
        /// <returns>True if the comparison holds, false otherwise.</returns>
        private static bool EvaluateNumericComparison(
            float currentValue,
            string comparisonOperator,
            string targetValueString,
            string conditionKey
        )
        {
            if (
                !float.TryParse(
                    targetValueString,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float targetValue
                )
            )
            {
                Debug.LogWarning(
                    $"{LogPrefix} Cannot parse target value \"{targetValueString}\" as float "
                        + $"in condition \"{conditionKey}\". Returning false."
                );
                return false;
            }

            switch (comparisonOperator)
            {
                case ">=":
                    return currentValue >= targetValue;
                case ">":
                    return currentValue > targetValue;
                case "<=":
                    return currentValue <= targetValue;
                case "<":
                    return currentValue < targetValue;
                case "=":
                case "==":
                    return Mathf.Approximately(currentValue, targetValue);
                case "!=":
                    return !Mathf.Approximately(currentValue, targetValue);
                default:
                    Debug.LogWarning(
                        $"{LogPrefix} Unknown operator: \"{comparisonOperator}\" "
                            + $"in condition \"{conditionKey}\". Returning false."
                    );
                    return false;
            }
        }
    }
}
