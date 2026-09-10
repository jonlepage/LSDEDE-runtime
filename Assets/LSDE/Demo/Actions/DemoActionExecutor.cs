using System;
using System.Collections;
using System.Collections.Generic;
using LSDE.Runtime;
using LsdeDialogEngine;
using UnityEngine;
using UnityEngine.Serialization;
using DemoIds = LsdedeDemoTsBlueprintIds;

namespace LSDE.Demo
{
    /// <summary>
    /// Demo implementation of <see cref="IActionExecutor"/> that maps a call's function id
    /// to a game effect via a switch/case pattern.
    ///
    /// The switch/case is intentional — it keeps all mapping in one place, making it easy for
    /// developers to see and extend. Both the function ids and the parameter names come from the
    /// generated <c>DemoIds.Functions</c> / <c>DemoIds.FunctionParams</c> constants, so a renamed
    /// function or parameter breaks the build instead of failing silently at runtime.
    ///
    /// <para>Camera calls (<c>shakeCamera</c>, <c>moveCameraToLabel</c>) use the
    /// <see cref="CameraFollowController"/> API (pause/resume/shake offset).
    /// Character movement (<c>moveCharacterAt</c>) uses <see cref="CharacterMovementController"/>.</para>
    /// </summary>
    public class DemoActionExecutor : MonoBehaviour, IActionExecutor
    {
        [SerializeField]
        [Tooltip("The camera follow controller on the main camera. Required for camera actions.")]
        private CameraFollowController _cameraFollowController;

        [SerializeField]
        [Tooltip(
            "The character registry that maps LSDE card names to scene GameObjects. "
                + "Required for moveCameraToLabel and moveCharacterAt."
        )]
        private DialogueCharacterRegistry _characterRegistry;

        [Header("Camera Settings")]
        // There was a "player character name" here, naming the one target after which
        // moveCameraToLabel would hand the camera back to the follow. It is gone because the
        // camera is handed back after EVERY pan now — see ExecuteMoveCameraToLabel — so there is
        // no longer a privileged target to name.

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
                + "When moveCharacterAt uses absolut=true, pixel offsets are applied relative "
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

        // Defaults for arguments the writer left empty — such an argument is ABSENT from the
        // bag, not zero, so every read states its own. Same values as the reference demo
        // (src/demos/shared/execute-action.ts).

        /// <summary>Fallback shake intensity, in blueprint units.</summary>
        private const float DefaultShakeIntensity = 1f;

        /// <summary>Fallback shake duration, in seconds.</summary>
        private const float DefaultShakeDurationInSeconds = 0.3f;

        /// <summary>Fallback camera travel duration, in seconds.</summary>
        private const float DefaultCameraDurationInSeconds = 1f;

        /// <summary>How long a call waits when it cannot do its work at all.</summary>
        private const float DefaultDurationInSeconds = 0.5f;

