using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Click-to-move character controller with bouncy hop walk and inertia tilt.
    /// Movement uses Vector3.SmoothDamp (Unity's built-in spring-damper) for
    /// frame-rate independent approach with natural deceleration — no overshoot,
    /// no teleportation, no manual easing required.
    ///
    /// Hop and tilt are ported from the TS demo movement.ts.
    /// Works on any character — player or NPC.
    /// </summary>
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
        [Tooltip("Frame distance below which the hop decays instead of advancing.")]
        private float _hopSpeedFloor = 0.001f;

        [Header("Collision Avoidance")]
        [SerializeField]
        [Tooltip(
            "Layer mask for obstacles the character cannot walk through (e.g. NPCs). "
                + "After each movement step, overlap with these layers is resolved "
                + "by pushing the character out."
        )]
        private LayerMask _obstacleLayerMask;

        [SerializeField]
        [Tooltip("Radius used for overlap detection with obstacles.")]
        private float _collisionRadius = 0.4f;

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

        // --- Internal state ---
        private Vector3? _currentTarget;
        private float _groundLevelY;
        private Vector3 _smoothDampVelocity;
        private float _distanceSinceLastHop;
        private float _hopProgress = -1f;
        private float _currentHopHeight;
        private bool _isMoving;
        private Collider _ownCollider;
        private readonly Collider[] _overlapResultsBuffer = new Collider[8];

        /// <summary>
        /// Whether the character is currently moving toward a target.
        /// </summary>
        public bool IsCharacterMoving => _isMoving;

        private void Awake()
        {
            _groundLevelY = transform.position.y;
            _ownCollider = GetComponent<Collider>();
        }

        /// <summary>
        /// Set a movement target in world space. The character will move toward it.
        /// </summary>
        public void SetMovementTarget(Vector3 worldPosition)
        {
            _currentTarget = new Vector3(worldPosition.x, _groundLevelY, worldPosition.z);
            _isMoving = true;

            // Start the first hop immediately so the character doesn't glide
            if (_hopProgress < 0f)
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
            transform.position = new Vector3(
                transform.position.x,
                _groundLevelY,
                transform.position.z
            );
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
                transform.position = new Vector3(targetPosition.x, _groundLevelY, targetPosition.z);
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

            // Advance active hop — always advance regardless of speed,
            // so SmoothDamp's slow ramp-up doesn't cancel the hop.
            if (_hopProgress >= 0f)
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
            else if (frameDistance < _hopSpeedFloor)
            {
                // No active hop AND speed too low: decay residual height
                _currentHopHeight *= 0.85f;
                if (_currentHopHeight < 0.001f)
                {
                    _currentHopHeight = 0f;
                }
            }

            // ------------------------------------------------------------------
            // Apply final position: XZ from SmoothDamp, Y from ground + hop
            // ------------------------------------------------------------------
            transform.position = new Vector3(newX, _groundLevelY + _currentHopHeight, newZ);

            // ------------------------------------------------------------------
            // 8. Collision resolution — push out of any overlapping obstacles
            // ------------------------------------------------------------------
            ResolveObstacleOverlaps();
        }

        /// <summary>
        /// Check for overlaps with obstacles and push the character out.
        /// Uses Physics.OverlapSphere to find nearby colliders on the obstacle layer,
        /// then Physics.ComputePenetration to find the exact push direction and distance.
        /// </summary>
        private void ResolveObstacleOverlaps()
        {
            if (_obstacleLayerMask.value == 0 || _ownCollider == null)
            {
                return;
            }

            int overlapCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                _collisionRadius,
                _overlapResultsBuffer,
                _obstacleLayerMask
            );

            for (int index = 0; index < overlapCount; index++)
            {
                var overlappedCollider = _overlapResultsBuffer[index];

                // Skip our own collider and trigger colliders (proximity zones)
                if (overlappedCollider == _ownCollider || overlappedCollider.isTrigger)
                {
                    continue;
                }

                if (
                    Physics.ComputePenetration(
                        _ownCollider,
                        transform.position,
                        transform.rotation,
                        overlappedCollider,
                        overlappedCollider.transform.position,
                        overlappedCollider.transform.rotation,
                        out Vector3 pushDirection,
                        out float pushDistance
                    )
                )
                {
                    // Push only on XZ plane — Y is handled by hop system
                    Vector3 pushVector = pushDirection * pushDistance;
                    transform.position += new Vector3(pushVector.x, 0f, pushVector.z);
                }
            }
        }

        /// <summary>
        /// Idle state: decay smoothed velocity and hop height.
        /// </summary>
        private void HandleIdleDecay()
        {
            // Decay residual hop height smoothly
            if (_currentHopHeight > 0.001f)
            {
                _currentHopHeight *= 0.85f;
                if (_currentHopHeight < 0.001f)
                {
                    _currentHopHeight = 0f;
                }

                transform.position = new Vector3(
                    transform.position.x,
                    _groundLevelY + _currentHopHeight,
                    transform.position.z
                );
            }
            else if (transform.position.y != _groundLevelY)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    _groundLevelY,
                    transform.position.z
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
    }
}
