using System;
using System.Collections;
using System.Collections.Generic;
using LSDE.Runtime;
using LsdeDialogEngine;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Demo implementation of <see cref="IActionExecutor"/> that maps action IDs
    /// to simulated game effects via a switch/case pattern.
    ///
    /// Each action logs its parameters and waits for the specified duration
    /// to simulate async work (camera shake, camera movement, character movement).
    /// In a real game, replace the simulated waits with actual game effect calls.
    ///
    /// This is the Unity C# equivalent of the TypeScript <c>execute-action.ts</c>
    /// shared module. The switch/case pattern is intentional — it keeps all action
    /// mapping in one place, making it easy for developers to see and extend.
    ///
    /// Uses <see cref="lsdeActionId"/> constants for compile-time validated matching.
    /// Never use string literals for action IDs.
    /// </summary>
    public class DemoActionExecutor : MonoBehaviour, IActionExecutor
    {
        private const string LogPrefix = "[LSDE Action]";

        /// <summary>
        /// Default simulated duration in seconds when the action's parameters
        /// do not specify an explicit duration.
        /// </summary>
        private const float DefaultSimulatedDurationInSeconds = 0.5f;

        /// <inheritdoc />
        public IEnumerator ExecuteAction(ExportAction action)
        {
            switch (action.ActionId)
            {
                case lsdeActionId.shakeCamera:
                    yield return ExecuteShakeCamera(action.Params);
                    break;

                case lsdeActionId.moveCameraToLabel:
                    yield return ExecuteMoveCameraToLabel(action.Params);
                    break;

                case lsdeActionId.moveCharacterAt:
                    yield return ExecuteMoveCharacterAt(action.Params);
                    break;

                default:
                    Debug.LogWarning(
                        $"{LogPrefix} Unknown action ID '{action.ActionId}'. Skipping."
                    );
                    yield break;
            }
        }

        /// <summary>
        /// Simulate a camera shake effect.
        /// Params: [0] intensity (number), [1] duration in seconds (number).
        /// In a real game, this would apply screen shake to the main camera.
        /// </summary>
        /// <param name="parameters">Ordered parameter values from the blueprint action.</param>
        private IEnumerator ExecuteShakeCamera(List<object> parameters)
        {
            var intensity = ConvertParameterToFloat(parameters, 0, "intensity");
            var durationInSeconds = ConvertParameterToFloat(parameters, 1, "duration");

            Debug.Log(
                $"{LogPrefix} shakeCamera — intensity={intensity}, "
                    + $"duration={durationInSeconds}s"
            );

            yield return new WaitForSeconds(
                durationInSeconds > 0 ? durationInSeconds : DefaultSimulatedDurationInSeconds
            );

            Debug.Log($"{LogPrefix} shakeCamera — complete");
        }

        /// <summary>
        /// Simulate moving the camera to focus on a labeled target.
        /// Params: [0] label (string, typically a character ID), [1] duration in seconds (number).
        /// In a real game, this would smoothly pan the camera to the target position.
        /// </summary>
        /// <param name="parameters">Ordered parameter values from the blueprint action.</param>
        private IEnumerator ExecuteMoveCameraToLabel(List<object> parameters)
        {
            var label = parameters.Count > 0 ? parameters[0]?.ToString() : "unknown";
            var durationInSeconds = ConvertParameterToFloat(parameters, 1, "duration");

            Debug.Log(
                $"{LogPrefix} moveCameraToLabel — label={label}, "
                    + $"duration={durationInSeconds}s"
            );

            yield return new WaitForSeconds(
                durationInSeconds > 0 ? durationInSeconds : DefaultSimulatedDurationInSeconds
            );

            Debug.Log($"{LogPrefix} moveCameraToLabel — complete");
        }

        /// <summary>
        /// Simulate moving a character to a position.
        /// Params: [0] characterId (string), [1] offsetX (number),
        ///         [2] offsetY (number, optional), [3] isAbsolute (boolean, optional).
        /// In a real game, this would animate the character walking/moving to the target.
        /// </summary>
        /// <param name="parameters">Ordered parameter values from the blueprint action.</param>
        private IEnumerator ExecuteMoveCharacterAt(List<object> parameters)
        {
            var characterId = parameters.Count > 0 ? parameters[0]?.ToString() : "unknown";
            var offsetX = ConvertParameterToFloat(parameters, 1, "offsetX");
            var offsetY = ConvertParameterToFloat(parameters, 2, "offsetY");
            var isAbsolute = parameters.Count > 3 && parameters[3] is bool boolValue && boolValue;

            Debug.Log(
                $"{LogPrefix} moveCharacterAt — character={characterId}, "
                    + $"offsetX={offsetX}, offsetY={offsetY}, absolute={isAbsolute}"
            );

            yield return new WaitForSeconds(DefaultSimulatedDurationInSeconds);

            Debug.Log($"{LogPrefix} moveCharacterAt — complete");
        }

        /// <summary>
        /// Safely extract a float parameter by index from the action's parameter list.
        /// Handles JSON deserialization quirks where Newtonsoft produces <c>long</c>
        /// for integers and <c>double</c> for decimals. Direct <c>(float)</c> casting
        /// on a boxed <c>long</c> throws <see cref="InvalidCastException"/>.
        /// <see cref="Convert.ToSingle(object)"/> handles all numeric types correctly.
        /// </summary>
        /// <param name="parameters">The action's parameter list.</param>
        /// <param name="index">The position of the parameter to extract.</param>
        /// <param name="parameterName">
        /// Human-readable name of the parameter, used in warning messages
        /// when the value is missing or cannot be converted.
        /// </param>
        /// <returns>The parameter value as a float, or 0 if missing or unconvertible.</returns>
        private static float ConvertParameterToFloat(
            List<object> parameters,
            int index,
            string parameterName
        )
        {
            if (parameters == null || index >= parameters.Count || parameters[index] == null)
            {
                return 0f;
            }

            try
            {
                return Convert.ToSingle(parameters[index]);
            }
            catch (Exception)
            {
                Debug.LogWarning(
                    $"{LogPrefix} Cannot convert parameter '{parameterName}' "
                        + $"value '{parameters[index]}' to float. Defaulting to 0."
                );
                return 0f;
            }
        }
    }
}
