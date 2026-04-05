using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Applies click-to-move movement with ease-out deceleration, hop sautillant (bouncy walk),
    /// and inertia tilt. Ported from the TS demo movement.ts.
    /// Works on any character — player or NPC. The input source is external
    /// (PlayerClickToMoveInput for the player, scripts for NPCs).
    ///
    /// The hop is applied to a child Transform (<see cref="_spriteContainer"/>)
    /// so the hop visual offset doesn't affect the character's real world position.
    /// </summary>
    public class CharacterMovementController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField]
        [Tooltip("Movement speed in world units per second.")]
        private float _movementSpeed = 4f;

        [Header("Hop (Bouncy Walk)")]
        [SerializeField]
        [Tooltip("Distance in world units between consecutive hops.")]
        private float _hopStrideDistance = 0.8f;

        [SerializeField]
        [Tooltip("Maximum height of the hop arc in world units.")]
        private float _hopMaxHeight = 0.15f;

        [Header("Inertia")]
        [SerializeField]
        [Tooltip(
            "How much the sprite tilts in the movement direction (radians per unit velocity)."
        )]
        private float _inertiaTiltFactor = 0.055f;

        [SerializeField]
        [Tooltip("Lag factor for smoothed velocity — lower = more inertia feel.")]
        private float _inertiaVelocityLerp = 0.1f;

        [Header("References")]
        [SerializeField]
        [Tooltip(
            "Child Transform containing the visual sprite. "
                + "The hop offset is applied to this, not to the root, "
                + "so the character's real world position stays on the ground plane."
        )]
        private Transform _spriteContainer;

        private const float ArrivalThreshold = 0.1f;
        private const float HopSpeedFloor = 0.02f;
        private const float InertiaDecayFactor = 0.85f;

        /// <summary>
        /// The movement state data for this character.
        /// External scripts (input, AI) call <see cref="SetMovementTarget"/> to set the target.
        /// </summary>
        public CharacterMovementState MovementState { get; private set; }

        private void Awake()
        {
            MovementState = new CharacterMovementState(
                _movementSpeed,
                _hopStrideDistance,
                _hopMaxHeight
            );

            // Default to the first child if no sprite container is assigned
            if (_spriteContainer == null && transform.childCount > 0)
            {
                _spriteContainer = transform.GetChild(0);
            }
        }

        /// <summary>
        /// Set a movement target in world space. The character will move toward it
        /// with ease-out deceleration, hop animation, and inertia tilt.
        /// </summary>
        public void SetMovementTarget(Vector3 worldPosition)
        {
            MovementState.SetTarget(worldPosition);
        }

        /// <summary>
        /// Stop all movement immediately.
        /// </summary>
        public void StopMovement()
        {
            MovementState.ClearTarget();
            MovementState.ResetAnimationState();
            ApplyHopOffset(0f);
            transform.rotation = Quaternion.identity;
        }

        private void Update()
        {
            if (MovementState.CurrentTarget == null)
            {
                ApplyIdleInertiaDecay();
                return;
            }

            Vector3 targetPosition = MovementState.CurrentTarget.Value;
            Vector3 currentPosition = transform.position;

            // Calculate distance remaining on the XZ plane
            float deltaX = targetPosition.x - currentPosition.x;
            float deltaZ = targetPosition.z - currentPosition.z;
            float distanceRemaining = Mathf.Sqrt(deltaX * deltaX + deltaZ * deltaZ);

            // Arrived at target
            if (distanceRemaining < ArrivalThreshold)
            {
                transform.position = new Vector3(
                    targetPosition.x,
                    currentPosition.y,
                    targetPosition.z
                );
                ApplyHopOffset(0f);
                transform.rotation = Quaternion.identity;
                MovementState.ResetAnimationState();
                MovementState.ClearTarget();
                return;
            }

            // Calculate eased step size for this frame
            float totalStepSize = MovementState.MovementSpeed * Time.deltaTime;

            float newPositionX = ComputeEasedStep(
                currentPosition.x,
                targetPosition.x,
                distanceRemaining,
                totalStepSize
            );
            float newPositionZ = ComputeEasedStep(
                currentPosition.z,
                targetPosition.z,
                distanceRemaining,
                totalStepSize
            );

            float frameDeltaX = newPositionX - currentPosition.x;
            float frameDeltaZ = newPositionZ - currentPosition.z;
            float frameDistance = Mathf.Sqrt(frameDeltaX * frameDeltaX + frameDeltaZ * frameDeltaZ);

            transform.position = new Vector3(newPositionX, currentPosition.y, newPositionZ);

            // Inertia tilt — smoothed horizontal velocity creates a lean effect
            MovementState.SmoothedHorizontalVelocity +=
                (frameDeltaX - MovementState.SmoothedHorizontalVelocity) * _inertiaVelocityLerp;
            transform.rotation = Quaternion.Euler(
                0f,
                0f,
                MovementState.SmoothedHorizontalVelocity * _inertiaTiltFactor * Mathf.Rad2Deg
            );

            // Hop animation — small bouncy hops while moving
            ApplyHopAnimation(frameDistance);
        }

        /// <summary>
        /// When idle (no target), decay the inertia smoothly so the sprite
        /// settles back to upright position.
        /// </summary>
        private void ApplyIdleInertiaDecay()
        {
            MovementState.SmoothedHorizontalVelocity *= InertiaDecayFactor;
            if (Mathf.Abs(MovementState.SmoothedHorizontalVelocity) < 0.001f)
            {
                MovementState.SmoothedHorizontalVelocity = 0f;
                transform.rotation = Quaternion.identity;
            }
            else
            {
                transform.rotation = Quaternion.Euler(
                    0f,
                    0f,
                    MovementState.SmoothedHorizontalVelocity * _inertiaTiltFactor * Mathf.Rad2Deg
                );
            }
        }

        /// <summary>
        /// Apply the hop (bouncy walk) animation. Ported from movement.ts lines 186-213.
        /// Uses a parabolic arc: 4 * progress * (1 - progress), peaking at progress = 0.5.
        /// </summary>
        private void ApplyHopAnimation(float frameDistance)
        {
            float strideDistance = MovementState.HopStrideDistance;
            float maxHeight = MovementState.HopMaxHeight;

            if (frameDistance < HopSpeedFloor)
            {
                // Moving too slowly — dampen hop
                float currentHopOffset =
                    _spriteContainer != null ? _spriteContainer.localPosition.y : 0f;
                ApplyHopOffset(currentHopOffset * InertiaDecayFactor);
                MovementState.HopProgress = -1f;
                return;
            }

            MovementState.DistanceSinceLastHop += frameDistance;

            // Start a new hop when enough distance has been covered
            if (
                MovementState.HopProgress < 0f
                && MovementState.DistanceSinceLastHop >= strideDistance
            )
            {
                MovementState.HopProgress = 0f;
                MovementState.DistanceSinceLastHop = 0f;
            }

            // Advance hop animation
            if (MovementState.HopProgress >= 0f)
            {
                MovementState.HopProgress += frameDistance / strideDistance;

                if (MovementState.HopProgress >= 1f)
                {
                    MovementState.HopProgress = -1f;
                    ApplyHopOffset(0f);
                }
                else
                {
                    // Parabolic arc: peaks at 0.5, returns to 0 at 0 and 1
                    float arcHeight =
                        4f * MovementState.HopProgress * (1f - MovementState.HopProgress);
                    ApplyHopOffset(arcHeight * maxHeight);
                }
            }
        }

        /// <summary>
        /// Apply the hop Y offset to the sprite container (not the root transform).
        /// </summary>
        private void ApplyHopOffset(float verticalOffset)
        {
            if (_spriteContainer == null)
            {
                return;
            }

            var localPosition = _spriteContainer.localPosition;
            _spriteContainer.localPosition = new Vector3(
                localPosition.x,
                verticalOffset,
                localPosition.z
            );
        }

        /// <summary>
        /// Compute the next position for one axis using ease-out interpolation.
        /// Ported from movement.ts computeEasedStep (lines 83-101).
        /// The character moves fast initially and decelerates smoothly near the target.
        /// </summary>
        private static float ComputeEasedStep(
            float currentPosition,
            float targetPosition,
            float distanceRemaining,
            float totalStepSize
        )
        {
            if (distanceRemaining < ArrivalThreshold)
            {
                return targetPosition;
            }

            float directionSign = targetPosition > currentPosition ? 1f : -1f;
            float axisDistance = Mathf.Abs(targetPosition - currentPosition);
            float axisRatio = axisDistance / distanceRemaining;

            // Ease-out quadratic: 1 - (1 - t)²
            float proximityRatio = Mathf.Min(distanceRemaining / 4f, 1f);
            float easedSpeed = EaseOutQuadratic(proximityRatio) * totalStepSize * axisRatio;
            float clampedStep = Mathf.Min(easedSpeed, axisDistance);

            return currentPosition + directionSign * clampedStep;
        }

        /// <summary>
        /// Ease-out quadratic: fast start, smooth deceleration.
        /// f(t) = 1 - (1 - t)²
        /// </summary>
        private static float EaseOutQuadratic(float progressRatio)
        {
            return 1f - (1f - progressRatio) * (1f - progressRatio);
        }
    }
}
