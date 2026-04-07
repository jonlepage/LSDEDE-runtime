using UnityEngine;
using UnityEngine.InputSystem;

namespace LSDE.Demo
{
    /// <summary>
    /// Smooth camera follow with per-axis dead zones — evolved from camera.ts port.
    ///
    /// Three states per axis (X, Y, Z evaluated independently):
    /// 1. Inside dead zone: camera does NOTHING on that axis — no movement, no rotation.
    /// 2. Between dead zone and full follow radius: follow strength ramps progressively.
    /// 3. Beyond full follow radius: full follow strength on that axis.
    ///
    /// Using per-axis dead zones (box/AABB) instead of a single circular radius allows
    /// different tolerances for lateral movement (X), vertical (Y), and forward/backward
    /// progression (Z). Typical setup: tight Z dead zone so the player can advance freely,
    /// generous X dead zone for comfortable lateral exploration.
    ///
    /// The camera rotation is fixed at Start() and never changes — matching
    /// the TS demo where the "camera" is just a container pivot with no rotation.
    /// Dynamic tilt (yaw + roll) adds subtle cinematic rotation on top of the base.
    ///
    /// By default, Y dead zone is 0 (instant vertical tracking). Set Y dead zone > 0
    /// if you need tolerance for jumping or vertical platforms.
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

        [Header("Dead Zone — Per-Axis")]
        [SerializeField]
        [Tooltip(
            "Dead zone size per axis. Inside this zone the camera does absolutely nothing "
                + "— no movement, no rotation.\n\n"
                + "X = left/right tolerance (higher = the character can move further "
                + "sideways before the camera reacts).\n"
                + "Y = vertical tolerance (0 = camera tracks height instantly, "
                + "increase if you add jumping/platforms later).\n"
                + "Z = forward/backward tolerance (keep LOW so the player can advance "
                + "without feeling stuck — this is the 'progression' axis).\n\n"
                + "Typical values: X=1.5, Y=0, Z=0.5"
        )]
        private Vector3 _deadZone = new Vector3(1.5f, 0f, 0.5f);

        [SerializeField]
        [Tooltip(
            "Distance per axis at which the camera reaches full follow speed. "
                + "Between the dead zone and this radius, follow strength ramps "
                + "progressively (smooth acceleration).\n\n"
                + "X = full follow distance for left/right movement.\n"
                + "Y = full follow distance for vertical movement "
                + "(only relevant if dead zone Y > 0).\n"
                + "Z = full follow distance for forward/backward movement.\n\n"
                + "Must be greater than the corresponding dead zone value on each axis.\n"
                + "Typical values: X=5, Y=0, Z=3"
        )]
        private Vector3 _fullFollowRadius = new Vector3(5f, 0f, 3f);

        [SerializeField]
        [Tooltip(
            "Base follow lerp speed (shared across all axes). "
                + "Controls how fast the camera catches up once outside the dead zone.\n\n"
                + "Very low (0.001–0.005) = cinematic lag, smooth and slow.\n"
                + "Medium (0.01–0.05) = responsive but still smooth.\n"
                + "High (0.1+) = nearly instant follow.\n\n"
                + "The actual speed is modulated per axis by the follow strength "
                + "(distance-based ramp between dead zone and full follow radius)."
        )]
        private float _followLerpSpeed = 0.003f;

        [Header("Scroll Zoom")]
        [SerializeField]
        [Tooltip(
            "Enable zooming in/out with the mouse scroll wheel. "
                + "Adjusts the camera offset smoothly."
        )]
        private bool _enableScrollZoom = true;

        [SerializeField]
        [Tooltip(
            "How much each scroll step changes the zoom level. "
                + "Higher = faster zoom per scroll tick."
        )]
        private float _scrollZoomStep = 1.5f;

        [SerializeField]
        [Tooltip(
            "Minimum zoom multiplier (closest to the scene). Default 0.5 = half the original distance."
        )]
        private float _minimumZoomMultiplier = 0.5f;

