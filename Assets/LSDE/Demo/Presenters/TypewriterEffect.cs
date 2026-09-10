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
        /// What to call once every character is on screen — however that happened.
        ///
        /// <para>Held in a field rather than passed down to the coroutine, because the reveal has
        /// TWO ends: the coroutine running out, and <see cref="Skip"/> cutting it short. Only the
        /// first used to notify anyone, and <see cref="Skip"/> kills the coroutine before its tail
        /// ever runs — so a line the player hurried never reported itself finished.</para>
        ///
        /// <para>That silence is what hangs a scene. A block carrying <c>timeout</c> arms its
        /// countdown from this callback, precisely because the countdown starts when the line has
        /// been SAID; no callback, no countdown, and the block stays on screen for good. A block on
        /// <c>waitInput</c> survived it only because its advance was registered up front. The
        /// reference demo cannot have this bug: its typewriter exposes <c>isComplete</c> as STATE
        /// and skipping sets it, so both ends meet at the same flag.</para>
        ///
        /// <para>Cleared as it fires, so a reveal reports itself finished exactly once.</para>
        /// </summary>
        private Action _onRevealComplete;

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
            _onRevealComplete = onComplete;

            // Assign full text so TMP computes the final layout (and BubbleSizeFitter
            // gets the correct size immediately). Then hide all characters.
            _textComponent.text = dialogueText;
            _textComponent.maxVisibleCharacters = 0;

            _activeTypewriterCoroutine = StartCoroutine(RevealCharactersCoroutine());
        }

        /// <summary>
        /// Immediately reveal all characters, stopping the coroutine.
        ///
        /// <para>The line still counts as SAID: a hurried reveal reports itself finished exactly
        /// like one that ran its course, which is what lets a <c>timeout</c> start counting. See
        /// <see cref="_onRevealComplete"/>.</para>
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

            CompleteReveal();
        }

        /// <summary>
        /// Mark the reveal finished and notify whoever was waiting, once.
        ///
        /// <para>The single exit both ends of the reveal go through — the coroutine running out,
        /// and <see cref="Skip"/>. The callback is taken out of the field BEFORE being invoked, so
        /// a handler that starts another line from inside it cannot see a stale one.</para>
        /// </summary>
        private void CompleteReveal()
        {
            _isTypewriterPlaying = false;

            var callback = _onRevealComplete;
            _onRevealComplete = null;
            callback?.Invoke();
        }

        /// <summary>
        /// Stop the typewriter without revealing remaining characters.
        /// Used internally for cleanup when a new text starts.
        ///
        /// <para>Drops the pending callback instead of firing it, which is the difference with
        /// <see cref="Skip"/>: this line is being REPLACED, not finished. Reporting it as said
        /// would arm the timeout of a block that is no longer on screen.</para>
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
            _onRevealComplete = null;
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
        private IEnumerator RevealCharactersCoroutine()
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

            _activeTypewriterCoroutine = null;
            CompleteReveal();
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