        /// <inheritdoc />
        public IEnumerator ExecuteAction(ActionCall call)
        {
            // Fn is empty when the writer has not picked a function yet. That is a draft, not an
            // error, and the flow must carry on.
            if (string.IsNullOrEmpty(call.Fn))
            {
                Debug.LogWarning($"{LogPrefix} A call has no function picked yet. Skipping.");
                yield break;
            }

            switch (call.Fn)
            {
                case DemoIds.Functions.shakeCamera:
                    yield return ExecuteShakeCamera(call.Args);
                    break;

                case DemoIds.Functions.moveCameraToLabel:
                    yield return ExecuteMoveCameraToLabel(call.Args);
                    break;

                case DemoIds.Functions.moveCharacterAt:
                    yield return ExecuteMoveCharacterAt(call.Args);
                    break;

                default:
                    Debug.LogWarning($"{LogPrefix} Unknown function '{call.Fn}'. Skipping.");
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
        /// Shake the camera by applying random offsets each frame for the given duration.
        /// Uses <see cref="CameraFollowController.SetShakeOffset"/> so the shake is additive
        /// on top of both normal follow and paused (command) states.
        /// Arguments: <c>intensity</c> (number), <c>duration</c> (number, seconds).
        /// </summary>
        private IEnumerator ExecuteShakeCamera(Dictionary<string, object> arguments)
        {
            var intensity = LsdeActionArgs.GetSingle(
                arguments,
                DemoIds.FunctionParams.shakeCamera.intensity,
                DefaultShakeIntensity
            );
            var durationInSeconds = LsdeActionArgs.GetSingle(
                arguments,
                DemoIds.FunctionParams.shakeCamera.duration,
                DefaultShakeDurationInSeconds
            );

            if (durationInSeconds <= 0)
            {
                durationInSeconds = DefaultShakeDurationInSeconds;
            }

            Debug.Log(
                $"{LogPrefix} shakeCamera — {LsdeActionArgs.Describe(arguments)} "
                    + $"→ duration={durationInSeconds}s"
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
        /// Smoothly move the camera to focus on a character named by a dictionary key.
        ///
        /// <para>The follow is paused for the PAN and resumed the moment it lands — always, whoever
        /// was targeted. Resuming does not snap: the follow lerp glides back from wherever the
        /// camera stopped, so the shot still lingers on what it was sent to show before drifting
        /// home.</para>
        ///
        /// <para>This used to resume only when the target WAS the player, and leave the camera
        /// parked on anything else "until the next camera command or scene exit". No such command
        /// comes in <c>advance-full-demo</c>: both of its calls aim at the beast, so the camera
        /// stayed on it for the whole scene while the rabbits spoke off-screen — and the three
        /// simultaneous bubbles of <c>DIALOG-015</c>, the point of that scene, were never in
        /// frame. The reference demo resumes in a <c>finally</c>, and it is right to.</para>
        ///
        /// <para>Arguments: <c>id</c> (a key of the <c>moveCameraToLabel_id</c> dictionary — a card
        /// name), <c>duration</c> (number, seconds).</para>
        /// </summary>
        private IEnumerator ExecuteMoveCameraToLabel(Dictionary<string, object> arguments)
        {
            var targetName = LsdeActionArgs.GetString(
                arguments,
                DemoIds.FunctionParams.moveCameraToLabel.id
            );
            var durationInSeconds = LsdeActionArgs.GetSingle(
                arguments,
                DemoIds.FunctionParams.moveCameraToLabel.duration,
                DefaultCameraDurationInSeconds
            );

            if (durationInSeconds <= 0)
            {
                durationInSeconds = DefaultCameraDurationInSeconds;
            }

            // No target named: the writer has not filled the argument in. Nothing to aim at,
            // and nothing to wait for either.
            if (string.IsNullOrEmpty(targetName))
            {
                Debug.LogWarning($"{LogPrefix} moveCameraToLabel — no target named.");
                yield break;
            }

            Debug.Log(
                $"{LogPrefix} moveCameraToLabel — {LsdeActionArgs.Describe(arguments)} "
                    + $"→ duration={durationInSeconds}s"
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

            var characterMarker = _characterRegistry.FindMarkerByCharacterName(targetName);
            if (characterMarker == null)
            {
                Debug.LogWarning(
                    $"{LogPrefix} Character '{targetName}' not in scene — simulating wait only."
                );
                yield return new WaitForSeconds(durationInSeconds);
                yield break;
            }

            // Target position = camera anchor ground position + camera offset.
            // Uses CameraAnchorPoint instead of transform directly — this allows
            // per-character camera framing by placing the anchor further away.
            Vector3 anchorPosition = characterMarker.CameraAnchorPoint.position;
            Vector3 characterGroundPosition = new Vector3(anchorPosition.x, 0f, anchorPosition.z);
            Vector3 targetCameraPosition =
                characterGroundPosition + _cameraFollowController.CameraOffset;

            _cameraFollowController.PauseFollow();

            // try/finally so the follow comes back even if this coroutine is cut short — a scene
            // abandoned on Escape stops it mid-pan, and a camera left paused there would never
            // track the player again.
            try
            {
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
            }
            finally
            {
                // Always. The shot has been delivered; the camera now belongs to the player again
                // and lerps home on its own, without snapping.
                _cameraFollowController.ResumeFollow();
            }

            Debug.Log($"{LogPrefix} moveCameraToLabel — complete");
        }

        /// <summary>
        /// Move a character to a target position using <see cref="CharacterMovementController"/>.
        /// The character walks to the target with hop animation and collision handling.
        ///
        /// Two positioning modes:
        /// - <b>Relative</b> (<c>absolut</c> absent or false): offset from the character's current
        ///   position. Example: character at X=500, x=-100 → target = 400.
        /// - <b>Absolute</b> (<c>absolut=true</c>): offset from
        ///   <see cref="_absolutePositionOriginTransform"/>.
        ///
        /// Blueprint offsets are in 2D pixel space. <see cref="_pixelToWorldScaleFactor"/>
        /// converts them to 3D world units (e.g. 800px * 0.01 = 8 units).
        ///
        /// Arguments: <c>id</c> (a key of the <c>party</c> dictionary — a card name),
        /// <c>x</c> (number), <c>y</c> (number, often absent), <c>absolut</c> (boolean, often absent).
        /// </summary>
        private IEnumerator ExecuteMoveCharacterAt(Dictionary<string, object> arguments)
        {
            var characterName = LsdeActionArgs.GetString(
                arguments,
                DemoIds.FunctionParams.moveCharacterAt.id
            );
            var offsetX = LsdeActionArgs.GetSingle(
                arguments,
                DemoIds.FunctionParams.moveCharacterAt.x
            );
            var offsetY = LsdeActionArgs.GetSingle(
                arguments,
                DemoIds.FunctionParams.moveCharacterAt.y
            );
            var isAbsolute = LsdeActionArgs.GetBoolean(
                arguments,
                DemoIds.FunctionParams.moveCharacterAt.absolut
            );

            Debug.Log(
                $"{LogPrefix} moveCharacterAt — {LsdeActionArgs.Describe(arguments)} "
                    + $"→ offset=({offsetX}, {offsetY}) absolute={isAbsolute} "
                    + $"scaleFactor={_pixelToWorldScaleFactor}"
            );

            // No character named: nothing to move, and nothing to wait for.
            if (string.IsNullOrEmpty(characterName))
            {
                Debug.LogWarning($"{LogPrefix} moveCharacterAt — no character named.");
                yield break;
            }

            if (_characterRegistry == null)
            {
                Debug.LogWarning(
                    $"{LogPrefix} No CharacterRegistry assigned — cannot move character."
                );
                yield return new WaitForSeconds(DefaultDurationInSeconds);
                yield break;
            }

            var characterMarker = _characterRegistry.FindMarkerByCharacterName(characterName);
            if (characterMarker == null)
            {
                Debug.LogWarning($"{LogPrefix} Character '{characterName}' not found in scene.");
                yield return new WaitForSeconds(DefaultDurationInSeconds);
                yield break;
            }

            var movementController = characterMarker.GetComponent<CharacterMovementController>();
            if (movementController == null)
            {
                Debug.LogWarning(
                    $"{LogPrefix} No CharacterMovementController on character '{characterName}'."
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

            Debug.Log(
                $"{LogPrefix} moveCharacterAt — "
                    + $"currentPos={characterMarker.transform.position}, "
                    + $"targetPos={targetPosition}"
            );

            // Suspend the follower in the party follow controller to prevent
            // the trail system from fighting with this action-driven movement.
            if (_partyFollowController != null)
            {
                _partyFollowController.SuspendFollower(characterName);
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
                _partyFollowController.ResumeFollower(characterName);
            }

            Debug.Log($"{LogPrefix} moveCharacterAt — complete");
        }

        /// <summary>
        /// Ease-in-out cubic easing function for smooth camera movements.
        /// Accelerates during the first half, decelerates during the second half.
        /// </summary>
        private static float EaseInOutCubic(float progress)
        {
            if (progress < 0.5f)
            {
                return 4f * progress * progress * progress;
            }

            float shifted = -2f * progress + 2f;
            return 1f - shifted * shifted * shifted / 2f;
        }
    }
}