        [SerializeField]
        [Tooltip(
            "Maximum zoom multiplier (farthest from the scene). Default 2.0 = double the original distance."
        )]
        private float _maximumZoomMultiplier = 2.0f;

        [SerializeField]
        [Tooltip(
            "How smoothly the zoom interpolates to the target level. "
                + "Higher = snappier, lower = smoother.\n\n"
                + "3–5 = smooth, cinematic.\n"
                + "8–12 = responsive."
        )]
        private float _zoomSmoothSpeed = 15f;

        [Header("Dynamic Tilt")]
        [SerializeField]
        [Tooltip(
            "Maximum yaw angle in degrees when the target is at the edge of the "
                + "follow zone on the X axis. Creates a subtle 'look toward' effect "
                + "— the camera turns slightly toward where the character is.\n\n"
                + "0 = disabled (no horizontal rotation).\n"
                + "1–3 = subtle, cinematic.\n"
                + "5+ = very noticeable turning."
        )]
        private float _maxDynamicYawAngle = 3f;

        [SerializeField]
        [Tooltip(
            "Maximum roll angle in degrees during lateral (left/right) movement. "
                + "Creates a cinematic lean effect, like a camera operator tilting "
                + "their body while tracking a moving subject.\n\n"
                + "0 = disabled (no roll).\n"
                + "1–2 = subtle, natural lean.\n"
                + "5+ = exaggerated, stylized."
        )]
        private float _maxDynamicRollAngle = 2f;

        [SerializeField]
        [Tooltip(
            "How fast the dynamic tilt (yaw + roll) smoothly catches up to its "
                + "target value. Think of it as the 'weight' of the camera operator.\n\n"
                + "Low (1–2) = heavy, cinematic lag — tilt changes slowly.\n"
                + "Medium (3–5) = balanced responsiveness.\n"
                + "High (8+) = snappy, almost instant tilt reaction."
        )]
        private float _dynamicTiltSmoothSpeed = 3f;

        [SerializeField]
        [Tooltip(
            "Reference movement speed (units/second) for normalizing the roll "
                + "computation. Should match the character's maximum movement speed.\n\n"
                + "If the character moves at 4 units/sec, set this to 4. "
                + "A mismatch means the roll will feel too weak (value too high) "
                + "or too aggressive (value too low)."
        )]
        private float _movementSpeedReference = 4f;

        // --- Internal state ---
        /// <summary>Target zoom multiplier driven by scroll input. Lerped toward smoothly.</summary>
        private float _targetZoomMultiplier = 1f;

        /// <summary>Current zoom multiplier (smoothly interpolated toward target).</summary>
        private float _currentZoomMultiplier = 1f;

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
        /// Position offset to apply this frame for camera shake.
        /// Set each frame by a camera shake coroutine, reset to zero when shake ends.
        /// Works both during normal follow and when follow is paused.
        /// </summary>
        private Vector3 _shakeOffset;

        /// <summary>
        /// The shake offset that was actually applied to the camera position last frame.
        /// Used to undo the previous frame's shake before applying the new one,
        /// preventing cumulative drift when follow is paused.
        /// </summary>
        private Vector3 _previousAppliedShakeOffset;

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

