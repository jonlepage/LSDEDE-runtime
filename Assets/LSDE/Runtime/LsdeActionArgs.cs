using System;
using System.Collections.Generic;
using System.Globalization;

namespace LSDE.Runtime
{
    /// <summary>
    /// Reads the argument bag of an <c>ActionCall</c>.
    ///
    /// <para>v2 passes arguments <b>BY NAME</b> — <c>Args["duration"]</c> — where v1 passed a
    /// positional list. Two consequences change how a game must read them:</para>
    ///
    /// <list type="bullet">
    ///   <item><b>An argument can be missing.</b> A parameter the writer left empty is simply
    ///     absent from the bag; it is not present-and-null. In this project's payload,
    ///     <c>moveCharacterAt</c> is called nine times and carries <c>y</c> in five of them and
    ///     <c>absolut</c> in four. So every read needs a default, and "missing" must not mean
    ///     "zero" when zero is a meaningful value.</item>
    ///   <item><b>A number is boxed as <c>long</c> or <c>double</c></b>, depending on whether the
    ///     writer typed <c>2</c> or <c>1.5</c>. A direct <c>(float)</c> or <c>(double)</c> cast on
    ///     a boxed <c>long</c> throws <see cref="InvalidCastException"/> — the classic way to lose
    ///     an afternoon. <see cref="Convert"/> handles every numeric type through IConvertible.</item>
    /// </list>
    ///
    /// <para>Unit note: the <c>delay</c> and <c>timeout</c> NATIVES are milliseconds, but a
    /// function parameter named "duration" is whatever the project decided it means — the engine
    /// passes it through untouched and has no opinion. In this demo they are seconds.</para>
    /// </summary>
    public static class LsdeActionArgs
    {
        /// <summary>
        /// Read a numeric argument, or <paramref name="fallbackValue"/> when it is absent or
        /// cannot be read as a number.
        /// </summary>
        /// <param name="arguments">The call's argument bag.</param>
        /// <param name="argumentName">The parameter name, as declared in the LSDE project.</param>
        /// <param name="fallbackValue">Returned when the argument is missing or unreadable.</param>
        public static float GetSingle(
            IReadOnlyDictionary<string, object> arguments,
            string argumentName,
            float fallbackValue = 0f
        )
        {
            if (!TryGetValue(arguments, argumentName, out var rawValue))
            {
                return fallbackValue;
            }

            if (rawValue is string stringValue)
            {
                return float.TryParse(
                    stringValue,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float parsedValue
                )
                    ? parsedValue
                    : fallbackValue;
            }

            try
            {
                return Convert.ToSingle(rawValue, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return fallbackValue;
            }
        }

        /// <summary>
        /// Read a boolean argument, or <paramref name="fallbackValue"/> when it is absent or
        /// cannot be read as a boolean.
        /// </summary>
        public static bool GetBoolean(
            IReadOnlyDictionary<string, object> arguments,
            string argumentName,
            bool fallbackValue = false
        )
        {
            if (!TryGetValue(arguments, argumentName, out var rawValue))
            {
                return fallbackValue;
            }

            if (rawValue is bool booleanValue)
            {
                return booleanValue;
            }

            if (rawValue is string stringValue)
            {
                return bool.TryParse(stringValue, out bool parsedValue)
                    ? parsedValue
                    : fallbackValue;
            }

            try
            {
                return Convert.ToBoolean(rawValue, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return fallbackValue;
            }
        }

        /// <summary>
        /// Read a string argument, or <paramref name="fallbackValue"/> when it is absent.
        /// A <c>dictionaryKey</c> parameter arrives here: its value is one of the keys declared in
        /// the dictionary the parameter points at.
        /// </summary>
        public static string GetString(
            IReadOnlyDictionary<string, object> arguments,
            string argumentName,
            string fallbackValue = null
        )
        {
            if (!TryGetValue(arguments, argumentName, out var rawValue))
            {
                return fallbackValue;
            }

            return rawValue is string stringValue
                ? stringValue
                : Convert.ToString(rawValue, CultureInfo.InvariantCulture);
        }

        /// <summary>Whether the writer filled this argument in at all.</summary>
        public static bool Has(IReadOnlyDictionary<string, object> arguments, string argumentName)
        {
            return TryGetValue(arguments, argumentName, out _);
        }

        /// <summary>
        /// Describe the bag for a log line: names, values and CLR types, so a surprise
        /// (a <c>long</c> where a <c>double</c> was expected, an absent argument) is visible.
        /// </summary>
        public static string Describe(IReadOnlyDictionary<string, object> arguments)
        {
            if (arguments == null || arguments.Count == 0)
            {
                return "(no arguments)";
            }

            var parts = new List<string>(arguments.Count);
            foreach (var entry in arguments)
            {
                var typeName = entry.Value != null ? entry.Value.GetType().Name : "null";
                parts.Add($"{entry.Key}={entry.Value} ({typeName})");
            }
            return string.Join(", ", parts);
        }

        private static bool TryGetValue(
            IReadOnlyDictionary<string, object> arguments,
            string argumentName,
            out object rawValue
        )
        {
            rawValue = null;

            if (arguments == null || argumentName == null)
            {
                return false;
            }

            return arguments.TryGetValue(argumentName, out rawValue) && rawValue != null;
        }
    }
}
