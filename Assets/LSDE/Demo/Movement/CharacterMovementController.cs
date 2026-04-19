using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Click-to-move character controller with bouncy hop walk and inertia tilt.
    /// Movement uses Vector3.SmoothDamp (Unity's built-in spring-damper) for
    /// frame-rate independent approach with natural deceleration — no overshoot,
    /// no teleportation, no manual easing required.
    ///
    /// Collisions are handled natively by Unity's <see cref="CharacterController"/>
    /// component — any GameObject with a Collider will block movement automatically.
    ///
    /// Hop and tilt are ported from the TS demo movement.ts.
    /// Works on any character — player or NPC.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMovementController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField]
        [Tooltip("Maximum movement speed in world units per second.")]
        private float _movementSpeed = 4f;

        [SerializeField]
        [Tooltip(
            "Approximate time in seconds for the character to reach the target. "
                + "Controls how early deceleration begins. "
                + "Lower = snappier arrival, higher = more cinematic glide."
        )]
        private float _smoothTime = 0.3f;

        [SerializeField]
        [Tooltip(
            "Distance below which the character snaps to the target and stops. "
                + "Must be very small so the snap is invisible."
        )]
        private float _arrivalThreshold = 0.02f;

        [Header("Hop (Bouncy Walk)")]
        [SerializeField]
        [Tooltip(
            "Distance between consecutive hops. Port of HOP_DISTANCE_PER_STRIDE = 30px → 0.8 units."
        )]
        private float _hopStrideDistance = 0.4f;

        [SerializeField]
        [Tooltip("Maximum height of the hop arc in world units.")]
        private float _hopMaxHeight = 0.35f;

        [SerializeField]
        [Tooltip(
            "Duration in seconds for the character to land when decelerating mid-hop. "
                + "Uses Mathf.MoveTowards for frame-rate independent smooth landing."
        )]
        private float _hopLandingDuration = 0.15f;

        [Header("Inertia Tilt")]
        [SerializeField]
        [Tooltip(
            "Maximum tilt angle in degrees when moving at full speed. "
                + "Applied proportionally to the character's sideways velocity."
        )]
        private float _maxTiltAngle = 15f;

        [SerializeField]
        [Tooltip(
            "How fast the smoothed velocity catches up to actual velocity. "
                + "Port of INERTIA_VELOCITY_LERP = 0.1. Lower = more inertia lag."
        )]
        private float _inertiaVelocityLerp = 0.1f;

        [Header("Terrain Following")]
        [SerializeField]
        [Tooltip(
            "Layer mask for ground surfaces (terrain, planes). Sampled each frame "
                + "via downward raycast so the character follows hills and dips automatically. "
                + "Leave empty to keep the legacy flat-ground behavior."
        )]
        private LayerMask _groundLayerMask;

        // --- Internal state ---
        private Vector3? _currentTarget;
        private float _groundLevelY;
        private Vector3 _smoothDampVelocity;
        private float _distanceSinceLastHop;
        private float _hopProgress = -1f;
        private float _currentHopHeight;
        private bool _isMoving;
        private CharacterController _characterController;
        private float _stuckTimer;
        private float _previousDistanceToTarget;

        /// <summary>
        /// Whether the character is currently moving toward a target.
        /// </summary>
        public bool IsCharacterMoving => _isMoving;

        private void Awake()
        {
            _groundLevelY = transform.position.y;
            _characterController = GetComponent<CharacterController>();
        }

        /// <summary>
        /// Raycast downward at the given XZ position to find the actual ground surface Y.
        /// Lets the character follow terrain elevation (hills and dips) instead of staying
        /// frozen on the initial ground level captured at <see cref="Awake"/>.
        /// Falls back to <see cref="_groundLevelY"/> if no ground collider is hit, or if
        /// <see cref="_groundLayerMask"/> is empty (legacy flat-ground behavior).
        /// </summary>
        private float SampleGroundHeightAt(float worldX, float worldZ)
        {
            if (_groundLayerMask.value == 0)
            {
                return _groundLevelY;
            }

            Vector3 rayOrigin = new Vector3(worldX, transform.position.y + 50f, worldZ);
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hitInfo, 200f, _groundLayerMask))
            {
                return hitInfo.point.y;
            }

            return _groundLevelY;
        }

        /// <summary>
        /// Set a movement target in world space. The character will move toward it.
        /// </summary>
        public void SetMovementTarget(Vector3 worldPosition)
        {
            _currentTarget = new Vector3(worldPosition.x, _groundLevelY, worldPosition.z);
            _isMoving = true;
            _stuckTimer = 0f;
            _previousDistanceToTarget = float.MaxValue;

            // Only start a hop if the distance is at least one full stride —
            // micro-movements (a few frames) glide naturally without bouncing.
            float distanceToTarget = Vector3.Distance(
                _currentTarget.Value,
                new Vector3(transform.position.x, _groundLevelY, transform.position.z)
            );

            if (_hopProgress < 0f && distanceToTarget >= _hopStrideDistance)
            {
                _hopProgress = 0f;
                _distanceSinceLastHop = 0f;
            }
        }

        /// <summary>
        /// Stop all movement immediately and reset to ground.
        /// </summary>
        public void StopMovement()
        {
            _currentTarget = null;
            _isMoving = false;
            _smoothDampVelocity = Vector3.zero;
            _distanceSinceLastHop = 0f;
            _hopProgress = -1f;
            _currentHopHeight = 0f;
            ApplyPosition(new Vector3(transform.position.x, _groundLevelY, transform.position.z));
            transform.rotation = Quaternion.identity;
        }

        private void Update()
        {
            // ------------------------------------------------------------------
            // 1. IDLE — no target
            // ------------------------------------------------------------------
            if (_currentTarget == null)
            {
                HandleIdleDecay();
                return;
            }

            Vector3 targetPosition = _currentTarget.Value;
            Vector3 currentPosition = transform.position;

            // ------------------------------------------------------------------
            // 2. Distance remaining on XZ plane
            // ------------------------------------------------------------------
            float deltaX = targetPosition.x - currentPosition.x;
            float deltaZ = targetPosition.z - currentPosition.z;
            float distanceRemaining = Mathf.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

            // ------------------------------------------------------------------
            // 3. Arrival — snap only when very close AND nearly stopped
            //    SmoothDamp brings us smoothly to near-zero distance,
            //    so the snap is invisible (0.02 units = sub-pixel).
            // ------------------------------------------------------------------
            if (
                distanceRemaining < _arrivalThreshold
                && _smoothDampVelocity.sqrMagnitude < 0.001f
                && _currentHopHeight < 0.01f
            )
            {
                float arrivalGroundY = SampleGroundHeightAt(targetPosition.x, targetPosition.z);
                ApplyPosition(new Vector3(targetPosition.x, arrivalGroundY, targetPosition.z));
                _smoothDampVelocity = Vector3.zero;
                _distanceSinceLastHop = 0f;
                _hopProgress = -1f;
                _currentHopHeight = 0f;
                transform.rotation = Quaternion.identity;
                _currentTarget = null;
                _isMoving = false;
                return;
            }

            // ------------------------------------------------------------------
            // 4. SmoothDamp — Unity's built-in spring-damper movement
            //    Frame-rate independent, no overshoot, smooth deceleration.
            //    Operates on XZ only (Y is handled by hop system).
            // ------------------------------------------------------------------
            Vector3 currentGroundPosition = new Vector3(
                currentPosition.x,
                _groundLevelY,
                currentPosition.z
            );
            Vector3 newGroundPosition = Vector3.SmoothDamp(
                currentGroundPosition,
                targetPosition,
                ref _smoothDampVelocity,
                _smoothTime,
                _movementSpeed
            );
            float newX = newGroundPosition.x;
            float newZ = newGroundPosition.z;

            // ------------------------------------------------------------------
            // 5. Frame distance — needed for hop and tilt calculations
            // ------------------------------------------------------------------
            float frameDeltaX = newX - currentPosition.x;
            float frameDeltaZ = newZ - currentPosition.z;
            float frameDistance = Mathf.Sqrt(frameDeltaX * frameDeltaX + frameDeltaZ * frameDeltaZ);

            // ------------------------------------------------------------------
            // 6. Inertia tilt
            //    Use _smoothDampVelocity.x directly — it's already smooth
            //    (SmoothDamp filters out frame-to-frame noise internally).
            //    Normalise to -1..+1 by dividing by max speed.
            // ------------------------------------------------------------------
            float normalizedTilt =
                _movementSpeed > 0.001f ? _smoothDampVelocity.x / _movementSpeed : 0f;
            float tiltDegrees = Mathf.Clamp(normalizedTilt, -1f, 1f) * _maxTiltAngle;
            transform.rotation = Quaternion.Euler(0f, 0f, tiltDegrees);

            // ------------------------------------------------------------------
            // 7. Hop — distance-based (movement.ts:184-213)
            // ------------------------------------------------------------------
            // Accumulate distance walked
            _distanceSinceLastHop += frameDistance;

            // Trigger a new hop when stride distance reached (only if not already hopping)
            if (_hopProgress < 0f && _distanceSinceLastHop >= _hopStrideDistance)
            {
                _hopProgress = 0f;
                _distanceSinceLastHop = 0f;
            }

            // Advance active hop — switch to smooth landing when horizontal
            // speed is too low to sustain the parabolic arc.
            // This prevents the character from freezing mid-air and clipping
            // to the ground when SmoothDamp decelerates near the target.
            if (_hopProgress >= 0f)
            {
                // Adaptive threshold: 5% of full-speed frame distance —
                // scales automatically with _movementSpeed and framerate.
                float adaptiveSpeedFloor = _movementSpeed * Time.deltaTime * 0.05f;

                if (frameDistance < adaptiveSpeedFloor)
                {
                    // Speed too low to sustain hop — smooth frame-rate independent
                    // landing using MoveTowards. Duration adapts to _hopMaxHeight.
                    float landingSpeed =
                        (_hopMaxHeight / _hopLandingDuration) * Time.deltaTime;
                    _currentHopHeight = Mathf.MoveTowards(
                        _currentHopHeight,
                        0f,
                        landingSpeed
                    );
                    if (_currentHopHeight == 0f)
                    {
                        _hopProgress = -1f;
                    }
                }
                else
                {
                    _hopProgress += frameDistance / _hopStrideDistance;

                    if (_hopProgress >= 1f)
                    {
                        _hopProgress = -1f;
                        _currentHopHeight = 0f;
                    }
                    else
                    {
                        // Parabolic arc: 4*p*(1-p) peaks at 1.0 when p=0.5
                        float arc = 4f * _hopProgress * (1f - _hopProgress);
                        _currentHopHeight = arc * _hopMaxHeight;
                    }
                }
            }
            else if (_currentHopHeight > 0f)
            {
                // No active hop but residual height — smooth landing
                float landingSpeed =
                    (_hopMaxHeight / _hopLandingDuration) * Time.deltaTime;
                _currentHopHeight = Mathf.MoveTowards(_currentHopHeight, 0f, landingSpeed);
            }

            // ------------------------------------------------------------------
            // Apply final position via CharacterController.Move()
            // Unity handles all collision resolution natively — the character
            // slides along walls, stops at obstacles, no manual overlap code needed.
            // ------------------------------------------------------------------
            float currentGroundY = SampleGroundHeightAt(newX, newZ);
            Vector3 desiredPosition = new Vector3(newX, currentGroundY + _currentHopHeight, newZ);
            ApplyPosition(desiredPosition);

            // ------------------------------------------------------------------
            // 8. Stuck detection — if the distance to the target is not
            //    decreasing, the character is blocked by a collider.
            //    Stop after a short grace period so it doesn't hop in place.
            // ------------------------------------------------------------------
            float currentDistanceToTarget = Vector3.Distance(
                new Vector3(transform.position.x, 0f, transform.position.z),
                new Vector3(targetPosition.x, 0f, targetPosition.z)
            );

            if (currentDistanceToTarget >= _previousDistanceToTarget - 0.001f)
            {
                _stuckTimer += Time.deltaTime;
                if (_stuckTimer > 0.2f)
                {
                    _stuckTimer = 0f;
                    _previousDistanceToTarget = float.MaxValue;
                    StopMovement();
                    return;
                }
            }
            else
            {
                _stuckTimer = 0f;
            }

            _previousDistanceToTarget = currentDistanceToTarget;
        }

        /// <summary>
        /// Idle state: decay smoothed velocity and hop height.
        /// </summary>
        private void HandleIdleDecay()
        {
            float idleGroundY = SampleGroundHeightAt(transform.position.x, transform.position.z);

            // Decay residual hop height with frame-rate independent landing
            if (_currentHopHeight > 0f)
            {
                float landingSpeed =
                    (_hopMaxHeight / _hopLandingDuration) * Time.deltaTime;
                _currentHopHeight = Mathf.MoveTowards(_currentHopHeight, 0f, landingSpeed);

                ApplyPosition(
                    new Vector3(
                        transform.position.x,
                        idleGroundY + _currentHopHeight,
                        transform.position.z
                    )
                );
            }
            else if (Mathf.Abs(transform.position.y - idleGroundY) > 0.001f)
            {
                ApplyPosition(
                    new Vector3(transform.position.x, idleGroundY, transform.position.z)
                );
            }

            // Decay tilt smoothly using SmoothDamp's residual velocity
            // (it decays naturally to zero after movement stops)
            float currentTiltZ = transform.rotation.eulerAngles.z;
            if (currentTiltZ > 180f)
            {
                currentTiltZ -= 360f;
            }

            if (Mathf.Abs(currentTiltZ) > 0.1f)
            {
                float decayedTilt = currentTiltZ * 0.9f;
                transform.rotation = Quaternion.Euler(0f, 0f, decayedTilt);
            }
            else
            {
                transform.rotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// Move the character to a desired position using <see cref="CharacterController.Move"/>.
        /// Unity's CharacterController handles collision detection and sliding natively.
        /// </summary>
        private void ApplyPosition(Vector3 desiredPosition)
        {
            Vector3 movementDelta = desiredPosition - transform.position;
            _characterController.Move(movementDelta);
        }
    }
}
