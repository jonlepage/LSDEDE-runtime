using System;
using System.Globalization;
using LSDE.Runtime;
using LsdeDialogEngine;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Demo implementation of <see cref="IConditionResolver"/> that answers blueprint tests from
    /// the game state held in <see cref="DemoGameState"/>.
    ///
    /// <para>A v2 test is already split for you: <c>Dict</c> is the dictionary id, <c>Entry</c>
    /// the entry read in it, <c>Op</c> the comparison and <c>Value</c> the right-hand side. There
    /// is no dotted key to parse any more — v1 sent <c>"inventory.carrot"</c> as one string and
    /// every game had to split it.</para>
    ///
    /// <para>This payload asks two dictionaries:</para>
    /// <list type="bullet">
    ///   <item><c>inventory</c> — a number, e.g. <c>carrot &gt;= 1</c></item>
    ///   <item><c>party</c> — a boolean, e.g. <c>l2 == true</c> ("is l2 with us?")</item>
    /// </list>
    ///
    /// <para>Anything else is read as a plain game variable keyed <c>dict.entry</c>, which reads 0
    /// when unset — so a test nobody can answer fails rather than opening a branch. That is also
    /// what the engine does for routing: an unanswerable test is false. Option visibility is the
    /// exception, and the engine handles it: there, unknown stays unknown, because saying false
    /// about a question nobody could answer would HIDE an answer.</para>
    ///
    /// <para>One thing worth keeping in your own game: <b>Value is a boxed JSON value</b> — a
    /// <c>bool</c>, a <c>long</c>, a <c>double</c> or a <c>string</c> depending on how the writer
    /// typed it — so compare through <c>Convert</c>, never with a direct cast, which throws on a
    /// boxed <c>long</c>.</para>
    ///
    /// <para>Tests on the reserved <c>choice</c> dictionary never arrive here: the engine answers
    /// those from the scene's own choice history.</para>
    /// </summary>
    public class DemoConditionResolver : MonoBehaviour, IConditionResolver
    {
        private const string LogPrefix = "[LSDE Condition]";

        [SerializeField]
        [Tooltip(
            "Reference to the game state that holds inventory, party, and variables. "
                + "The resolver queries this state to answer blueprint tests."
        )]
        private DemoGameState _gameState;

        /// <inheritdoc />
        public bool EvaluateCondition(ConditionTest test)
        {
            if (_gameState == null)
            {
                Debug.LogError(
                    $"{LogPrefix} DemoGameState reference is missing! "
                        + "Assign it in the Inspector. Answering false."
                );
                return false;
            }

            bool result = EvaluateByDictionary(test);

            Debug.Log(
                $"{LogPrefix} {test.Dict}.{test.Entry} {test.Op} {Describe(test.Value)} "
                    + $"→ {result}"
            );

            return result;
        }

        /// <summary>
        /// Route the test to the right store, by dictionary id.
        /// </summary>
        private bool EvaluateByDictionary(ConditionTest test)
        {
            switch (test.Dict)
            {
                case LsdedeDemoTsBlueprintIds.Dictionaries.inventory:
                {
                    int quantity = _gameState.GetItemQuantity(test.Entry);
                    return CompareNumbers(quantity, test.Op, test.Value, test);
                }

                case LsdedeDemoTsBlueprintIds.Dictionaries.party:
                {
                    bool isMember = _gameState.IsInParty(test.Entry);
                    return CompareBooleans(isMember, test.Op, test.Value, test);
                }

                default:
                {
                    // Any dictionary this demo does not model specially is read as a plain
                    // game variable, keyed "dict.entry". An unset variable reads 0, so a
                    // test like `>= 1` naturally fails instead of opening a branch.
                    float variableValue = _gameState.GetVariable($"{test.Dict}.{test.Entry}");
                    return CompareNumbers(variableValue, test.Op, test.Value, test);
                }
            }
        }

        /// <summary>
        /// Compare a boolean game value against the test's value.
        /// Only equality makes sense on a boolean dictionary; anything else is a design mistake
        /// in the blueprint and answers false rather than guessing.
        /// </summary>
        private static bool CompareBooleans(
            bool currentValue,
            string comparisonOperator,
            object expectedValue,
            ConditionTest test
        )
        {
            if (!TryConvertToBoolean(expectedValue, out bool expected))
            {
                Debug.LogWarning(
                    $"{LogPrefix} Cannot read {Describe(expectedValue)} as a boolean "
                        + $"in {test.Dict}.{test.Entry}. Answering false."
                );
                return false;
            }

            switch (comparisonOperator)
            {
                case ConditionOperator.Equals:
                    return currentValue == expected;
                case ConditionOperator.NotEquals:
                    return currentValue != expected;
                default:
                    Debug.LogWarning(
                        $"{LogPrefix} Operator \"{comparisonOperator}\" makes no sense on the "
                            + $"boolean dictionary \"{test.Dict}\". Answering false."
                    );
                    return false;
            }
        }

        /// <summary>
        /// Compare a numeric game value against the test's value, for every operator.
        /// </summary>
        private static bool CompareNumbers(
            float currentValue,
            string comparisonOperator,
            object expectedValue,
            ConditionTest test
        )
        {
            if (!TryConvertToSingle(expectedValue, out float expected))
            {
                Debug.LogWarning(
                    $"{LogPrefix} Cannot read {Describe(expectedValue)} as a number "
                        + $"in {test.Dict}.{test.Entry}. Answering false."
                );
                return false;
            }

            switch (comparisonOperator)
            {
                case ConditionOperator.Equals:
                    return Mathf.Approximately(currentValue, expected);
                case ConditionOperator.NotEquals:
                    return !Mathf.Approximately(currentValue, expected);
                case ConditionOperator.LessThan:
                    return currentValue < expected;
                case ConditionOperator.LessOrEqual:
                    return currentValue <= expected;
                case ConditionOperator.GreaterThan:
                    return currentValue > expected;
                case ConditionOperator.GreaterOrEqual:
                    return currentValue >= expected;
                default:
                    Debug.LogWarning(
                        $"{LogPrefix} Unknown operator \"{comparisonOperator}\" "
                            + $"in {test.Dict}.{test.Entry}. Answering false."
                    );
                    return false;
            }
        }

        /// <summary>
        /// Read a boxed JSON value as a boolean. Newtonsoft hands back a real <c>bool</c> for
        /// <c>true</c>/<c>false</c>, but a writer may also have typed the string "true".
        /// </summary>
        private static bool TryConvertToBoolean(object value, out bool result)
        {
            result = false;

            if (value is bool booleanValue)
            {
                result = booleanValue;
                return true;
            }

            if (value is string stringValue)
            {
                return bool.TryParse(stringValue, out result);
            }

            try
            {
                result = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Read a boxed JSON value as a float. An integer arrives as <c>long</c> and a decimal as
        /// <c>double</c>, so a direct <c>(float)</c> cast throws on the first one.
        /// </summary>
        private static bool TryConvertToSingle(object value, out float result)
        {
            result = 0f;

            if (value is string stringValue)
            {
                return float.TryParse(
                    stringValue,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out result
                );
            }

            try
            {
                result = Convert.ToSingle(value, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Describe a boxed value with its CLR type, for readable logs.</summary>
        private static string Describe(object value)
        {
            if (value == null)
            {
                return "null";
            }
            return $"{value} ({value.GetType().Name})";
        }
    }
}
