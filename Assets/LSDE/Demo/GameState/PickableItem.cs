using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// An item lying in the world that the player walks up to and clicks to collect.
    ///
    /// <para>It follows the same pattern as <see cref="PartyRecruitmentTrigger"/> and
    /// <see cref="DialogueProximityTrigger"/>: a <c>SphereCollider</c> trigger added at
    /// <c>Awake</c> for proximity, <c>OnTriggerEnter</c>/<c>Exit</c> to know when the player is in
    /// range, and a public <see cref="TryPickUp"/> that <see cref="PlayerClickToMoveInput"/> calls
    /// when its raycast lands here.</para>
    ///
    /// <para>It used to answer Unity's legacy <c>OnMouseDown</c> through a <c>BoxCollider2D</c> on
    /// the Default layer instead — the only clickable thing in the project that did. Two things
    /// were wrong with that. The 2D collider was a sliver 0.11 units tall against a carrot of 0.16,
    /// so a click a few pixels off the centre missed and the carrot looked dead; and 2D picking
    /// runs beside the 3D raycast that drives every other interaction, so a carrot standing behind
    /// a rabbit could be taken through it. Sharing one collider layer and one raycast is what makes
    /// them behave the same.</para>
    ///
    /// <para>Proximity is also what the reference PixiJS demo requires
    /// (<c>setup-carrot-field.ts</c>, <c>PICKUP_DISTANCE</c>): an item is picked up by GOING to it,
    /// not by clicking it from across the map.</para>
    ///
    /// <para>Setup in Unity Editor: put this on a GameObject that carries a <b>3D Collider</b>
    /// (a BoxCollider) on the <b>Interactable</b> layer, then drag the DemoGameState onto the
    /// <c>Game State</c> field in the Inspector.</para>
    /// </summary>
    public class PickableItem : MonoBehaviour
    {
        [Header("Item Configuration")]
        [SerializeField]
        [Tooltip(
            "The inventory entry key for this item, as the blueprint cites it "
                + "(e.g. carrot). "
                + "Must match the condition key used in LSDE blueprints."
        )]
        private string _itemKey = "carrot";

        [SerializeField]
        [Tooltip("How many of this item to add to inventory when picked up.")]
        private int _quantity = 1;

        [Header("Interaction")]
        [SerializeField]
        [Tooltip(
            "How close the player must be to collect this item. A SphereCollider (isTrigger) "
                + "is created automatically with this radius."
        )]
        private float _interactionRadius = 1.6f;

        [SerializeField]
        [Tooltip(
            "Optional hint shown above the item while the player is in range. "
                + "Leave empty for no hint."
        )]
        private InteractionHintDisplay _interactionHintDisplay;

        [Header("Visual")]
        [SerializeField]
        [Tooltip("Tint color applied to the object at startup.")]
        private Color _tintColor = new Color(1f, 0.5f, 0f, 1f); // Orange

        [Header("Dependencies")]
        [SerializeField]
        [Tooltip("Reference to the game state that tracks inventory.")]
        private DemoGameState _gameState;

        private bool _isPlayerInRange;
        private bool _hasBeenPickedUp;

        /// <summary>
        /// Whether a click right now would collect this item: the player is close enough and it
        /// has not been taken already.
        /// </summary>
        public bool CanPickUp => _isPlayerInRange && !_hasBeenPickedUp;

        private void Awake()
        {
            // Proximity zone, created here rather than in the Inspector so a carrot dropped into
            // the scene needs no setup beyond its own collider.
            var proximitySphere = gameObject.AddComponent<SphereCollider>();
            proximitySphere.isTrigger = true;
            proximitySphere.radius = _interactionRadius;
        }

        private void Start()
        {
            ApplyTintColor();
        }

        /// <summary>
        /// Collect the item, if the player is close enough.
        /// Called by <see cref="PlayerClickToMoveInput"/> when its raycast lands on this object.
        /// </summary>
        /// <returns>
        /// True when the item was collected — which tells the caller to stop, so the same click
        /// does not also send the player walking. False leaves the click to whatever comes next.
        /// </returns>
        public bool TryPickUp()
        {
            if (!CanPickUp)
            {
                return false;
            }

            if (_gameState == null)
            {
                Debug.LogError(
                    $"[LSDE Pickable] DemoGameState is not assigned on {gameObject.name}. "
                        + "Cannot add item to inventory."
                );
                return false;
            }

            _hasBeenPickedUp = true;
            _gameState.AddItem(_itemKey, _quantity);

            Debug.Log(
                $"[LSDE Pickable] Picked up {_quantity}x {_itemKey} — destroying {gameObject.name}."
            );

            Destroy(gameObject);
            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _isPlayerInRange = true;

            if (_interactionHintDisplay != null && !_hasBeenPickedUp)
            {
                _interactionHintDisplay.ShowHint();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _isPlayerInRange = false;

            if (_interactionHintDisplay != null)
            {
                _interactionHintDisplay.HideHint();
            }
        }

        /// <summary>
        /// Apply the configured tint.
        ///
        /// <para>A <c>SpriteRenderer</c> is tinted through its own <c>color</c>, which costs
        /// nothing; touching <c>material</c> would instance the material and drop the sprite out of
        /// its batch. Anything else falls back to the material.</para>
        /// </summary>
        private void ApplyTintColor()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = _tintColor;
                return;
            }

            var objectRenderer = GetComponent<Renderer>();
            if (objectRenderer == null)
            {
                Debug.LogWarning(
                    $"[LSDE Pickable] No Renderer found on {gameObject.name}. Cannot apply tint."
                );
                return;
            }

            // Accessing .material (not .sharedMaterial) creates a per-instance copy,
            // so this tint only affects THIS object.
            objectRenderer.material.color = _tintColor;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, _interactionRadius);
        }
    }
}
