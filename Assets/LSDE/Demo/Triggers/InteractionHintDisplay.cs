using TMPro;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Displays an interaction hint (e.g. a speech emoji) above a character
    /// to indicate that the player can interact with them.
    /// The hint automatically faces the camera (billboard) each frame.
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

        private void Awake()
        {
            _initialLocalPosition = transform.localPosition;

            if (_hintText != null)
            {
                _hintText.text = _displayText;
            }

            // Start hidden — the trigger controls visibility
            gameObject.SetActive(false);
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
            gameObject.SetActive(true);
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
            gameObject.SetActive(false);

            // Reset position so it doesn't resume mid-float
            transform.localPosition = _initialLocalPosition;
        }

        private void LateUpdate()
        {
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
    }
}
