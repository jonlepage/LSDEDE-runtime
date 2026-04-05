using TMPro;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Controls a single speech bubble's visibility and text content.
    /// Attached to a world-space Canvas that is a child of a character's bubble anchor.
    /// The bubble starts hidden and is shown/hidden by the <see cref="BubbleDialoguePresenter"/>.
    ///
    /// Structure expected:
    /// <code>
    /// SpeechBubble (this script + BillboardRotation)
    /// └── Canvas (World Space)
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

        /// <summary>
        /// Whether the speech bubble is currently visible.
        /// </summary>
        public bool IsVisible => gameObject.activeSelf;

        /// <summary>
        /// Show the speech bubble with the given character name and dialogue text.
        /// Activates the GameObject and updates the TextMeshPro fields.
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

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hide the speech bubble. Deactivates the entire GameObject.
        /// </summary>
        public void HideDialogue()
        {
            gameObject.SetActive(false);
        }
    }
}
