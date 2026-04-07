using System.Collections;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Plays a squash-and-stretch bounce animation on a character when triggered.
    /// Port of the TS <c>bounceUpdate</c> from <c>animations.ts</c>:
    /// a quick elastic squash that decays over the animation duration,
    /// giving the character a lively "speaking" feel.
    ///
    /// Call <see cref="PlayBounce"/> to trigger the animation. If an animation
    /// is already playing, subsequent calls are ignored (no stacking).
    ///
    /// Setup in Unity Editor:
    /// 1. Attach this component to any character GameObject that has a visible sprite
    /// 2. The animation modifies <c>transform.localScale</c> and restores it when done
    /// </summary>
    public class CharacterBounceAnimation : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Duration of the bounce animation in seconds.")]
        private float _bounceDuration = 0.5f;

        [SerializeField]
        [Tooltip(
            "Intensity of the squash effect. Higher values = more exaggerated bounce. "
                + "0.2 = subtle, 0.4 = playful."
        )]
        private float _bounceIntensity = 0.2f;

        [SerializeField]
        [Tooltip("Number of oscillation cycles during the bounce.")]
        private int _bounceCycles = 3;

        /// <summary>Whether a bounce animation is currently playing.</summary>
        private bool _isPlaying;

        /// <summary>The original local scale, captured before animation starts.</summary>
        private Vector3 _originalScale;

        /// <summary>
        /// Play the bounce animation. If already playing, the call is ignored.
        /// The animation modifies <c>localScale</c> with a decaying squash-and-stretch
        /// effect, then restores the original scale when complete.
        /// </summary>
        public void PlayBounce()
        {
            if (_isPlaying)
            {
                return;
            }

            StartCoroutine(BounceCoroutine());
        }

        /// <summary>
        /// Coroutine that drives the bounce animation over <see cref="_bounceDuration"/> seconds.
        /// Uses the same math as the TS reference:
        /// <c>squash = 1 + sin(progress * PI * cycles) * intensity * (1 - progress)</c>
        /// The <c>(1 - progress)</c> term makes the bounce decay to zero by the end.
        /// </summary>
        private IEnumerator BounceCoroutine()
        {
            _isPlaying = true;
            _originalScale = transform.localScale;

            float elapsedTime = 0f;

            while (elapsedTime < _bounceDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / _bounceDuration);

                // Decaying sine wave — same formula as TS bounceUpdate
                float squash =
                    1f
                    + Mathf.Sin(progress * Mathf.PI * _bounceCycles)
                        * _bounceIntensity
                        * (1f - progress);

                transform.localScale = new Vector3(
                    _originalScale.x / squash,
                    _originalScale.y * squash,
                    _originalScale.z
                );

                yield return null;
            }

            // Restore original scale
            transform.localScale = _originalScale;
            _isPlaying = false;
        }
    }
}
