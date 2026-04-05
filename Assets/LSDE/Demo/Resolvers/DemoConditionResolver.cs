using LSDE.Runtime;
using LsdeDialogEngine;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Demo implementation of <see cref="IConditionResolver"/> that simulates
    /// game state evaluation with hardcoded values.
    /// Ported from playground.ts GameOnResolveCondition function.
    /// In a real game, this would query inventory, flags, quest states, etc.
    /// </summary>
    public class DemoConditionResolver : IConditionResolver
    {
        /// <inheritdoc />
        public bool EvaluateCondition(ExportCondition condition)
        {
            Debug.Log(
                $"[LSDE] ResolveCondition: key={condition.Key} "
                    + $"{condition.Operator} {condition.Value}"
            );

            var keyParts = condition.Key.Split('.');
            var target = keyParts.Length > 0 ? keyParts[0] : "";
            var key = keyParts.Length > 1 ? keyParts[1] : "";

            switch (target)
            {
                case "VariableGlobal":
                    switch (key)
                    {
                        case "key1":
                            return true;
                        case "key2":
                            return false;
                    }
                    break;
            }

            // Default: conditions pass unless explicitly handled above
            return true;
        }
    }
}
