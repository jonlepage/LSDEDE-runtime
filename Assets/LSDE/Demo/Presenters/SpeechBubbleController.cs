using TMPro;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Controls a single speech bubble's visibility and text content.
    /// Attached to a world-space Canvas that is a child of a character's bubble anchor.
    /// The bubble starts hidden and is shown/hidden by the <see cref="BubbleDialoguePresenter"/>.
    ///
    /// Visibility uses <see cref="CanvasGroup.alpha"/> instead of <c>SetActive</c>
    /// to avoid the TextMeshPro first-activation lag spike. The GameObject stays
    /// active at all times so TMP initializes during scene load.
    ///
    /// Structure expected:
    /// <code>
    /// SpeechBubble (this script + BillboardRotation)
    /// └── Canvas (World Space, with CanvasGroup)
    ///     └── BubblePanel (Image background)
    ///         ├── CharacterNameText (TextMeshProUGUI)
    ///         └── DialogueContentText (TextMeshProUGUI)
    /// </code>
    /// </summary>
    public class SpeechBubbleController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("TextMeshPro component displaying the character's name (bold, at the top).")]
        private TextMeshProUGUI _characterNameText;

        [SerializeField]
        [Tooltip("TextMeshPro component displaying the dialogue text (body).")]
        private TextMeshProUGUI _dialogueContentText;

        private CanvasGroup _canvasGroup;
        private bool _isBubbleVisible;

        /// <summary>
        /// Whether the speech bubble is currently visible.
        /// </summary>
        public bool IsVisible => _isBubbleVisible;

        /// <summary>
        /// Initialize the CanvasGroup and hide the bubble.
        /// Called either by Unity (if the GameObject starts active) or manually
        /// by <see cref="BubbleDialoguePresenter"/> during warm-up (if it starts inactive
        /// in the Editor to avoid visual clutter).
        /// </summary>
        public void WarmUp()
        {
            if (_canvasGroup != null)
            {
                // Already warmed up
                return;
            }

            gameObject.SetActive(true);

            // Find or add a CanvasGroup on the Canvas child.
            // CanvasGroup.alpha controls visibility without triggering
            // the expensive TMP OnEnable/OnDisable cycle.
            var canvas = GetComponentInChildren<Canvas>(true);
            if (canvas != null)
            {
                _canvasGroup = canvas.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
                }
            }

            // Start hidden — alpha 0, non-interactable, non-blocking
            SetBubbleVisible(false);
        }

        private void Awake()
        {
            WarmUp();
        }

        /// <summary>
        /// Show the speech bubble with the given character name and dialogue text.
        /// Uses CanvasGroup.alpha to make the bubble visible without SetActive.
        /// </summary>
        /// <param name="characterName">The name of the speaking character.</param>
        /// <param name="dialogueText">The localized dialogue text to display.</param>
        public void ShowDialogue(string characterName, string dialogueText)
        {
            if (_characterNameText != null)
            {
                _characterNameText.text = characterName;
            }

            if (_dialogueContentText != null)
            {
                _dialogueContentText.text = dialogueText;
            }

            SetBubbleVisible(true);
        }

        /// <summary>
        /// Hide the speech bubble. Uses CanvasGroup.alpha = 0 to hide
        /// without deactivating the GameObject.
        /// </summary>
        public void HideDialogue()
        {
            SetBubbleVisible(false);
        }

        private void SetBubbleVisible(bool visible)
        {
            _isBubbleVisible = visible;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = visible ? 1f : 0f;
                _canvasGroup.interactable = visible;
                _canvasGroup.blocksRaycasts = visible;
            }
        }
    }
}
