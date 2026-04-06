using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    /// When <see cref="ShowDialogue"/> is called, the bubble fades in (alpha 0→1)
    /// while the <see cref="TypewriterEffect"/> reveals text character by character
    /// in parallel.
    ///
    /// Structure expected:
    /// <code>
    /// SpeechBubble (this script + BillboardRotation)
    /// └── Canvas (World Space, with CanvasGroup)
    ///     ├── BubbleBackground (ProceduralSpeechBubble)
    ///     └── TextPanel
    ///         ├── CharacterNameText (TextMeshProUGUI)
    ///         └── DialogueContentText (TextMeshProUGUI + TypewriterEffect)
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

        [SerializeField]
        [Tooltip("Duration in seconds for the bubble fade-in animation (alpha 0 → 1).")]
        private float _fadeInDurationSeconds = 0.25f;

        private CanvasGroup _canvasGroup;
        private bool _isBubbleVisible;
        private Coroutine _activeFadeCoroutine;
        private TypewriterEffect _typewriterEffect;

        /// <summary>
        /// Dynamically created choice button GameObjects, tracked for cleanup.
        /// Populated by <see cref="ShowChoices"/> and destroyed by <see cref="HideChoices"/>.
        /// </summary>
        private readonly List<GameObject> _dynamicChoiceButtons = new List<GameObject>();

        /// <summary>
        /// Whether the bubble is currently displaying choice buttons instead of dialogue text.
        /// </summary>
        private bool _isShowingChoices;

        /// <summary>
        /// Whether the speech bubble is currently visible.
        /// </summary>
        public bool IsVisible => _isBubbleVisible;

        /// <summary>
        /// Whether the typewriter animation is currently playing on this bubble.
        /// Used by <see cref="DialogueClickAdvancer"/> to decide whether a click
        /// should skip the typewriter or advance to the next block.
        /// </summary>
        public bool IsTypewriterPlaying => _typewriterEffect != null && _typewriterEffect.IsPlaying;

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

            // Resolve the TypewriterEffect on the dialogue content text
            if (_dialogueContentText != null)
            {
                _typewriterEffect = _dialogueContentText.GetComponent<TypewriterEffect>();
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
        /// Launches a fade-in animation (alpha 0→1) and the typewriter effect in parallel.
        /// When the typewriter finishes (or is skipped), <paramref name="onTypewriterComplete"/>
        /// is invoked so the presenter can enable click-to-advance.
        /// </summary>
        /// <param name="characterName">The name of the speaking character.</param>
        /// <param name="dialogueText">The localized dialogue text to display.</param>
        /// <param name="onTypewriterComplete">
        /// Callback invoked when the typewriter finishes revealing all characters
        /// (either naturally or via skip). Can be null if no callback is needed.
        /// </param>
        public void ShowDialogue(
            string characterName,
            string dialogueText,
            Action onTypewriterComplete = null
        )
        {
            if (_characterNameText != null)
            {
                _characterNameText.text = characterName;
            }

            // Start fade-in animation (alpha 0 → 1 with ease-out)
            _isBubbleVisible = true;
            StartFadeIn();

            // Lazy-resolve TypewriterEffect if WarmUp didn't catch it.
            // This handles initialization order edge cases where WarmUp ran
            // before TypewriterEffect.Awake() completed, or where SetActive
            // triggered a reentrant WarmUp call during the first activation.
            if (_typewriterEffect == null && _dialogueContentText != null)
            {
                _typewriterEffect = _dialogueContentText.GetComponent<TypewriterEffect>();
            }

            // Start typewriter effect in parallel with the fade-in.
            // TypewriterEffect.Play() handles text assignment internally —
            // it sets the full text on the TMP component and then hides characters.
            // If no typewriter is available, assign text directly as fallback.
            if (_typewriterEffect != null)
            {
                _typewriterEffect.Play(dialogueText, onTypewriterComplete);
            }
            else
            {
                // No typewriter component — assign text directly and reveal immediately
                if (_dialogueContentText != null)
                {
                    _dialogueContentText.text = dialogueText;
                }

                onTypewriterComplete?.Invoke();
            }
        }

        /// <summary>
        /// Skip the typewriter animation, immediately revealing all remaining text.
        /// Called by the click advancer when the player clicks during the typewriter.
        /// </summary>
        public void SkipTypewriter()
        {
            if (_typewriterEffect != null)
            {
                _typewriterEffect.Skip();
            }
        }

        /// <summary>
        /// Show choice buttons inside the bubble instead of dialogue text.
        /// Hides the character name and dialogue content, then creates interactive
        /// <see cref="ChoiceButtonItem"/> elements for each visible choice.
        ///
        /// Each button displays the localized choice text with a triangle marker (▸)
        /// and changes color on hover. Clicking a button invokes the selection callback.
        ///
        /// The bubble uses the same fade-in animation as dialogue display.
        /// </summary>
        /// <param name="characterName">The name of the character presenting the choices.</param>
        /// <param name="choices">List of (uuid, localizedText) tuples for each visible choice.</param>
        /// <param name="onChoiceSelected">Callback invoked with the chosen UUID when the player clicks.</param>
        public void ShowChoices(
            string characterName,
            IReadOnlyList<(string uuid, string localizedText)> choices,
            Action<string> onChoiceSelected
        )
        {
            // Clean up any previous choices or dialogue
            HideChoices();

            if (_characterNameText != null)
            {
                _characterNameText.text = characterName;
            }

            // Hide the dialogue content text — choices replace it
            if (_dialogueContentText != null)
            {
                _dialogueContentText.gameObject.SetActive(false);
            }

            // Create a choice button for each visible option.
            // Buttons are added as children of the TextPanel (which has a VerticalLayoutGroup),
            // so they stack below the character name automatically.
            var textPanel =
                _dialogueContentText != null ? _dialogueContentText.transform.parent : null;

            if (textPanel != null)
            {
                foreach (var (choiceUuid, choiceLocalizedText) in choices)
                {
                    var buttonGameObject = new GameObject($"Choice_{choiceUuid.Substring(0, 8)}");
                    buttonGameObject.transform.SetParent(textPanel, false);

                    // TextMeshProUGUI acts as both the visual and the raycast target
                    var buttonText = buttonGameObject.AddComponent<TextMeshProUGUI>();
                    buttonText.fontSize = 20f;
                    buttonText.alignment = TextAlignmentOptions.TopLeft;
                    buttonText.textWrappingMode = TextWrappingModes.Normal;
                    buttonText.overflowMode = TextOverflowModes.Overflow;
                    buttonText.raycastTarget = true;

                    // LayoutElement ensures consistent sizing in the vertical layout
                    var layoutElement = buttonGameObject.AddComponent<LayoutElement>();
                    layoutElement.preferredHeight = 32f;
                    layoutElement.flexibleWidth = 1f;

                    // ChoiceButtonItem handles hover color change and click detection
                    var choiceButton = buttonGameObject.AddComponent<ChoiceButtonItem>();
                    choiceButton.Initialize(choiceUuid, choiceLocalizedText, onChoiceSelected);

                    _dynamicChoiceButtons.Add(buttonGameObject);
                }
            }

            _isShowingChoices = true;
            _isBubbleVisible = true;
            StartFadeIn();
        }

        /// <summary>
        /// Remove all dynamically created choice buttons and restore the dialogue text area.
        /// Called when a choice is selected, when the bubble is hidden, or before showing new choices.
        /// </summary>
        public void HideChoices()
        {
            foreach (var buttonGameObject in _dynamicChoiceButtons)
            {
                if (buttonGameObject != null)
                {
                    Destroy(buttonGameObject);
                }
            }
            _dynamicChoiceButtons.Clear();

            // Restore dialogue text visibility for future dialogue blocks
            if (_dialogueContentText != null && _isShowingChoices)
            {
                _dialogueContentText.gameObject.SetActive(true);
            }

            _isShowingChoices = false;
        }

        /// <summary>
        /// Hide the speech bubble. Uses CanvasGroup.alpha = 0 to hide
        /// without deactivating the GameObject.
        /// </summary>
        public void HideDialogue()
        {
            // Stop the typewriter coroutine if it's still running.
            // Without this, hiding a bubble during typewriter playback
            // leaves a coroutine ticking on an invisible object.
            if (_typewriterEffect != null && _typewriterEffect.IsPlaying)
            {
                _typewriterEffect.Skip();
            }

            // Clean up any choice buttons before hiding
            HideChoices();

            StopFade();
            SetBubbleVisible(false);
        }

        /// <summary>
        /// Start the fade-in coroutine. Stops any running fade first.
        /// Uses an ease-out curve (fast start, slow finish) for organic feel.
        /// </summary>
        private void StartFadeIn()
        {
            StopFade();

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                _activeFadeCoroutine = StartCoroutine(FadeInCoroutine());
            }
        }

        /// <summary>
        /// Stop any running fade coroutine.
        /// </summary>
        private void StopFade()
        {
            if (_activeFadeCoroutine != null)
            {
                StopCoroutine(_activeFadeCoroutine);
                _activeFadeCoroutine = null;
            }
        }

        /// <summary>
        /// Coroutine that interpolates CanvasGroup.alpha from 0 to 1
        /// over <see cref="_fadeInDurationSeconds"/> using an ease-out curve.
        /// Ease-out: fast at the start, decelerates toward the end.
        /// </summary>
        private IEnumerator FadeInCoroutine()
        {
            float elapsedTime = 0f;

            while (elapsedTime < _fadeInDurationSeconds)
            {
                elapsedTime += Time.deltaTime;
                float linearProgress = Mathf.Clamp01(elapsedTime / _fadeInDurationSeconds);

                // Ease-out: 1 - (1 - t)^2 — fast start, smooth deceleration
                float easedProgress = 1f - (1f - linearProgress) * (1f - linearProgress);

                _canvasGroup.alpha = easedProgress;
                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _activeFadeCoroutine = null;
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