            // --- Scroll zoom ---
            if (_enableScrollZoom && Mouse.current != null)
            {
                float rawScrollValue = Mouse.current.scroll.ReadValue().y;
                float scrollDelta = rawScrollValue / 120f;

                if (Mathf.Abs(rawScrollValue) > 0.1f)
                {
                    Debug.Log(
                        $"[LSDE Zoom] raw={rawScrollValue} delta={scrollDelta} target={_targetZoomMultiplier}"
                    );
                    // Scroll up = zoom in (smaller multiplier), scroll down = zoom out
                    _targetZoomMultiplier -= scrollDelta * _scrollZoomStep;
                    _targetZoomMultiplier = Mathf.Clamp(
                        _targetZoomMultiplier,
                        _minimumZoomMultiplier,
                        _maximumZoomMultiplier
                    );
                }

                _currentZoomMultiplier = Mathf.Lerp(
                    _currentZoomMultiplier,
                    _targetZoomMultiplier,
                    Time.deltaTime * _zoomSmoothSpeed
                );
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

            Vector3 desiredCameraPosition =
                targetGroundPosition + _cameraOffset * _currentZoomMultiplier;
            Vector3 currentCameraPosition = transform.position;

            // Per-axis delta between current camera and desired position
            float deltaX = desiredCameraPosition.x - currentCameraPosition.x;
            float deltaY = desiredCameraPosition.y - currentCameraPosition.y;
            float deltaZ = desiredCameraPosition.z - currentCameraPosition.z;

            // ------------------------------------------------------------------
            // Per-axis position update — each axis has its own dead zone.
            // Inside the dead zone for a given axis: that axis stays perfectly
            // still (no jitter). Outside: progressive follow ramps up.
            // This replaces the old circular dead zone with a box (AABB),
            // allowing tight follow on Z (progression) and loose on X (lateral).
            // ------------------------------------------------------------------
            float absoluteDeltaX = Mathf.Abs(deltaX);
            float absoluteDeltaZ = Mathf.Abs(deltaZ);
            float absoluteDeltaY = Mathf.Abs(deltaY);

            // X axis — lateral movement
            float followStrengthX = 0f;
            if (absoluteDeltaX > _deadZone.x && _fullFollowRadius.x > _deadZone.x)
            {
                followStrengthX = Mathf.Clamp01(
                    (absoluteDeltaX - _deadZone.x) / (_fullFollowRadius.x - _deadZone.x)
                );
            }

            // Z axis — forward/backward progression
            float followStrengthZ = 0f;
            if (absoluteDeltaZ > _deadZone.z && _fullFollowRadius.z > _deadZone.z)
            {
                followStrengthZ = Mathf.Clamp01(
                    (absoluteDeltaZ - _deadZone.z) / (_fullFollowRadius.z - _deadZone.z)
                );
            }

            // Y axis — vertical (uses dead zone only if configured, otherwise instant)
            float followStrengthY = 1f;
            if (_deadZone.y > 0f && _fullFollowRadius.y > _deadZone.y)
            {
                followStrengthY =
                    absoluteDeltaY > _deadZone.y
                        ? Mathf.Clamp01(
                            (absoluteDeltaY - _deadZone.y) / (_fullFollowRadius.y - _deadZone.y)
                        )
                        : 0f;
            }

            // Exponential lerp per axis — same formula as the original port
            // of camera.ts:359 but applied independently per component.
            float frameMultiplier = Time.deltaTime * 60f;

            float lerpAmountX =
                1f - Mathf.Pow(1f - _followLerpSpeed * followStrengthX, frameMultiplier);
            float lerpAmountZ =
                1f - Mathf.Pow(1f - _followLerpSpeed * followStrengthZ, frameMultiplier);
            float lerpAmountY =
                1f - Mathf.Pow(1f - _followLerpSpeed * followStrengthY, frameMultiplier);

            transform.position = new Vector3(
                currentCameraPosition.x + deltaX * lerpAmountX,
                currentCameraPosition.y + deltaY * lerpAmountY,
                currentCameraPosition.z + deltaZ * lerpAmountZ
            );

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
            float normalizedOffsetX =
                _fullFollowRadius.x > 0f ? Mathf.Clamp(deltaX / _fullFollowRadius.x, -1f, 1f) : 0f;
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
        /// Apply the shake offset to the camera position, undoing the previous
        /// frame's offset first to prevent cumulative drift.
        ///
        /// When follow is active, the follow logic recalculates position from scratch
        /// each frame, so the undo is redundant but harmless.
        /// When follow is paused, the undo is critical — without it, each frame's
        /// random shake would accumulate and the camera would drift away.
        /// </summary>
        private void ApplyShakeOffset()
        {
            // Undo last frame's shake to restore the "clean" base position
            transform.position -= _previousAppliedShakeOffset;

            // Apply this frame's shake
            if (_shakeOffset != Vector3.zero)
            {
                transform.position += _shakeOffset;
            }

            _previousAppliedShakeOffset = _shakeOffset;
        }
    }
}
