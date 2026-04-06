using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// A clickable item in the scene that adds itself to the game state inventory when picked up.
    /// Attach to any GameObject with a <see cref="Collider"/> (3D primitives like Cube, Sphere,
    /// Capsule come with one automatically).
    ///
    /// When the player clicks the object, it calls <see cref="DemoGameState.AddItem"/> and
    /// destroys itself. The color tint is applied at startup so you can use a plain primitive
    /// without creating a custom material.
    ///
    /// Demo usage: create a Cube, scale it to (0.3, 0.3, 0.3), attach this script, assign
    /// the <see cref="DemoGameState"/> reference, and set <see cref="_itemKey"/> to
    /// <c>lsdeDictionaryinventory.carrot</c>.
    /// </summary>
    public class PickableItem : MonoBehaviour
    {
        [Header("Item Configuration")]
        [SerializeField]
        [Tooltip(
            "The inventory key for this item (e.g. lsdeDictionaryinventory.carrot). "
                + "Must match the condition key used in LSDE blueprints."
        )]
        private string _itemKey = "carrot";

        [SerializeField]
        [Tooltip("How many of this item to add to inventory when picked up.")]
        private int _quantity = 1;

        [Header("Visual")]
        [SerializeField]
        [Tooltip("Tint color applied to the object's material at startup.")]
        private Color _tintColor = new Color(1f, 0.5f, 0f, 1f); // Orange

        [Header("Dependencies")]
        [SerializeField]
        [Tooltip("Reference to the game state that tracks inventory.")]
        private DemoGameState _gameState;

        private void Start()
        {
            ApplyTintColor();
        }

        /// <summary>
        /// Called by Unity when the player clicks on this object's collider.
        /// Requires a <see cref="Collider"/> component on this GameObject.
        /// </summary>
        private void OnMouseDown()
        {
            if (_gameState == null)
            {
                Debug.LogError(
                    $"[LSDE Pickable] DemoGameState is not assigned on {gameObject.name}. "
                        + "Cannot add item to inventory."
                );
                return;
            }

            _gameState.AddItem(_itemKey, _quantity);

            Debug.Log(
                $"[LSDE Pickable] Picked up {_quantity}x {_itemKey} — destroying {gameObject.name}."
            );

            Destroy(gameObject);
        }

        /// <summary>
        /// Apply the configured tint color to this object's renderer material.
        /// Creates a material instance so other objects sharing the same base material
        /// are not affected.
        /// </summary>
        private void ApplyTintColor()
        {
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
    }
}
