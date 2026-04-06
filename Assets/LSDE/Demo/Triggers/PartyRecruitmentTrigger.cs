using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Party recruitment trigger — detects player proximity and recruits the NPC on click.
    /// Port of the TS <c>setup-party-recruitment.ts</c> pattern:
    ///
    /// 1. Player enters the interaction zone → hint appears ("Rejoins !")
    /// 2. Player clicks on the NPC → character added to party via
    ///    <see cref="DemoGameState.AddToParty"/>, hint hidden permanently
    /// 3. Once recruited, the NPC starts following the player (handled by
    ///    <see cref="PartyFollowController"/> which listens to the party event)
    ///
    /// Follows the same architectural pattern as <see cref="DialogueProximityTrigger"/>:
    /// SphereCollider trigger zone + OnTriggerEnter/Exit + public TryX() method
    /// called by <see cref="PlayerClickToMoveInput"/>.
    ///
    /// Requirements (same as DialogueProximityTrigger):
    /// - This NPC must have a Collider (for click detection) on the "Interactable" layer
    /// - The player must have a Collider + Rigidbody (isKinematic) and the tag "Player"
    /// - A SphereCollider (isTrigger) is added automatically at Awake for proximity detection
    /// - The NPC must have a <see cref="DialogueCharacterMarker"/> for character ID lookup
    ///
    /// Setup in Unity Editor:
    /// 1. Attach this script to a recruitable NPC GameObject (e.g. l1, l2, l3)
    /// 2. Assign the <see cref="_gameState"/> reference (DemoGameState)
    /// 3. Assign the <see cref="_interactionHintDisplay"/> reference (InteractionHintDisplay child)
    /// 4. Ensure the NPC has a Collider and is on the "Interactable" layer
    /// 5. Ensure the player has the "Player" tag, a Collider, and a Rigidbody (isKinematic)
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PartyRecruitmentTrigger : MonoBehaviour
    {
        [Header("Trigger Zone")]
        [SerializeField]
        [Tooltip(
            "Radius of the recruitment zone around this NPC. "
                + "The player must enter this zone for the hint to appear. "
                + "A SphereCollider (isTrigger) is created automatically with this radius."
        )]
        private float _interactionRadius = 3f;

        [Header("References")]
        [SerializeField]
        [Tooltip(
            "Reference to the DemoGameState that manages party membership. "
                + "AddToParty() is called when the player recruits this NPC."
        )]
        private DemoGameState _gameState;

        [SerializeField]
        [Tooltip("The interaction hint display component that shows/hides above this NPC.")]
        private InteractionHintDisplay _interactionHintDisplay;

        private DialogueCharacterMarker _characterMarker;
        private bool _isPlayerInZone;
        private bool _hasBeenRecruited;

        /// <summary>
        /// Whether this NPC can currently be recruited by a click.
        /// True when the player is in the zone, the NPC has not been recruited yet,
        /// and the NPC is not already in the party (e.g. from the Inspector list).
        /// </summary>
        public bool CanRecruit =>
            _isPlayerInZone
            && !_hasBeenRecruited
            && _gameState != null
            && _characterMarker != null
            && !_gameState.IsInParty(_characterMarker.LsdeCharacterId);

        private void Awake()
        {
            // Get the character marker to know which LSDE character this NPC represents
            _characterMarker = GetComponent<DialogueCharacterMarker>();

            if (_characterMarker == null)
            {
                Debug.LogError(
                    $"[LSDE Demo] PartyRecruitmentTrigger on '{gameObject.name}' "
                        + "requires a DialogueCharacterMarker component."
                );
            }

            // Create the proximity detection sphere automatically
            // (same pattern as DialogueProximityTrigger)
            var proximitySphere = gameObject.AddComponent<SphereCollider>();
            proximitySphere.isTrigger = true;
            proximitySphere.radius = _interactionRadius;

            // If already in party at start (e.g. set from Inspector), disable recruitment
            if (
                _characterMarker != null
                && _gameState != null
                && _gameState.IsInParty(_characterMarker.LsdeCharacterId)
            )
            {
                _hasBeenRecruited = true;
            }
        }

        /// <summary>
        /// Called by <see cref="PlayerClickToMoveInput"/> when the player clicks on this NPC.
        /// Adds the character to the party if recruitment is allowed.
        /// </summary>
        /// <returns>True if the NPC was recruited, false if recruitment was not allowed.</returns>
        public bool TryRecruit()
        {
            if (!CanRecruit)
            {
                return false;
            }

            _hasBeenRecruited = true;
            HideRecruitmentHint();

            _gameState.AddToParty(_characterMarker.LsdeCharacterId);

            Debug.Log(
                $"[LSDE Demo] Recruited {_characterMarker.LsdeCharacterId} "
                    + $"from '{gameObject.name}'!"
            );

            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _isPlayerInZone = true;

            if (!_hasBeenRecruited)
            {
                ShowRecruitmentHint();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _isPlayerInZone = false;
            HideRecruitmentHint();
        }

        private void ShowRecruitmentHint()
        {
            if (_interactionHintDisplay != null)
            {
                _interactionHintDisplay.ShowHint();
            }
        }

        private void HideRecruitmentHint()
        {
            if (_interactionHintDisplay != null)
            {
                _interactionHintDisplay.HideHint();
            }
        }

        /// <summary>
        /// Reset the trigger to its initial state so the NPC can be recruited again.
        /// Called by <see cref="WebGlSceneController"/> when switching between demo scenes.
        /// </summary>
        public void ResetTrigger()
        {
            _hasBeenRecruited = false;
            _isPlayerInZone = false;
            HideRecruitmentHint();
        }

        /// <summary>
        /// Draw recruitment radius as a wire sphere in the Scene view.
        /// Cyan when available, grey when already recruited.
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = _hasBeenRecruited
                ? new Color(0.5f, 0.5f, 0.5f, 0.2f)
                : new Color(0f, 1f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _interactionRadius);
        }
    }
}
