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
    /// Character movement (<c>moveCharacterAt</c>) uses <see cref="CharacterMovementController"/>.
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
                + "Required for moveCameraToLabel and moveCharacterAt."
        )]
        private DialogueCharacterRegistry _characterRegistry;

        [Header("Camera Settings")]
        [SerializeField]
        [Tooltip(
            "The LSDE character ID of the player character. When moveCameraToLabel targets "
                + "this character, the camera resumes following after arriving. "
                + "For other targets, the camera stays on the target until the next command."
        )]
        private string _playerCharacterId = lsdeCharacter.l4;

        [SerializeField]
        [Tooltip(
            "Scaling factor that converts blueprint shake intensity values "
                + "(designed for 2D pixel space) to Unity 3D world units. "
                + "Blueprint uses values like 5, 8, 16 — in 3D these need to be smaller."
        )]
        private float _shakeIntensityWorldScaleFactor = 0.02f;

        [SerializeField]
        [Tooltip(
            "Optional reference to the party follow controller. When assigned, "
                + "moveCharacterAt will suspend the follower before moving it "
                + "and resume it after, preventing the follow controller from fighting "
                + "with action-driven movement."
        )]
        private PartyFollowController _partyFollowController;

        [Header("Character Movement Settings")]
        [SerializeField]
        [Tooltip(
            "Scaling factor that converts blueprint pixel offsets to Unity 3D world units. "
                + "Blueprint uses pixel values (e.g. 800px). Multiply by this factor "
                + "to get 3D distances. Example: 800px * 0.01 = 8 world units."
        )]
        private float _pixelToWorldScaleFactor = 0.01f;

        [SerializeField]
        [Tooltip(
            "Reference Transform used as the origin for absolute character positioning. "
                + "When moveCharacterAt uses isAbsolute=true, pixel offsets are applied relative "
                + "to this Transform's position. Drag the player character or a central scene "
                + "object here. If not assigned, falls back to (0,0,0) which is rarely correct."
        )]
        private Transform _absolutePositionOriginTransform;

        [SerializeField]
        [Tooltip(
            "Fallback origin point if no Transform is assigned above. "
                + "Only used when _absolutePositionOriginTransform is null."
        )]
        private Vector3 _absolutePositionOriginFallback = Vector3.zero;

        private const string LogPrefix = "[LSDE Action]";

        /// <summary>
        /// Default duration in seconds for actions that don't have
        /// an explicit duration parameter.
        /// </summary>
        private const float DefaultDurationInSeconds = 0.5f;

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
        /// Reset camera state to normal follow mode. Clears any active shake offset
        /// and resumes the follow controller. Called by the presenter during scene exit
        /// to ensure the camera returns to its default behavior.
        /// </summary>
        public void ResetCameraState()
        {
            if (_cameraFollowController == null)
            {
                return;
            }

            _cameraFollowController.SetShakeOffset(Vector3.zero);
            _cameraFollowController.ResumeFollow();
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

            float worldIntensity = intensity * _shakeIntensityWorldScaleFactor;
            float elapsedTime = 0f;

            while (elapsedTime < durationInSeconds)
            {
                var shakeOffset = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f) * worldIntensity,
                    UnityEngine.Random.Range(-1f, 1f) * worldIntensity,
                    0f
                );

                _cameraFollowController.SetShakeOffset(shakeOffset);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _cameraFollowController.SetShakeOffset(Vector3.zero);

            Debug.Log($"{LogPrefix} shakeCamera — complete");
        }

        /// <summary>
        /// Smoothly move the camera to focus on a character identified by label.
        /// Pauses the camera follow, lerps to the character's position (+ camera offset)
        /// with ease-in-out cubic easing, then:
        /// - If the target is the player character (<see cref="_playerCharacterId"/>):
        ///   resumes follow so the camera tracks the player again.
        /// - Otherwise: leaves follow paused so the camera stays on the target
        ///   until the next camera command or scene exit.
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
            Vector3 characterGroundPosition = new Vector3(
                characterMarker.transform.position.x,
                0f,
                characterMarker.transform.position.z
            );
            Vector3 targetCameraPosition =
                characterGroundPosition + _cameraFollowController.CameraOffset;

            _cameraFollowController.PauseFollow();

            Vector3 startCameraPosition = _cameraFollowController.transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < durationInSeconds)
            {
                elapsedTime += Time.deltaTime;
                float linearProgress = Mathf.Clamp01(elapsedTime / durationInSeconds);
                float easedProgress = EaseInOutCubic(linearProgress);

                _cameraFollowController.transform.position = Vector3.Lerp(
                    startCameraPosition,
                    targetCameraPosition,
                    easedProgress
                );

                yield return null;
            }

            _cameraFollowController.transform.position = targetCameraPosition;

            // Resume follow only when returning to the player character.
            // For other targets, the camera stays on the target until the next
            // camera command or scene exit (which calls ResetCameraState).
            bool isTargetingPlayerCharacter = string.Equals(
                label,
                _playerCharacterId,
                StringComparison.OrdinalIgnoreCase
            );

            if (isTargetingPlayerCharacter)
            {
                _cameraFollowController.ResumeFollow();
            }

            Debug.Log($"{LogPrefix} moveCameraToLabel — complete");
        }

        /// <summary>
        /// Move a character to a target position using <see cref="CharacterMovementController"/>.
        /// The character walks to the target with hop animation and collision handling.
        ///
        /// Two positioning modes:
        /// - <b>Relative</b> (<c>isAbsolute=false</c>): offset from the character's current position.
        ///   Example: character at X=500, offsetX=-100 → target = 400.
        /// - <b>Absolute</b> (<c>isAbsolute=true</c>): offset from <see cref="_absolutePositionOriginTransform"/>.
        ///   The dev configures the origin point (default: scene center at 0,0,0).
        ///
        /// Blueprint offsets are in 2D pixel space. <see cref="_pixelToWorldScaleFactor"/>
        /// converts them to 3D world units (e.g. 800px * 0.01 = 8 units).
        ///
        /// Params: [0] characterId (string), [1] offsetX (number),
        ///         [2] offsetY (number, optional), [3] isAbsolute (boolean, optional).
        /// </summary>
        /// <param name="parameters">Ordered parameter values from the blueprint action.</param>
        private IEnumerator ExecuteMoveCharacterAt(List<object> parameters)
        {
            var characterId = parameters.Count > 0 ? parameters[0]?.ToString() : "unknown";
            var offsetX = ConvertParameterToFloat(parameters, 1, "offsetX");
            var offsetY = ConvertParameterToFloat(parameters, 2, "offsetY");
            // The boolean parameter may arrive as a raw bool or as a Newtonsoft JValue
            // wrapping a bool. "parameters[3] is bool" fails for JValue, so we use
            // Convert.ToBoolean which handles both via IConvertible.
            var isAbsolute = ConvertParameterToBoolean(parameters, 3);

            // Diagnostic log: print the actual C# type of the boolean parameter
            // to verify Newtonsoft deserialization (JValue vs bool).
            string boolParamDebugInfo =
                parameters.Count > 3 && parameters[3] != null
                    ? $"type={parameters[3].GetType().Name}, value={parameters[3]}"
                    : "missing/null";

            Debug.Log(
                $"{LogPrefix} moveCharacterAt — character={characterId}, "
                    + $"offsetX={offsetX}, offsetY={offsetY}, absolute={isAbsolute}, "
                    + $"scaleFactor={_pixelToWorldScaleFactor}, "
                    + $"boolParam=[{boolParamDebugInfo}]"
            );

            if (_characterRegistry == null)
            {
                Debug.LogWarning(
                    $"{LogPrefix} No CharacterRegistry assigned — cannot move character."
                );
                yield return new WaitForSeconds(DefaultDurationInSeconds);
                yield break;
            }

            var characterMarker = _characterRegistry.FindMarkerByCharacterId(characterId);
            if (characterMarker == null)
            {
                Debug.LogWarning($"{LogPrefix} Character '{characterId}' not found in scene.");
                yield return new WaitForSeconds(DefaultDurationInSeconds);
                yield break;
            }

            var movementController = characterMarker.GetComponent<CharacterMovementController>();
            if (movementController == null)
            {
                Debug.LogWarning(
                    $"{LogPrefix} No CharacterMovementController on character '{characterId}'."
                );
                yield return new WaitForSeconds(DefaultDurationInSeconds);
                yield break;
            }

            // Convert 2D pixel offsets to 3D world units
            // Blueprint X axis → Unity X axis (left/right)
            // Blueprint Y axis → Unity Z axis (forward/back)
            float worldOffsetX = offsetX * _pixelToWorldScaleFactor;
            float worldOffsetZ = offsetY * _pixelToWorldScaleFactor;

            Vector3 targetPosition;

            if (isAbsolute)
            {
                // Absolute: offset from the origin reference point.
                // Uses the Transform if assigned, otherwise falls back to the Vector3 field.
                Vector3 originPosition =
                    _absolutePositionOriginTransform != null
                        ? _absolutePositionOriginTransform.position
                        : _absolutePositionOriginFallback;

                targetPosition = new Vector3(
                    originPosition.x + worldOffsetX,
                    characterMarker.transform.position.y,
                    originPosition.z + worldOffsetZ
                );
            }
            else
            {
                // Relative: offset from the character's current position
                targetPosition = new Vector3(
                    characterMarker.transform.position.x + worldOffsetX,
                    characterMarker.transform.position.y,
                    characterMarker.transform.position.z + worldOffsetZ
                );
            }

            Vector3 logOrigin = isAbsolute
                ? (
                    _absolutePositionOriginTransform != null
                        ? _absolutePositionOriginTransform.position
                        : _absolutePositionOriginFallback
                )
                : characterMarker.transform.position;

            Debug.Log(
                $"{LogPrefix} moveCharacterAt — "
                    + $"currentPos={characterMarker.transform.position}, "
                    + $"worldOffset=({worldOffsetX}, {worldOffsetZ}), "
                    + $"origin={logOrigin}, "
                    + $"targetPos={targetPosition}"
            );

            // Suspend the follower in the party follow controller to prevent
            // the trail system from fighting with this action-driven movement.
            if (_partyFollowController != null)
            {
                _partyFollowController.SuspendFollower(characterId);
            }

            movementController.SetMovementTarget(targetPosition);

            // Wait for the character to arrive (or stop moving due to stuck detection)
            while (movementController.IsCharacterMoving)
            {
                yield return null;
            }

            // Resume the follower so the follow controller can take over again
            if (_partyFollowController != null)
            {
                _partyFollowController.ResumeFollower(characterId);
            }

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

        /// <summary>
        /// Safely extract a boolean parameter by index from the action's parameter list.
        /// Handles the same Newtonsoft deserialization quirk as <see cref="ConvertParameterToFloat"/>:
        /// JSON <c>true</c>/<c>false</c> may arrive as a raw <c>bool</c> OR as a Newtonsoft
        /// <c>JValue</c> wrapping a bool. Direct <c>is bool</c> pattern matching fails for
        /// <c>JValue</c>, so we use <see cref="Convert.ToBoolean(object)"/> which handles
        /// both via <see cref="IConvertible"/>.
        /// </summary>
        /// <param name="parameters">The action's parameter list.</param>
        /// <param name="index">The position of the parameter to extract.</param>
        /// <returns>The parameter value as a bool, or false if missing or unconvertible.</returns>
        private static bool ConvertParameterToBoolean(List<object> parameters, int index)
        {
            if (parameters == null || index >= parameters.Count || parameters[index] == null)
            {
                return false;
            }

            try
            {
                return Convert.ToBoolean(parameters[index]);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
