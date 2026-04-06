using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LSDE.Demo
{
    /// <summary>
    /// Interactive choice button displayed inside a speech bubble during CHOICE blocks.
    /// Handles pointer hover (color change) and click detection using Unity's EventSystem.
    ///
    /// Created dynamically by <see cref="SpeechBubbleController.ShowChoices"/> —
    /// one instance per visible choice option. Destroyed when choices are dismissed.
    ///
    /// Visual behavior matches the LSDE TypeScript demo:
    /// - Normal state: dark text
    /// - Hover state: blue accent color (#4a9eff)
    /// - Click: invokes the selection callback with this choice's UUID
    ///
    /// Requires a <see cref="TextMeshProUGUI"/> on the same GameObject with
    /// <c>raycastTarget = true</c> so the EventSystem can detect pointer events.
    /// </summary>
    public class ChoiceButtonItem
        : MonoBehaviour,
            IPointerEnterHandler,
            IPointerExitHandler,
            IPointerClickHandler
    {
        private static readonly Color NormalTextColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        private static readonly Color HoverTextColor = new Color(0.29f, 0.62f, 1f, 1f);

        private TextMeshProUGUI _choiceText;
        private Action<string> _onChoiceSelected;
        private string _choiceUuid;

        /// <summary>
        /// Set up this choice button with its UUID, display text, and selection callback.
        /// Must be called immediately after the component is added to a GameObject.
        /// </summary>
        /// <param name="choiceUuid">The UUID of this choice, passed back to the engine on selection.</param>
        /// <param name="displayText">The localized choice text to display (without prefix).</param>
        /// <param name="onChoiceSelected">Callback invoked with the choice UUID when the player clicks.</param>
        public void Initialize(
            string choiceUuid,
            string displayText,
            Action<string> onChoiceSelected
        )
        {
            _choiceUuid = choiceUuid;
            _onChoiceSelected = onChoiceSelected;

            _choiceText = GetComponent<TextMeshProUGUI>();
            _choiceText.text = $"\u25b8 {displayText}";
            _choiceText.color = NormalTextColor;
        }

        /// <summary>
        /// Highlight the choice text when the pointer enters.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_choiceText != null)
            {
                _choiceText.color = HoverTextColor;
            }
        }

        /// <summary>
        /// Restore normal text color when the pointer exits.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (_choiceText != null)
            {
                _choiceText.color = NormalTextColor;
            }
        }

        /// <summary>
        /// Invoke the selection callback when the player clicks this choice.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            _onChoiceSelected?.Invoke(_choiceUuid);
        }
    }
}
