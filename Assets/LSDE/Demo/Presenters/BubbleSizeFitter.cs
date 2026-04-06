using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Synchronizes a world-space Canvas RectTransform size with the preferred size
    /// of a child layout panel (driven by <c>ContentSizeFitter</c> + <c>VerticalLayoutGroup</c>).
    ///
    /// In Unity, <c>ContentSizeFitter</c> cannot directly drive a world-space Canvas.
    /// This component bridges the gap: it reads the TextPanel's preferred size each frame
    /// and applies it to the Canvas RectTransform, so the <see cref="ProceduralSpeechBubble"/>
    /// background always wraps tightly around the text content.
    ///
    /// The padding in the <c>VerticalLayoutGroup</c> creates the margin between
    /// the text and the bubble outline. Increasing the padding → bigger bubble, more margin.
    /// </summary>
    [ExecuteAlways]
    public class BubbleSizeFitter : MonoBehaviour
    {
        [SerializeField]
        [Tooltip(
            "The text panel whose preferred size drives the Canvas size. "
                + "Must have a ContentSizeFitter + VerticalLayoutGroup."
        )]
        private RectTransform _textPanelRectTransform;

        [SerializeField]
        [Tooltip("Minimum width of the bubble in pixels.")]
        private float _minimumBubbleWidth = 200f;

        [SerializeField]
        [Tooltip("Minimum height of the bubble in pixels.")]
        private float _minimumBubbleHeight = 150f;

        private RectTransform _canvasRectTransform;

        private void Awake()
        {
            _canvasRectTransform = GetComponent<RectTransform>();
        }

        private void LateUpdate()
        {
            if (_textPanelRectTransform == null || _canvasRectTransform == null)
            {
                return;
            }

            // Read the preferred size computed by ContentSizeFitter + VerticalLayoutGroup
            float preferredWidth = UnityEngine.UI.LayoutUtility.GetPreferredWidth(
                _textPanelRectTransform
            );
            float preferredHeight = UnityEngine.UI.LayoutUtility.GetPreferredHeight(
                _textPanelRectTransform
            );

            float finalWidth = Mathf.Max(preferredWidth, _minimumBubbleWidth);
            float finalHeight = Mathf.Max(preferredHeight, _minimumBubbleHeight);

            _canvasRectTransform.sizeDelta = new Vector2(finalWidth, finalHeight);
        }
    }
}
