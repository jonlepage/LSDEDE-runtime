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
        /// Start revealing the text that is already assigned to the TextMeshProUGUI component.
        /// The text must be set on the TMP component BEFORE calling this method.
        /// When the animation finishes (or is skipped), <paramref name="onComplete"/> is invoked.
        /// </summary>
        /// <param name="onComplete">Callback invoked when all characters are visible.</param>
        public void Play(Action onComplete)
        {
            Stop();

            if (_textComponent == null)
            {
                onComplete?.Invoke();
                return;
            }

            _isTypewriterPlaying = true;
            _textComponent.maxVisibleCharacters = 0;

            // Force TMP to process the mesh so textInfo.characterCount is accurate
            _textComponent.ForceMeshUpdate();

            _activeTypewriterCoroutine = StartCoroutine(RevealCharactersCoroutine(onComplete));
        }

        /// <summary>
        /// Immediately reveal all characters, stopping the coroutine.
        /// The onComplete callback from <see cref="Play"/> will still be invoked.
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

            if (_textComponent != null)
            {
                _textComponent.maxVisibleCharacters = _textComponent.textInfo.characterCount;
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
        /// </summary>
        private IEnumerator RevealCharactersCoroutine(Action onComplete)
        {
            int totalCharacterCount = _textComponent.textInfo.characterCount;

            for (int characterIndex = 0; characterIndex < totalCharacterCount; characterIndex++)
            {
                _textComponent.maxVisibleCharacters = characterIndex + 1;

                // Determine if this character is punctuation for a longer pause
                char revealedCharacter = _textComponent
                    .textInfo
                    .characterInfo[characterIndex]
                    .character;

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
