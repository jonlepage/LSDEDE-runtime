using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LSDE.Demo
{
    /// <summary>
    /// One answer inside a speech bubble during a CHOICE block: a full-width row the player can
    /// point at, and the click that selects it.
    ///
    /// <para>Created by <see cref="SpeechBubbleController.ShowChoices"/>, one per offered option,
    /// destroyed when the choices are dismissed.</para>
    ///
    /// <para><b>The row is the target, not the glyphs.</b> The pointer is hit-tested against the
    /// background <see cref="Image"/> that spans the whole row, so anywhere on the strip works —
    /// including the empty space to the right of a short answer. Hit-testing the text instead is
    /// what made these nearly unclickable: text is tested against its rectangle, a wrapped answer
    /// drew outside that rectangle, and the player kept aiming at letters that were not a
    /// target.</para>
    ///
    /// <para>Hover tints the row AND the text. The text colour alone was too quiet to answer
    /// "which one am I about to pick?" — a band of colour under the whole answer is unambiguous,
    /// which matters when three of them are stacked in a small bubble.</para>
    /// </summary>
    public class ChoiceButtonItem
        : MonoBehaviour,
            IPointerEnterHandler,
            IPointerExitHandler,
            IPointerClickHandler
    {
        private static readonly Color NormalTextColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        private static readonly Color HoverTextColor = new Color(0.09f, 0.36f, 0.72f, 1f);

        /// <summary>
        /// The row at rest. Very nearly transparent rather than fully so: a faint plate separates
        /// one answer from the next, which is what stopped three stacked lines from reading as one
        /// paragraph.
        /// </summary>
        public static readonly Color NormalBackgroundColor = new Color(0f, 0f, 0f, 0.05f);

        /// <summary>The row under the pointer. The accent of the reference demo, kept light so the
        /// dark answer text stays readable on it.</summary>
        private static readonly Color HoverBackgroundColor = new Color(0.29f, 0.62f, 1f, 0.28f);

        private TextMeshProUGUI _choiceText;
        private Image _rowBackground;
        private Action<string> _onChoiceSelected;
        private string _choiceUuid;

        /// <summary>
        /// Set up this answer with its option id, display text, and selection callback.
        /// Must be called immediately after the component is added to a GameObject.
        /// </summary>
        /// <param name="choiceUuid">
        /// The option id, passed back to the engine on selection. In v2 this IS the exit port
        /// the flow leaves by.
        /// </param>
        /// <param name="displayText">The localized answer text, without the marker prefix.</param>
        /// <param name="onChoiceSelected">Callback invoked with the option id when the player clicks.</param>
        /// <param name="choiceText">The label to write into and tint.</param>
        /// <param name="rowBackground">The full-row plate that receives the pointer and the highlight.</param>
        public void Initialize(
            string choiceUuid,
            string displayText,
            Action<string> onChoiceSelected,
            TextMeshProUGUI choiceText,
            Image rowBackground
        )
        {
            _choiceUuid = choiceUuid;
            _onChoiceSelected = onChoiceSelected;
            _choiceText = choiceText;
            _rowBackground = rowBackground;

            if (_choiceText != null)
            {
                _choiceText.text = $"> {displayText}";
                _choiceText.color = NormalTextColor;
            }

            if (_rowBackground != null)
            {
                _rowBackground.color = NormalBackgroundColor;
            }
        }

        /// <summary>
        /// Light the row up when the pointer enters it.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            SetHighlighted(true);
        }

        /// <summary>
        /// Put the row back to rest when the pointer leaves.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            SetHighlighted(false);
        }

        /// <summary>
        /// Select this answer.
        ///
        /// <para>Nothing here stops the click reaching the world underneath — that is
        /// <see cref="PlayerClickToMoveInput"/>'s job, and it refuses any click that lands on UI.
        /// A pointer handler cannot do it: the movement code reads the mouse device directly, so
        /// it never passes through the event system at all.</para>
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            _onChoiceSelected?.Invoke(_choiceUuid);
        }

        private void SetHighlighted(bool isHighlighted)
        {
            if (_choiceText != null)
            {
                _choiceText.color = isHighlighted ? HoverTextColor : NormalTextColor;
            }

            if (_rowBackground != null)
            {
                _rowBackground.color = isHighlighted ? HoverBackgroundColor : NormalBackgroundColor;
            }
        }
    }
}
