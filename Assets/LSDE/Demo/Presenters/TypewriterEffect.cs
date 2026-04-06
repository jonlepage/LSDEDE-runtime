using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Reveals TextMeshPro text character by character using <see cref="TMP_Text.maxVisibleCharacters"/>.
    /// This approach assigns the full text once (so TMP computes layout once) and then
    /// progressively reveals characters — zero re-layout, zero allocation per tick.
    ///
    /// Punctuation characters (period, comma, exclamation, question mark, ellipsis)
    /// use a longer delay to create a natural reading rhythm.
    ///
    /// Attach this component to the same GameObject as the <see cref="TextMeshProUGUI"/>
    /// that displays dialogue content.
    ///
    /// <b>Text ownership:</b> The <see cref="Play"/> method receives the dialogue text
    /// and assigns it to the TMP component internally. Character counting uses the
    /// raw string length — zero dependency on TMP mesh processing (textInfo,
    /// GetParsedText, ForceMeshUpdate). This ensures reliability even on the very
    /// first text display after scene load.
    /// </summary>
    public class TypewriterEffect : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Delay in seconds between each character reveal.")]
        private float _secondsPerCharacter = 0.03f;

        [SerializeField]
        [Tooltip(
            "Delay multiplier applied to punctuation characters (. , ! ? …). "
                + "For example, 5× means a period waits 5 times longer than a normal character."
        )]
        private float _punctuationDelayMultiplier = 5f;

        private TextMeshProUGUI _textComponent;
        private Coroutine _activeTypewriterCoroutine;
        private bool _isTypewriterPlaying;

        /// <summary>
        /// The full dialogue text passed to <see cref="Play"/>.
        /// Stored so the coroutine and <see cref="Skip"/> can use the original
        /// string length for character counting — completely independent of
        /// TMP's internal mesh state (textInfo, GetParsedText, etc.).
        /// </summary>
        private string _currentDialogueText;

        /// <summary>
        /// Cached WaitForSeconds for normal character delay.
        /// Avoids GC allocation every tick inside the coroutine.
        /// </summary>
        private WaitForSeconds _cachedCharacterDelay;

        /// <summary>
        /// Cached WaitForSeconds for punctuation character delay.
        /// </summary>
        private WaitForSeconds _cachedPunctuationDelay;

        /// <summary>
        /// Whether the typewriter animation is currently playing.
        /// </summary>
        public bool IsPlaying => _isTypewriterPlaying;

        private void Awake()
        {
            _textComponent = GetComponent<TextMeshProUGUI>();
            RebuildCachedDelays();
        }

        /// <summary>
        /// Start revealing the given dialogue text character by character.
        /// This method assigns the text to the TMP component, hides all characters,
        /// and starts the reveal coroutine.
        ///
        /// The <paramref name="dialogueText"/> is stored internally so that character
        /// counting uses the raw string length — no dependency on TMP mesh processing.
        /// </summary>
        /// <param name="dialogueText">The plain dialogue text to reveal (no rich text tags).</param>
        /// <param name="onComplete">Callback invoked when all characters are visible.</param>
        public void Play(string dialogueText, Action onComplete = null)
        {
            Stop();

            if (_textComponent == null)
            {
                _textComponent = GetComponent<TextMeshProUGUI>();
            }

            if (_textComponent == null || string.IsNullOrEmpty(dialogueText))
            {
                onComplete?.Invoke();
                return;
            }

            _currentDialogueText = dialogueText;
            _isTypewriterPlaying = true;

            // Assign full text so TMP computes the final layout (and BubbleSizeFitter
            // gets the correct size immediately). Then hide all characters.
            _textComponent.text = dialogueText;
            _textComponent.maxVisibleCharacters = 0;

            _activeTypewriterCoroutine = StartCoroutine(RevealCharactersCoroutine(onComplete));
        }

        /// <summary>
        /// Immediately reveal all characters, stopping the coroutine.
        /// </summary>
        public void Skip()
        {
            if (!_isTypewriterPlaying)
            {
                return;
            }

            if (_activeTypewriterCoroutine != null)
            {
                StopCoroutine(_activeTypewriterCoroutine);
                _activeTypewriterCoroutine = null;
            }

            if (_textComponent != null && _currentDialogueText != null)
            {
                _textComponent.maxVisibleCharacters = _currentDialogueText.Length;
            }

            _isTypewriterPlaying = false;
        }

        /// <summary>
        /// Stop the typewriter without revealing remaining characters.
        /// Used internally for cleanup when a new text starts.
        /// </summary>
        private void Stop()
        {
            if (_activeTypewriterCoroutine != null)
            {
                StopCoroutine(_activeTypewriterCoroutine);
                _activeTypewriterCoroutine = null;
            }

            _isTypewriterPlaying = false;
            _currentDialogueText = null;
        }

        /// <summary>
        /// Rebuild cached WaitForSeconds instances when timing values change.
        /// </summary>
        private void RebuildCachedDelays()
        {
            _cachedCharacterDelay = new WaitForSeconds(_secondsPerCharacter);
            _cachedPunctuationDelay = new WaitForSeconds(
                _secondsPerCharacter * _punctuationDelayMultiplier
            );
        }

        /// <summary>
        /// Coroutine that reveals characters one at a time with appropriate delays.
        /// Punctuation characters get a longer pause for natural reading rhythm.
        ///
        /// Uses <see cref="_currentDialogueText"/> (the raw string) for character counting
        /// and punctuation detection — zero dependency on TMP internals (textInfo,
        /// GetParsedText, ForceMeshUpdate). This is reliable on every frame, including
        /// the very first text display after scene load.
        /// </summary>
        private IEnumerator RevealCharactersCoroutine(Action onComplete)
        {
            int totalCharacterCount = _currentDialogueText.Length;

            for (int characterIndex = 0; characterIndex < totalCharacterCount; characterIndex++)
            {
                _textComponent.maxVisibleCharacters = characterIndex + 1;

                char revealedCharacter = _currentDialogueText[characterIndex];

                if (IsPunctuationCharacter(revealedCharacter))
                {
                    yield return _cachedPunctuationDelay;
                }
                else
                {
                    yield return _cachedCharacterDelay;
                }
            }

            _isTypewriterPlaying = false;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Returns true if the character should use the longer punctuation delay.
        /// Includes period, comma, exclamation, question mark, and ellipsis.
        /// </summary>
        private static bool IsPunctuationCharacter(char character)
        {
            return character == '.'
                || character == ','
                || character == '!'
                || character == '?'
                || character == '\u2026'; // ellipsis (…)
        }
    }
}
