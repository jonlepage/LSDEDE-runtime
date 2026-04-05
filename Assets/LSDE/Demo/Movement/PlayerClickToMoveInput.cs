using UnityEngine;
using UnityEngine.InputSystem;

namespace LSDE.Demo
{
    /// <summary>
    /// Reads mouse click input and sets the movement target on the
    /// <see cref="CharacterMovementController"/> by raycasting onto the ground plane.
    /// Attach this ONLY to the player character — NPCs receive movement targets
    /// from scripts or AI, not from mouse input.
    ///
    /// Clicking simultaneously advances dialogue (via DialogueClickAdvancer)
    /// AND sets a movement target. Both systems react to the same click.
    /// </summary>
    public class PlayerClickToMoveInput : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The movement controller on this character.")]
        private CharacterMovementController _movementController;

        [SerializeField]
        [Tooltip(
            "Layer mask for the ground plane. Only surfaces on this layer "
                + "will be detected by the click raycast."
        )]
        private LayerMask _groundLayerMask;

        [SerializeField]
        [Tooltip("Maximum raycast distance from the camera.")]
        private float _maximumRaycastDistance = 100f;

        private Camera _cachedMainCamera;

        private void Start()
        {
            _cachedMainCamera = Camera.main;

            if (_movementController == null)
            {
                _movementController = GetComponent<CharacterMovementController>();
            }
        }

        private void Update()
        {
            var currentMouse = Mouse.current;
            if (currentMouse == null || !currentMouse.leftButton.wasPressedThisFrame)
            {
                return;
            }

            if (_cachedMainCamera == null)
            {
                _cachedMainCamera = Camera.main;
                if (_cachedMainCamera == null)
                {
                    return;
                }
            }

            // Cast a ray from the camera through the mouse position onto the ground
            Vector2 mouseScreenPosition = currentMouse.position.ReadValue();
            Ray rayFromCamera = _cachedMainCamera.ScreenPointToRay(mouseScreenPosition);

            Debug.Log(
                $"[Movement] Click at screen ({mouseScreenPosition.x:F0}, {mouseScreenPosition.y:F0}), "
                    + $"ray origin={rayFromCamera.origin}, dir={rayFromCamera.direction}, "
                    + $"groundMask={_groundLayerMask.value}"
            );

            if (
                Physics.Raycast(
                    rayFromCamera,
                    out RaycastHit groundHitInfo,
                    _maximumRaycastDistance,
                    _groundLayerMask
                )
            )
            {
                // Keep the character's current Y position (ground level)
                Vector3 targetWorldPosition = new Vector3(
                    groundHitInfo.point.x,
                    transform.position.y,
                    groundHitInfo.point.z
                );

                Debug.Log(
                    $"[Movement] Raycast hit '{groundHitInfo.collider.gameObject.name}' "
                        + $"at {targetWorldPosition}, layer={groundHitInfo.collider.gameObject.layer}"
                );

                _movementController.SetMovementTarget(targetWorldPosition);
            }
            else
            {
                Debug.LogWarning(
                    "[Movement] Raycast missed — no ground detected. "
                        + "Check that the Ground plane has a Collider and is on the 'Ground' layer."
                );
            }
        }
    }
}
