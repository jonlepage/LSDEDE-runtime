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
    /// Click behavior depends on the current state:
    /// <list type="bullet">
    ///   <item>If any typewriter is playing → first click skips ALL typewriters (reveals all text)</item>
    ///   <item>If all text is fully revealed → click advances ALL waiting blocks (broadcast)</item>
    /// </list>
    ///
    /// The <see cref="BubbleDialoguePresenter"/> registers pending advance callbacks here
    /// when DIALOG blocks are displayed. It also provides references to the active
    /// <see cref="SpeechBubbleController"/> instances so we can check typewriter state and skip them.
    ///
    /// In single-track mode (e.g. simpleDialogFlow), only one entry exists in the dictionary
    /// at a time — behavior is identical to a single-callback model.
    ///
    /// In multi-track mode (e.g. multiTracks), multiple entries coexist. A single click
    /// broadcasts to ALL waiting blocks: every track that needs acknowledgement reacts
    /// to the same player interaction at once.
    ///
    /// Clicking does NOT block other interactions (e.g. player movement).
    /// The click simply also advances dialogue if one is pending.
    /// </summary>
    public class DialogueClickAdvancer : MonoBehaviour
    {
        /// <summary>
        /// Holds the advance callback and bubble controller for a single block
        /// that is waiting for player input to proceed.
        /// </summary>
        private struct PendingAdvanceEntry
        {
            public Action AdvanceCallback;
            public SpeechBubbleController BubbleController;
        }

        /// <summary>
        /// All blocks currently waiting for a player click to advance.
        /// Keyed by block UUID so that individual entries can be added/removed
        /// independently when parallel dialogue tracks are active.
        /// </summary>
        private readonly Dictionary<string, PendingAdvanceEntry> _pendingAdvancesByBlockUuid =
            new Dictionary<string, PendingAdvanceEntry>();

        /// <summary>
        /// The frame number on which <see cref="SetPendingAdvance"/> was last called.
        /// Clicks on this exact frame are ignored to prevent the "phantom click" problem:
        /// the player's click to START the dialogue (via NPC interaction) would otherwise
        /// also be detected as a click to skip the typewriter, because both the dialogue
        /// trigger and this advancer process the same <c>wasPressedThisFrame</c> input
        /// within a single Unity frame.
        /// </summary>
        private int _armedOnFrame = -1;

        /// <summary>
        /// Whether there are any pending dialogue advances waiting for a click.
        /// </summary>
        public bool HasPendingAdvance => _pendingAdvancesByBlockUuid.Count > 0;

        /// <summary>
        /// Store the advance callback and active bubble controller for a specific block.
        /// Called by the presenter when a DIALOG block is shown and needs player input to advance.
        /// </summary>
        /// <param name="blockUuid">The UUID of the dialogue block, used as dictionary key.</param>
        /// <param name="advanceCallback">The engine's Next callback that advances to the next block.</param>
        /// <param name="activeBubbleController">
        /// The currently visible bubble controller, used to check typewriter state
        /// and skip it on first click. Can be null if no typewriter support is needed.
        /// </param>
        public void SetPendingAdvance(
            string blockUuid,
            Action advanceCallback,
            SpeechBubbleController activeBubbleController = null
        )
        {
            _pendingAdvancesByBlockUuid[blockUuid] = new PendingAdvanceEntry
            {
                AdvanceCallback = advanceCallback,
                BubbleController = activeBubbleController,
            };
            _armedOnFrame = Time.frameCount;
        }

        /// <summary>
        /// Clear the pending advance callback for a specific block.
        /// Called during individual block cleanup when the engine moves past this block,
        /// or when a timeout coroutine auto-advances the block.
        /// </summary>
        /// <param name="blockUuid">The UUID of the block to clear.</param>
        public void ClearPendingAdvanceForBlock(string blockUuid)
        {
            _pendingAdvancesByBlockUuid.Remove(blockUuid);
        }

        /// <summary>
        /// Clear all pending advance callbacks.
        /// Called during scene exit or when switching to a CHOICE block
        /// (choices use their own selection buttons, not click-anywhere).
        /// </summary>
        public void ClearAllPendingAdvances()
        {
            _pendingAdvancesByBlockUuid.Clear();
        }

        /// <summary>
        /// Unity calls Update every frame. We check for left mouse button press
        /// using the new Input System (UnityEngine.InputSystem).
        ///
        /// Two-phase broadcast click behavior:
        /// 1. If any typewriter is playing → skip ALL typewriters (reveal all text), do NOT advance yet
        /// 2. If all text is fully visible → invoke ALL advance callbacks (broadcast to all waiting blocks)
        ///
        /// The broadcast pattern models "press to continue" — every track that needs
        /// acknowledgement reacts to the same player interaction at once.
        /// </summary>
        private void Update()
        {
            if (_pendingAdvancesByBlockUuid.Count == 0)
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

            // Ignore clicks on the same frame the advancer was armed.
            // This prevents the "phantom click" where the player's click to START
            // the dialogue (via NPC interaction) is also detected as a click to
            // skip the typewriter — both systems see the same wasPressedThisFrame.
            if (Time.frameCount == _armedOnFrame)
            {
                return;
            }

            // Phase 1: If ANY typewriter is still playing, skip ALL typewriters.
            // Do NOT advance yet — let the player read the fully revealed text.
            bool anyTypewriterIsPlaying = false;
            foreach (var entry in _pendingAdvancesByBlockUuid.Values)
            {
                if (entry.BubbleController != null && entry.BubbleController.IsTypewriterPlaying)
                {
                    anyTypewriterIsPlaying = true;
                    break;
                }
            }

            if (anyTypewriterIsPlaying)
            {
                foreach (var entry in _pendingAdvancesByBlockUuid.Values)
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

            // Phase 2: All text is fully visible — advance ALL waiting blocks.
            // Snapshot callbacks before clearing to avoid mutation during invocation.
            // Each next() call may synchronously trigger PresentBlockCleanup which calls
            // ClearPendingAdvanceForBlock, modifying the dictionary while we iterate.
            // By clearing first and invoking from the snapshot, cleanup re-entry calls
            // Remove on an already-empty dictionary — safe no-op.
            var callbacksToInvoke = new List<Action>(_pendingAdvancesByBlockUuid.Count);
            foreach (var entry in _pendingAdvancesByBlockUuid.Values)
            {
                callbacksToInvoke.Add(entry.AdvanceCallback);
            }
            _pendingAdvancesByBlockUuid.Clear();

            foreach (var advanceCallback in callbacksToInvoke)
            {
                advanceCallback();
            }
        }
    }
}
