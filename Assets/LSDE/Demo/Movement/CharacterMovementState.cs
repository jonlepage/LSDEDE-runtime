using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Pure data class (POCO) holding the movement state for a single character.
    /// Ported from MovementState in the TS demo (movement.ts).
    /// Separated from the controller to follow Single Responsibility —
    /// the state holds data, the controller applies the math.
    /// Used by both player characters and NPCs.
    /// </summary>
    public class CharacterMovementState
    {
        /// <summary>
        /// Current movement target in world space. Null means no active movement.
        /// </summary>
        public Vector3? CurrentTarget { get; private set; }

        /// <summary>
        /// Movement speed in world units per second.
        /// </summary>
        public float MovementSpeed { get; set; }

        /// <summary>
        /// Distance traveled since the last hop started. When this exceeds
        /// <see cref="HopStrideDistance"/>, a new hop begins.
        /// </summary>
        public float DistanceSinceLastHop { get; set; }

        /// <summary>
        /// Current hop animation progress (0 to 1). -1 means no hop is active.
        /// The hop arc is: 4 * progress * (1 - progress) — a parabola peaking at 0.5.
        /// </summary>
        public float HopProgress { get; set; } = -1f;

        /// <summary>
        /// Smoothed horizontal velocity used for the inertia tilt effect.
        /// Lags behind the actual velocity with a lerp factor of 0.1,
        /// creating a lean that overshoots on direction changes.
        /// </summary>
        public float SmoothedHorizontalVelocity { get; set; }

        /// <summary>
        /// Distance in world units between consecutive hops.
        /// </summary>
        public float HopStrideDistance { get; set; }

        /// <summary>
        /// Maximum height of the hop arc in world units.
        /// </summary>
        public float HopMaxHeight { get; set; }

        /// <summary>
        /// Create a new movement state with the given parameters.
        /// </summary>
        public CharacterMovementState(
            float movementSpeed = 4f,
            float hopStrideDistance = 0.8f,
            float hopMaxHeight = 0.15f
        )
        {
            MovementSpeed = movementSpeed;
            HopStrideDistance = hopStrideDistance;
            HopMaxHeight = hopMaxHeight;
        }

        /// <summary>
        /// Set a new movement target. The character will begin moving toward it.
        /// </summary>
        public void SetTarget(Vector3 targetPosition)
        {
            CurrentTarget = targetPosition;
        }

        /// <summary>
        /// Clear the movement target. The character will stop moving.
        /// </summary>
        public void ClearTarget()
        {
            CurrentTarget = null;
        }

        /// <summary>
        /// Reset all animation state (hop, inertia) to idle values.
        /// Called when the character arrives at the target.
        /// </summary>
        public void ResetAnimationState()
        {
            DistanceSinceLastHop = 0f;
            HopProgress = -1f;
            SmoothedHorizontalVelocity = 0f;
        }
    }
}
