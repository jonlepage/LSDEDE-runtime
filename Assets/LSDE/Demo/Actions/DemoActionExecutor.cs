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
    /// to game effects via a switch/case pattern.
    ///
    /// This is the Unity C# equivalent of the TypeScript <c>execute-action.ts</c>
    /// shared module. The switch/case pattern is intentional — it keeps all action
    /// mapping in one place, making it easy for developers to see and extend.
    ///
    /// Camera actions (<c>shakeCamera</c>, <c>moveCameraToLabel</c>) use the
    /// <see cref="CameraFollowController"/> API (pause/resume/shake offset).
    /// Character movement (<c>moveCharacterAt</c>) is still simulated for now.
    ///
    /// Uses <see cref="lsdeActionId"/> constants for compile-time validated matching.
    /// Never use string literals for action IDs.
    /// </summary>
    public class DemoActionExecutor : MonoBehaviour, IActionExecutor
    {
        [SerializeField]
        [Tooltip("The camera follow controller on the main camera. Required for camera actions.")]
        private CameraFollowController _cameraFollowController;

        [SerializeField]
        [Tooltip(
            "The character registry that maps LSDE character IDs to scene GameObjects. "
                + "Required for moveCameraToLabel to find character positions."
        )]
        private DialogueCharacterRegistry _characterRegistry;

        private const string LogPrefix = "[LSDE Action]";

        /// <summary>
        /// Default simulated duration in seconds for actions that don't have
        /// an explicit duration parameter (e.g. <c>moveCharacterAt</c>).
        /// </summary>
        private const float DefaultDurationInSeconds = 0.5f;

        /// <summary>
        /// Intensity scaling factor that converts blueprint intensity values
        /// (designed for 2D pixel space) to Unity 3D world units.
        /// Blueprint uses values like 5, 8, 16 — in 3D these need to be much smaller.
        /// </summary>
        private const float ShakeIntensityWorldScaleFactor = 0.02f;

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
        /// Shake the camera by applying random offsets each frame for the specified duration.
        /// Uses <see cref="CameraFollowController.SetShakeOffset"/> so the shake is additive
        /// on top of both normal follow and paused (command) states.
        /// Params: [0] intensity (number), [1] duration in seconds (number).
        /// </summary>
        /// <param name="parameters">Ordered parameter values from the blueprint action.</param>
        private IEnumerator ExecuteShakeCamera(List<object> parameters)
        {
            var intensity = ConvertParameterToFloat(parameters, 0, "intensity");
            var durationInSeconds = ConvertParameterToFloat(parameters, 1, "duration");

            if (durationInSeconds <= 0)
            {
                durationInSeconds = DefaultDurationInSeconds;
            }

            Debug.Log(
                $"{LogPrefix} shakeCamera — intensity={intensity}, "
                    + $"duration={durationInSeconds}s"
            );

            if (_cameraFollowController == null)
            {
                Debug.LogWarning(
                    $"{LogPrefix} No CameraFollowController assigned — simulating wait only."
                );
                yield return new WaitForSeconds(durationInSeconds);
                yield break;
            }

            // Scale intensity from blueprint 2D values to 3D world units
            float worldIntensity = intensity * ShakeIntensityWorldScaleFactor;
            float elapsedTime = 0f;

            while (elapsedTime < durationInSeconds)
            {
                // Random offset on X and Y axes (Z stays 0 to avoid depth shift)
                var shakeOffset = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f) * worldIntensity,
                    UnityEngine.Random.Range(-1f, 1f) * worldIntensity,
                    0f
                );

                _cameraFollowController.SetShakeOffset(shakeOffset);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Reset shake offset when done
            _cameraFollowController.SetShakeOffset(Vector3.zero);

            Debug.Log($"{LogPrefix} shakeCamera — complete");
        }

        /// <summary>
        /// Smoothly move the camera to focus on a character identified by label.
        /// Pauses the camera follow, lerps to the character's position (+ camera offset),
        /// then leaves follow paused so the camera stays on the target until the engine
        /// naturally resumes (e.g. next block focuses on another character, or scene exits).
        ///
        /// Uses ease-in-out cubic easing for smooth cinematic movement,
        /// matching the TS <c>moveCameraToPosition</c> with time-based mode.
        ///
        /// Params: [0] label (string, character ID), [1] duration in seconds (number).
        /// </summary>
        /// <param name="parameters">Ordered parameter values from the blueprint action.</param>
        private IEnumerator ExecuteMoveCameraToLabel(List<object> parameters)
        {
            var label = parameters.Count > 0 ? parameters[0]?.ToString() : "unknown";
            var durationInSeconds = ConvertParameterToFloat(parameters, 1, "duration");

            if (durationInSeconds <= 0)
            {
                durationInSeconds = DefaultDurationInSeconds;
            }

            Debug.Log(
                $"{LogPrefix} moveCameraToLabel — label={label}, "
                    + $"duration={durationInSeconds}s"
            );

            if (_cameraFollowController == null || _characterRegistry == null)
            {
                Debug.LogWarning(
                    $"{LogPrefix} Missing CameraFollowController or CharacterRegistry "
                        + "— simulating wait only."
                );
                yield return new WaitForSeconds(durationInSeconds);
                yield break;
            }

            // Find the character's position in the scene
            var characterMarker = _characterRegistry.FindMarkerByCharacterId(label);
            if (characterMarker == null)
            {
                Debug.LogWarning(
                    $"{LogPrefix} Character '{label}' not found in scene — simulating wait only."
                );
                yield return new WaitForSeconds(durationInSeconds);
                yield break;
            }

            // Target position = character ground position + camera offset
            // Use Y=0 (ground level) to match the follow controller's behavior
            Vector3 characterGroundPosition = new Vector3(
                characterMarker.transform.position.x,
                0f,
                characterMarker.transform.position.z
            );
            Vector3 targetCameraPosition =
                characterGroundPosition + _cameraFollowController.CameraOffset;

            // Pause follow so the follow controller doesn't fight our position changes
            _cameraFollowController.PauseFollow();

            Vector3 startCameraPosition = _cameraFollowController.transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < durationInSeconds)
            {
                elapsedTime += Time.deltaTime;
                float linearProgress = Mathf.Clamp01(elapsedTime / durationInSeconds);

                // Ease-in-out cubic: smooth acceleration and deceleration
                // t < 0.5: 4t^3 (ease in)
                // t >= 0.5: 1 - (-2t+2)^3/2 (ease out)
                float easedProgress = EaseInOutCubic(linearProgress);

                _cameraFollowController.transform.position = Vector3.Lerp(
                    startCameraPosition,
                    targetCameraPosition,
                    easedProgress
                );

                yield return null;
            }

            // Snap to final position
            _cameraFollowController.transform.position = targetCameraPosition;

            // Resume follow so the camera smoothly catches back to the player
            _cameraFollowController.ResumeFollow();

            Debug.Log($"{LogPrefix} moveCameraToLabel — complete");
        }

        /// <summary>
        /// Simulate moving a character to a position.
        /// Params: [0] characterId (string), [1] offsetX (number),
        ///         [2] offsetY (number, optional), [3] isAbsolute (boolean, optional).
        /// Currently logs and waits — real character movement is a future enhancement.
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

            yield return new WaitForSeconds(DefaultDurationInSeconds);

            Debug.Log($"{LogPrefix} moveCharacterAt — complete");
        }

        /// <summary>
        /// Ease-in-out cubic easing function for smooth camera movements.
        /// Accelerates during the first half, decelerates during the second half.
        /// Matches the TS <c>easeInOutCubic</c> used in <c>moveCameraToPosition</c>.
        /// </summary>
        /// <param name="progress">Linear progress from 0 to 1.</param>
        /// <returns>Eased progress value from 0 to 1.</returns>
        private static float EaseInOutCubic(float progress)
        {
            if (progress < 0.5f)
            {
                return 4f * progress * progress * progress;
            }

            float shifted = -2f * progress + 2f;
            return 1f - shifted * shifted * shifted / 2f;
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
