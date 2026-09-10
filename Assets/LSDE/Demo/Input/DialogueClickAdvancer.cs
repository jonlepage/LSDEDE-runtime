using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LSDE.Demo
{
    /// <summary>
    /// Listens for mouse clicks and manages dialogue flow advancement with typewriter support.
    /// Supports multiple simultaneous dialogue blocks (multi-track parallel dialogue).
    ///
    /// <para>Click behaviour, in two phases:</para>
    /// <list type="bullet">
    ///   <item>If any typewriter is playing → the click skips ALL typewriters (reveals the text)
    ///     and advances nothing.</item>
    ///   <item>If every text is fully revealed → the click advances every block that is WAITING
    ///     FOR INPUT (broadcast).</item>
    /// </list>
    ///
    /// <para><b>Two kinds of registration.</b> A block that waits for the player is registered
    /// with <see cref="SetPendingAdvance"/>. A block that plays its own time —
    /// one carrying <c>timeout</c> — is registered with <see cref="SetRevealOnly"/> instead: it
    /// takes part in phase 1, so a click still HURRIES its reveal, but it is never advanced by a
    /// click. That is what "timeout outranks waitInput" means in practice: the writer said how
    /// long the line stays, so a click may only get to the end of the sentence faster.</para>
    ///
    /// <para>Entries are keyed by PRESENTATION, not by block id: with
    /// <c>inPortPerCharacter</c> one block can be on screen several times at once, each for a
    /// different actor. See <c>LsdePresentationKey</c>.</para>
    ///
    /// Clicking does NOT block other interactions (e.g. player movement).
    /// </summary>
    public class DialogueClickAdvancer : MonoBehaviour
    {
        /// <summary>
        /// One presentation waiting on the player, or merely skippable.
        /// </summary>
        private struct PendingEntry
        {
            /// <summary>The engine's Next callback, or null for a reveal-only entry.</summary>
            public Action AdvanceCallback;

            /// <summary>The bubble, used to check and skip its typewriter.</summary>
            public SpeechBubbleController BubbleController;
        }

        /// <summary>
        /// All presentations a click can act on, keyed by presentation key.
        /// </summary>
        private readonly Dictionary<string, PendingEntry> _pendingEntriesByKey =
            new Dictionary<string, PendingEntry>();

        /// <summary>
        /// The frame number on which an entry was last armed.
        /// Clicks on this exact frame are ignored to prevent the "phantom click" problem:
        /// the player's click to START the dialogue (via NPC interaction) would otherwise
        /// also be detected as a click to skip the typewriter, because both the dialogue
        /// trigger and this advancer process the same <c>wasPressedThisFrame</c> input
        /// within a single Unity frame.
        /// </summary>
        private int _armedOnFrame = -1;

        /// <summary>
        /// Whether any presentation is waiting for a click to advance.
        /// Reveal-only entries do not count: nothing is waiting on the player there.
        /// </summary>
        public bool HasPendingAdvance
        {
            get
            {
                foreach (var entry in _pendingEntriesByKey.Values)
                {
                    if (entry.AdvanceCallback != null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Register a presentation that WAITS for the player: a click reveals its text, and the
        /// next click advances the flow.
        /// </summary>
        /// <param name="presentationKey">Identifies this presentation of the block.</param>
        /// <param name="advanceCallback">The engine's Next callback.</param>
        /// <param name="activeBubbleController">
        /// The visible bubble, used to check typewriter state and skip it on the first click.
        /// May be null when no typewriter support is needed.
        /// </param>
        public void SetPendingAdvance(
            string presentationKey,
            Action advanceCallback,
            SpeechBubbleController activeBubbleController = null
        )
        {
            _pendingEntriesByKey[presentationKey] = new PendingEntry
            {
                AdvanceCallback = advanceCallback,
                BubbleController = activeBubbleController,
            };
            _armedOnFrame = Time.frameCount;
        }

        /// <summary>
        /// Register a presentation a click may only HURRY, never dismiss — a block whose
        /// <c>timeout</c> decides when it leaves.
        /// </summary>
        /// <param name="presentationKey">Identifies this presentation of the block.</param>
        /// <param name="activeBubbleController">The visible bubble, whose typewriter can be skipped.</param>
        public void SetRevealOnly(
            string presentationKey,
            SpeechBubbleController activeBubbleController
        )
        {
            _pendingEntriesByKey[presentationKey] = new PendingEntry
            {
                AdvanceCallback = null,
                BubbleController = activeBubbleController,
            };
            _armedOnFrame = Time.frameCount;
        }

        /// <summary>
        /// Forget one presentation. Called during block cleanup, or when a timeout advanced it.
        /// </summary>
        /// <param name="presentationKey">The presentation to clear.</param>
        public void ClearPendingAdvanceForBlock(string presentationKey)
        {
            _pendingEntriesByKey.Remove(presentationKey);
        }

        /// <summary>
        /// Forget every presentation.
        /// Called during scene exit or when switching to a CHOICE block
        /// (choices use their own selection buttons, not click-anywhere).
        /// </summary>
        public void ClearAllPendingAdvances()
        {
            _pendingEntriesByKey.Clear();
        }

        /// <summary>
        /// Unity calls Update every frame. We check for a left-button press
        /// using the new Input System (UnityEngine.InputSystem).
        /// </summary>
        private void Update()
        {
            if (_pendingEntriesByKey.Count == 0)
            {
                return;
            }

            var currentMouse = Mouse.current;
            if (currentMouse == null)
            {
                return;
            }

            if (!currentMouse.leftButton.wasPressedThisFrame)
            {
                return;
            }

            // Ignore clicks on the same frame an entry was armed — the click that STARTED the
            // dialogue must not also skip the first typewriter.
            if (Time.frameCount == _armedOnFrame)
            {
                return;
            }

            // Phase 1: if ANY typewriter is still playing, skip them all and advance nothing.
            // Reveal-only entries take part here: hurrying the reveal is exactly what they allow.
            bool anyTypewriterIsPlaying = false;
            foreach (var entry in _pendingEntriesByKey.Values)
            {
                if (entry.BubbleController != null && entry.BubbleController.IsTypewriterPlaying)
                {
                    anyTypewriterIsPlaying = true;
                    break;
                }
            }

            if (anyTypewriterIsPlaying)
            {
                foreach (var entry in _pendingEntriesByKey.Values)
                {
                    if (
                        entry.BubbleController != null
                        && entry.BubbleController.IsTypewriterPlaying
                    )
                    {
                        entry.BubbleController.SkipTypewriter();
                    }
                }
                return;
            }

            // Phase 2: every text is revealed — advance the presentations that WAIT for input.
            // A reveal-only entry is left alone: its timeout owns when it leaves.
            //
            // Snapshot the callbacks before invoking: each next() may synchronously trigger the
            // block cleanup, which calls ClearPendingAdvanceForBlock and would mutate the
            // dictionary while we iterate.
            var callbacksToInvoke = new List<Action>(_pendingEntriesByKey.Count);
            var keysToClear = new List<string>(_pendingEntriesByKey.Count);

            foreach (var entry in _pendingEntriesByKey)
            {
                if (entry.Value.AdvanceCallback != null)
                {
                    callbacksToInvoke.Add(entry.Value.AdvanceCallback);
                    keysToClear.Add(entry.Key);
                }
            }

            foreach (var key in keysToClear)
            {
                _pendingEntriesByKey.Remove(key);
            }

            foreach (var advanceCallback in callbacksToInvoke)
            {
                advanceCallback();
            }
        }
    }
}
