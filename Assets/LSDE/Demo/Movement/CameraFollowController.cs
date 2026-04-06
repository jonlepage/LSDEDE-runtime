using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Smooth camera follow with dead zone — faithful port of camera.ts (lines 332-366).
    ///
    /// Three states:
    /// 1. Inside dead zone: camera does NOTHING — no translation, no rotation.
    ///    This is the key difference from the previous implementation which called
    ///    LookAt() inside the dead zone, causing micro-rotation jitter.
    /// 2. Between dead zone and full follow radius: follow strength ramps progressively.
    /// 3. Beyond full follow radius: full follow strength.
    ///
    /// The camera rotation is fixed at Start() and never changes — matching
    /// the TS demo where the "camera" is just a container pivot with no rotation.
    ///
    /// The camera ignores the target's Y position (hop offset) by tracking
    /// ground-level position (Y=0), preventing bouncing with each hop.
    /// </summary>
    public class CameraFollowController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The Transform to follow (usually the player character).")]
        private Transform _targetToFollow;

        [SerializeField]
        [Tooltip(
            "Offset from the target position in world space. "
                + "X = left/right, Y = height, Z = distance behind."
        )]
        private Vector3 _cameraOffset = new Vector3(0f, 5f, -8f);

        [SerializeField]
        [Tooltip(
            "Additional rotation offset in degrees (pitch, yaw, roll) applied on top "
                + "of the automatic look direction. Use this to fine-tune the camera angle "
                + "without changing the position offset."
        )]
        private Vector3 _rotationOffset = Vector3.zero;

        [Header("Dead Zone")]
        [SerializeField]
        [Tooltip(
            "Radius within which the camera does absolutely nothing — no movement, "
                + "no rotation. Port of DEAD_ZONE_RATIO * 1080 ≈ 119px → 1.5 units."
        )]
        private float _deadZoneRadius = 1.5f;

        [SerializeField]
        [Tooltip(
            "Radius at which the camera reaches full follow speed. "
                + "Port of FULL_FOLLOW_RATIO * 1080 ≈ 400px → 5 units."
        )]
        private float _fullFollowRadius = 5f;

        [SerializeField]
        [Tooltip(
            "Base follow lerp speed. Port of DEFAULT_FOLLOW_LERP_FACTOR = 0.003. "
                + "Very low = cinematic lag."
        )]
        private float _followLerpSpeed = 0.003f;

        [Header("Dynamic Tilt")]
        [SerializeField]
        [Tooltip(
            "Maximum yaw angle in degrees when the target is at the edge of the follow zone. "
                + "Creates a subtle 'look toward' effect as the camera turns slightly "
                + "toward where the character is."
        )]
        private float _maxDynamicYawAngle = 3f;

        [SerializeField]
        [Tooltip(
            "Maximum roll angle in degrees during lateral movement. "
                + "Creates a cinematic lean effect, like a camera operator tilting "
                + "their body while tracking a moving subject."
        )]
        private float _maxDynamicRollAngle = 2f;

        [SerializeField]
        [Tooltip(
            "How fast the dynamic tilt smoothly catches up to its target value. "
                + "Lower = more cinematic lag, higher = more responsive."
        )]
        private float _dynamicTiltSmoothSpeed = 3f;

        [SerializeField]
        [Tooltip(
            "Reference movement speed for normalizing velocity when computing roll. "
                + "Should match the character's maximum movement speed."
        )]
        private float _movementSpeedReference = 4f;

        // --- Internal state ---
        private Quaternion _baseRotation;
        private float _currentDynamicYaw;
        private float _currentDynamicRoll;
        private Vector3 _previousTargetGroundPosition;

        /// <summary>
        /// When true, the follow logic in LateUpdate is skipped entirely.
        /// Used by camera commands (e.g. <c>moveCameraToLabel</c>) to take full control
        /// of the camera position without fighting the follow lerp.
        /// Matches the TS <c>pauseCameraFollow</c> / <c>resumeCameraFollow</c> pattern.
        /// </summary>
        private bool _isFollowPaused;

        /// <summary>
        /// Additive position offset applied after all other camera logic in LateUpdate.
        /// Set each frame by a camera shake coroutine, reset to zero when shake ends.
        /// Works both during normal follow and when follow is paused.
        /// </summary>
        private Vector3 _shakeOffset;

        /// <summary>
        /// The camera position offset in world space (height, distance behind, etc.).
        /// Exposed so action executors can compute the correct target position
        /// for camera commands (character position + this offset = desired camera position).
        /// </summary>
        public Vector3 CameraOffset => _cameraOffset;

        /// <summary>
        /// Pause the follow logic. The camera will stay at its current position
        /// until <see cref="ResumeFollow"/> is called. Camera commands and shake
        /// can still modify the position while follow is paused.
        /// </summary>
        public void PauseFollow()
        {
            _isFollowPaused = true;
        }

        /// <summary>
        /// Resume the follow logic after a pause. The camera will smoothly
        /// catch up to the target position from wherever it currently is.
        /// </summary>
        public void ResumeFollow()
        {
            _isFollowPaused = false;
        }

        /// <summary>
        /// Set the additive shake offset for this frame. Must be called every frame
        /// during a shake effect. Set to <see cref="Vector3.zero"/> when the shake ends.
        /// The offset is applied in LateUpdate after the follow position is computed.
        /// </summary>
        /// <param name="offset">The shake displacement to add to the camera position.</param>
        public void SetShakeOffset(Vector3 offset)
        {
            _shakeOffset = offset;
        }

        /// <summary>
        /// Set a fixed base rotation and snap to the initial position.
        /// The TS camera never rotates — it only translates the world pivot.
        /// We replicate this by computing the rotation once from the offset direction.
        /// </summary>
        private void Start()
        {
            // Base rotation derived from offset direction only.
            // _rotationOffset is applied every frame so it can be tweaked live in the Inspector.
            Vector3 lookDirection = -_cameraOffset.normalized;
            _baseRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = _baseRotation * Quaternion.Euler(_rotationOffset);

            // Snap to initial position (port of snapCameraToFollowTarget, camera.ts:279-289)
            if (_targetToFollow != null)
            {
                Vector3 targetGroundPosition = new Vector3(
                    _targetToFollow.position.x,
                    0f,
                    _targetToFollow.position.z
                );
                transform.position = targetGroundPosition + _cameraOffset;
                _previousTargetGroundPosition = targetGroundPosition;
            }
        }

        /// <summary>
        /// LateUpdate runs after all Update calls, ensuring the camera moves
        /// after the player has finished moving for this frame.
        /// Faithful port of camera.ts follow logic (lines 332-366) with added
        /// dynamic tilt for cinematic 3D feel.
        /// </summary>
        private void LateUpdate()
        {
            if (_targetToFollow == null)
            {
                ApplyShakeOffset();
                return;
            }

            if (_isFollowPaused)
            {
                // Follow is paused — camera position is controlled externally
                // (e.g. by a moveCameraToLabel coroutine). Only apply shake.
                ApplyShakeOffset();
                return;
            }

            // Use ground-level position (Y=0) to ignore hop bouncing
            Vector3 targetGroundPosition = new Vector3(
                _targetToFollow.position.x,
                0f,
                _targetToFollow.position.z
            );

            // ------------------------------------------------------------------
            // Target lateral velocity — used for roll computation
            // Computed before updating _previousTargetGroundPosition
            // ------------------------------------------------------------------
            float targetLateralVelocity =
                (targetGroundPosition.x - _previousTargetGroundPosition.x)
                / Mathf.Max(Time.deltaTime, 0.001f);
            _previousTargetGroundPosition = targetGroundPosition;

            Vector3 desiredCameraPosition = targetGroundPosition + _cameraOffset;
            Vector3 currentCameraPosition = transform.position;

            // Distance on XZ plane between current camera and desired position
            float deltaX = desiredCameraPosition.x - currentCameraPosition.x;
            float deltaZ = desiredCameraPosition.z - currentCameraPosition.z;
            float horizontalDistance = Mathf.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

            // ------------------------------------------------------------------
            // Position update — only outside dead zone (camera.ts:346)
            // Inside dead zone: position stays perfectly still (no jitter).
            // ------------------------------------------------------------------
            if (horizontalDistance > _deadZoneRadius)
            {
                // Progressive follow (camera.ts:347-357)
                float followStrength = Mathf.Clamp01(
                    (horizontalDistance - _deadZoneRadius) / (_fullFollowRadius - _deadZoneRadius)
                );

                // Exponential lerp — exact port of camera.ts:359
                float effectiveLerpSpeed = _followLerpSpeed * followStrength;
                float lerpAmount = 1f - Mathf.Pow(1f - effectiveLerpSpeed, Time.deltaTime * 60f);

                // XZ uses lerp, Y tracks immediately (no dead zone for height)
                transform.position = new Vector3(
                    currentCameraPosition.x + deltaX * lerpAmount,
                    desiredCameraPosition.y,
                    currentCameraPosition.z + deltaZ * lerpAmount
                );
            }

            // ------------------------------------------------------------------
            // Dynamic tilt — always computed so it decays smoothly to neutral
            //
            // Yaw: subtle horizontal rotation toward where the target is offset
            //       from the camera's center. When the character moves far right,
            //       the camera gently pans right to "look toward" them.
            //
            // Roll: subtle lean in the direction of lateral movement, like a
            //       camera operator tilting their body while tracking a subject.
            // ------------------------------------------------------------------
            float normalizedOffsetX = Mathf.Clamp(deltaX / _fullFollowRadius, -1f, 1f);
            float targetYawAngle = normalizedOffsetX * _maxDynamicYawAngle;

            float normalizedLateralVelocity = Mathf.Clamp(
                targetLateralVelocity / _movementSpeedReference,
                -1f,
                1f
            );
            float targetRollAngle = -normalizedLateralVelocity * _maxDynamicRollAngle;

            float smoothFactor = Time.deltaTime * _dynamicTiltSmoothSpeed;
            _currentDynamicYaw = Mathf.Lerp(_currentDynamicYaw, targetYawAngle, smoothFactor);
            _currentDynamicRoll = Mathf.Lerp(_currentDynamicRoll, targetRollAngle, smoothFactor);

            transform.rotation =
                _baseRotation
                * Quaternion.Euler(_rotationOffset)
                * Quaternion.Euler(0f, _currentDynamicYaw, _currentDynamicRoll);

            // Apply shake after follow position is fully computed
            ApplyShakeOffset();
        }

        /// <summary>
        /// Apply the additive shake offset to the camera position.
        /// Called at the end of LateUpdate so shake works both during
        /// normal follow and when follow is paused by a camera command.
        /// </summary>
        private void ApplyShakeOffset()
        {
            if (_shakeOffset != Vector3.zero)
            {
                transform.position += _shakeOffset;
            }
        }
    }
}
