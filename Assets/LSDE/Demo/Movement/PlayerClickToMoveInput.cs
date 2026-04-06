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
    /// Click priority:
    /// 1. If the click hits an interactable NPC (Interactable layer) with an active
    ///    <see cref="DialogueProximityTrigger"/> → trigger dialogue, NO movement.
    /// 2. Otherwise, raycast onto Ground layer → set movement target.
    ///
    /// Clicking also advances dialogue (via DialogueClickAdvancer) regardless.
    /// </summary>
    public class PlayerClickToMoveInput : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The movement controller on this character.")]
        private CharacterMovementController _movementController;

        [SerializeField]
        [Tooltip(
            "Layer mask for the ground plane. Only surfaces on this layer "
                + "will be detected by the movement raycast."
        )]
        private LayerMask _groundLayerMask;

        [SerializeField]
        [Tooltip(
            "Layer mask for interactable objects (NPCs with DialogueProximityTrigger). "
                + "Clicks on this layer take priority over ground movement."
        )]
        private LayerMask _interactableLayerMask;

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

            Vector2 mouseScreenPosition = currentMouse.position.ReadValue();
            Ray rayFromCamera = _cachedMainCamera.ScreenPointToRay(mouseScreenPosition);

            // Priority 1: Check if the click hit an interactable NPC
            // Uses RaycastAll because the NPC may have multiple colliders
            // (body collider + trigger SphereCollider from DialogueProximityTrigger).
            // We accept hits on any of them — the trigger's CanInteract state
            // determines whether the dialogue actually starts.
            if (_interactableLayerMask.value != 0)
            {
                var interactableHits = Physics.RaycastAll(
                    rayFromCamera,
                    _maximumRaycastDistance,
                    _interactableLayerMask
                );

                foreach (var hitInfo in interactableHits)
                {
                    var proximityTrigger =
                        hitInfo.collider.GetComponentInParent<DialogueProximityTrigger>();

                    if (proximityTrigger != null && proximityTrigger.TryTriggerDialogue())
                    {
                        return;
                    }
                }
            }

            // Priority 2: Raycast onto the ground for movement
            if (
                Physics.Raycast(
                    rayFromCamera,
                    out RaycastHit groundHitInfo,
                    _maximumRaycastDistance,
                    _groundLayerMask
                )
            )
            {
                Vector3 targetWorldPosition = new Vector3(
                    groundHitInfo.point.x,
                    transform.position.y,
                    groundHitInfo.point.z
                );

                _movementController.SetMovementTarget(targetWorldPosition);
            }
            // If both raycasts missed, no action
        }
    }
}
