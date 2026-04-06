using LSDE.Runtime;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Spatial dialogue trigger — detects player proximity and makes the NPC clickable.
    /// Faithful port of the TS setup-dialogue-trigger.ts pattern:
    ///
    /// 1. Player enters the interaction zone → hint appears, NPC becomes clickable
    /// 2. Player clicks on the NPC → dialogue scene launches, hint disappears
    /// 3. Dialogue scene ends → trigger re-arms automatically
    /// 4. Player leaves the zone → hint disappears
    ///
    /// Uses Unity's physics trigger system (SphereCollider + OnTriggerEnter/Exit)
    /// instead of polling distance each frame.
    ///
    /// Requirements:
    /// - This NPC must have a Collider (for click detection) on the "Interactable" layer
    /// - The player must have a Collider + Rigidbody (isKinematic) and the tag "Player"
    /// - A SphereCollider (isTrigger) is added automatically at Awake for proximity detection
    ///
    /// Setup in Unity Editor:
    /// 1. Attach this script to the NPC GameObject
    /// 2. Assign the DemoSceneTrigger and InteractionHintDisplay references
    /// 3. Set the scene UUID to launch (use LSDE_SCENES constants)
    /// 4. Ensure the NPC has a Collider and is on the "Interactable" layer
    /// 5. Ensure the player has the "Player" tag, a Collider, and a Rigidbody (isKinematic)
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DialogueProximityTrigger : MonoBehaviour
    {
        [Header("Trigger Zone")]
        [SerializeField]
        [Tooltip(
            "Radius of the interaction zone around this NPC. "
                + "The player must enter this zone for the hint to appear. "
                + "A SphereCollider (isTrigger) is created automatically with this radius."
        )]
        private float _interactionRadius = 3f;

        [Header("Dialogue")]
        [SerializeField]
        [Tooltip(
            "Reference to the DemoSceneTrigger that manages engine initialization "
                + "and scene launching."
        )]
        private DemoSceneTrigger _demoSceneTrigger;

        [SerializeField]
        [LsdeSceneSelector]
        [Tooltip("The LSDE scene to launch when the player interacts with this NPC.")]
        private string _sceneUuidToLaunch;

        [Header("Visual Feedback")]
        [SerializeField]
        [Tooltip("The interaction hint display component that shows/hides above this NPC.")]
        private InteractionHintDisplay _interactionHintDisplay;

        private bool _isPlayerInZone;
        private bool _hasTriggeredDialogue;

        /// <summary>
        /// Whether this trigger is ready to be activated by a click.
        /// True when the player is in the zone, dialogue is not active, and not already triggered.
        /// </summary>
        public bool CanInteract =>
            _isPlayerInZone
            && !_hasTriggeredDialogue
            && _demoSceneTrigger != null
            && !_demoSceneTrigger.IsDialogueSceneActive;

        private void Awake()
        {
            // Create the proximity detection sphere automatically
            var proximitySphere = gameObject.AddComponent<SphereCollider>();
            proximitySphere.isTrigger = true;
            proximitySphere.radius = _interactionRadius;
        }

        private void Update()
        {
            // Re-arm the trigger when a dialogue scene finishes
            // (the player might still be in the zone)
            if (
                _hasTriggeredDialogue
                && _demoSceneTrigger != null
                && !_demoSceneTrigger.IsDialogueSceneActive
            )
            {
                _hasTriggeredDialogue = false;

                // Re-show hint if player is still in zone
                if (_isPlayerInZone)
                {
                    ShowInteractionHint();
                }
            }
        }

        /// <summary>
        /// Called by <see cref="PlayerClickToMoveInput"/> when the player clicks on this NPC.
        /// Launches the configured dialogue scene if interaction is allowed.
        /// </summary>
        /// <returns>True if the dialogue was triggered, false if interaction was not allowed.</returns>
        public bool TryTriggerDialogue()
        {
            if (!CanInteract)
            {
                return false;
            }

            if (string.IsNullOrEmpty(_sceneUuidToLaunch))
            {
                Debug.LogWarning(
                    $"[LSDE Demo] DialogueProximityTrigger on '{gameObject.name}' "
                        + "has no scene UUID configured."
                );
                return false;
            }

            _hasTriggeredDialogue = true;
            HideInteractionHint();
            _demoSceneTrigger.LaunchDialogueScene(_sceneUuidToLaunch);

            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _isPlayerInZone = true;

            if (!_hasTriggeredDialogue && !_demoSceneTrigger.IsDialogueSceneActive)
            {
                ShowInteractionHint();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _isPlayerInZone = false;
            HideInteractionHint();
        }

        private void ShowInteractionHint()
        {
            if (_interactionHintDisplay != null)
            {
                _interactionHintDisplay.ShowHint();
            }
        }

        private void HideInteractionHint()
        {
            if (_interactionHintDisplay != null)
            {
                _interactionHintDisplay.HideHint();
            }
        }
    }
}
