using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Smooth camera follow for the player character.
    /// Uses LateUpdate to run after all character movement has been applied,
    /// preventing jitter between the camera and the player sprite.
    /// The camera maintains a configurable offset and smoothly interpolates
    /// toward the target position each frame.
    /// </summary>
    public class CameraFollowController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The Transform to follow (usually the player character l4).")]
        private Transform _targetToFollow;

        [SerializeField]
        [Tooltip(
            "Offset from the target position in world space. "
                + "X = left/right, Y = height, Z = distance behind."
        )]
        private Vector3 _cameraOffset = new Vector3(0f, 5f, -8f);

        [SerializeField]
        [Tooltip(
            "How quickly the camera catches up to the target. "
                + "Higher = snappier, lower = more cinematic lag."
        )]
        private float _followSmoothSpeed = 5f;

        /// <summary>
        /// LateUpdate runs after all Update calls, ensuring the camera moves
        /// after the player has finished moving for this frame.
        /// </summary>
        private void LateUpdate()
        {
            if (_targetToFollow == null)
            {
                return;
            }

            Vector3 desiredCameraPosition = _targetToFollow.position + _cameraOffset;

            transform.position = Vector3.Lerp(
                transform.position,
                desiredCameraPosition,
                _followSmoothSpeed * Time.deltaTime
            );

            transform.LookAt(_targetToFollow);
        }
    }
}
