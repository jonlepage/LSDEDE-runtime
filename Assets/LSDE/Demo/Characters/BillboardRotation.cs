using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Forces this GameObject to always face the camera (billboard effect).
    /// Copies the camera's rotation directly rather than using LookAt —
    /// LookAt would point the object toward the camera position (perspective distortion),
    /// while copying rotation keeps the sprite perfectly flat and parallel to the camera plane.
    ///
    /// Attach this to any object that should always face the camera (speech bubbles, 2D sprites in 3D, etc.).
    /// Uses LateUpdate to run after all other transforms have been updated.
    /// </summary>
    public class BillboardRotation : MonoBehaviour
    {
        private Transform _cachedCameraTransform;

        private void Start()
        {
            if (Camera.main != null)
            {
                _cachedCameraTransform = Camera.main.transform;
            }
        }

        /// <summary>
        /// LateUpdate runs after all Update calls, ensuring the billboard rotation
        /// is applied after any camera or character movement.
        /// </summary>
        private void LateUpdate()
        {
            if (_cachedCameraTransform == null)
            {
                if (Camera.main != null)
                {
                    _cachedCameraTransform = Camera.main.transform;
                }
                return;
            }

            transform.rotation = _cachedCameraTransform.rotation;
        }
    }
}
