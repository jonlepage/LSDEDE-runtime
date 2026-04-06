using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LSDE.Demo
{
    /// <summary>
    /// Listens for mouse clicks and manages dialogue flow advancement with typewriter support.
    ///
    /// Click behavior depends on the current state:
    /// <list type="bullet">
    ///   <item>If the typewriter is playing → first click skips the typewriter (reveals all text)</item>
    ///   <item>If text is fully revealed → click advances the engine to the next block</item>
    /// </list>
    ///
    /// The <see cref="BubbleDialoguePresenter"/> stores the pending advance callback here
    /// when a DIALOG block is displayed. It also provides a reference to the active
    /// <see cref="SpeechBubbleController"/> so we can check typewriter state and skip it.
    ///
    /// Clicking does NOT block other interactions (e.g. player movement).
    /// The click simply also advances dialogue if one is pending.
    /// </summary>
    public class DialogueClickAdvancer : MonoBehaviour
    {
        private Action _pendingAdvanceCallback;
        private SpeechBubbleController _activeBubbleController;

        /// <summary>
        /// Whether there is a pending dialogue advance waiting for a click.
        /// </summary>
        public bool HasPendingAdvance => _pendingAdvanceCallback != null;

        /// <summary>
        /// Store the advance callback and active bubble controller.
        /// Called by the presenter when a DIALOG block is shown to the player.
        /// </summary>
        /// <param name="advanceCallback">The engine's Next callback that advances to the next block.</param>
        /// <param name="activeBubbleController">
        /// The currently visible bubble controller, used to check typewriter state
        /// and skip it on first click. Can be null if no typewriter support is needed.
        /// </param>
        public void SetPendingAdvance(
            Action advanceCallback,
            SpeechBubbleController activeBubbleController = null
        )
        {
            _pendingAdvanceCallback = advanceCallback;
            _activeBubbleController = activeBubbleController;
        }

        /// <summary>
        /// Clear any pending advance callback and active bubble reference.
        /// Called during block cleanup or when switching to a CHOICE block
        /// (choices use their own selection buttons, not click-anywhere).
        /// </summary>
        public void ClearPendingAdvance()
        {
            _pendingAdvanceCallback = null;
            _activeBubbleController = null;
        }

        /// <summary>
        /// Unity calls Update every frame. We check for left mouse button press
        /// using the new Input System (UnityEngine.InputSystem).
        ///
        /// Two-phase click behavior:
        /// 1. If typewriter is playing → skip it (reveal all text), do NOT advance yet
        /// 2. If text is fully visible → invoke advance callback to go to next block
        /// </summary>
        private void Update()
        {
            if (_pendingAdvanceCallback == null)
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

            // Phase 1: If typewriter is still playing, skip it (reveal all text)
            // but do NOT advance to the next block yet — let the player read.
            if (_activeBubbleController != null && _activeBubbleController.IsTypewriterPlaying)
            {
                _activeBubbleController.SkipTypewriter();
                return;
            }

            // Phase 2: Text is fully visible — advance to the next block
            var callbackToInvoke = _pendingAdvanceCallback;
            _pendingAdvanceCallback = null;
            _activeBubbleController = null;
            callbackToInvoke();
        }
    }
}
