using TMPro;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Displays an interaction hint (e.g. a speech emoji) above a character
    /// to indicate that the player can interact with them.
    /// The hint automatically faces the camera (billboard) each frame.
    ///
    /// Visibility uses <see cref="TextMeshPro.enabled"/> instead of <c>SetActive</c>
    /// to avoid the TMP first-activation lag spike. The GameObject stays active
    /// so TMP initializes during scene load.
    ///
    /// This component is controlled by <see cref="DialogueProximityTrigger"/>
    /// which calls <see cref="ShowHint"/> and <see cref="HideHint"/> based on
    /// proximity and dialogue state.
    ///
    /// Setup in Unity Editor:
    /// 1. Create a child GameObject under the NPC named "InteractionHint"
    /// 2. Add a TextMeshPro (3D) component to it with the hint text (e.g. "...")
    /// 3. Attach this script to the same GameObject
    /// 4. Position it above the character's head
    /// </summary>
    public class InteractionHintDisplay : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The TextMeshPro component displaying the hint text.")]
        private TextMeshPro _hintText;

        [SerializeField]
        [Tooltip("The text displayed as the interaction hint.")]
        private string _displayText = "\ud83d\udcac";

        [SerializeField]
        [Tooltip("Gentle floating animation amplitude in world units.")]
        private float _floatingAmplitude = 0.08f;

        [SerializeField]
        [Tooltip("Floating animation speed (cycles per second).")]
        private float _floatingSpeed = 1.5f;

        private Camera _cachedMainCamera;
        private Vector3 _initialLocalPosition;
        private bool _isHintVisible;
        private MeshRenderer _meshRenderer;

        private void Awake()
        {
            _initialLocalPosition = transform.localPosition;
            _meshRenderer = GetComponent<MeshRenderer>();

            if (_hintText != null)
            {
                _hintText.text = _displayText;
            }

            // Start hidden via renderer — the GameObject stays active
            // so TMP initializes its font atlas during scene load (no lag spike later).
            SetHintRendererVisible(false);
        }

        /// <summary>
        /// Show the interaction hint. Called by the trigger when the player
        /// enters the interaction zone and dialogue is not active.
        /// </summary>
        public void ShowHint()
        {
            if (_isHintVisible)
            {
                return;
            }

            _isHintVisible = true;
            SetHintRendererVisible(true);
        }

        /// <summary>
        /// Hide the interaction hint. Called when the player leaves the zone
        /// or when a dialogue scene starts.
        /// </summary>
        public void HideHint()
        {
            if (!_isHintVisible)
            {
                return;
            }

            _isHintVisible = false;
            SetHintRendererVisible(false);

            // Reset position so it doesn't resume mid-float
            transform.localPosition = _initialLocalPosition;
        }

        private void LateUpdate()
        {
            if (!_isHintVisible)
            {
                return;
            }

            // Billboard — always face the camera
            if (_cachedMainCamera == null)
            {
                _cachedMainCamera = Camera.main;
                if (_cachedMainCamera == null)
                {
                    return;
                }
            }

            transform.rotation = _cachedMainCamera.transform.rotation;

            // Gentle floating animation (sin wave on local Y)
            float floatingOffset =
                Mathf.Sin(Time.time * _floatingSpeed * Mathf.PI * 2f) * _floatingAmplitude;
            transform.localPosition = _initialLocalPosition + new Vector3(0f, floatingOffset, 0f);
        }

        private void SetHintRendererVisible(bool visible)
        {
            if (_meshRenderer != null)
            {
                _meshRenderer.enabled = visible;
            }
        }
    }
}
