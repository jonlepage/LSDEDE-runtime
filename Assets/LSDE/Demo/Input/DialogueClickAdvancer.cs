using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LSDE.Demo
{
    /// <summary>
    /// Listens for mouse clicks and advances the dialogue flow when a click occurs.
    /// The <see cref="BubbleDialoguePresenter"/> stores the pending advance callback here
    /// when a DIALOG block is displayed. On click, this component invokes the callback
    /// and clears it — advancing the engine to the next block.
    ///
    /// Clicking does NOT block other interactions (e.g. player movement).
    /// The click simply also advances dialogue if one is pending.
    /// </summary>
    public class DialogueClickAdvancer : MonoBehaviour
    {
        private Action _pendingAdvanceCallback;

        /// <summary>
        /// Whether there is a pending dialogue advance waiting for a click.
        /// </summary>
        public bool HasPendingAdvance => _pendingAdvanceCallback != null;

        /// <summary>
        /// Store the advance callback to be invoked on the next click.
        /// Called by the presenter when a DIALOG block is shown to the player.
        /// </summary>
        /// <param name="advanceCallback">The engine's Next callback that advances to the next block.</param>
        public void SetPendingAdvance(Action advanceCallback)
        {
            _pendingAdvanceCallback = advanceCallback;
        }

        /// <summary>
        /// Clear any pending advance callback.
        /// Called during block cleanup or when switching to a CHOICE block
        /// (choices use their own selection buttons, not click-anywhere).
        /// </summary>
        public void ClearPendingAdvance()
        {
            _pendingAdvanceCallback = null;
        }

        /// <summary>
        /// Unity calls Update every frame. We check for left mouse button press
        /// using the new Input System (UnityEngine.InputSystem).
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

            if (currentMouse.leftButton.wasPressedThisFrame)
            {
                var callbackToInvoke = _pendingAdvanceCallback;
                _pendingAdvanceCallback = null;
                callbackToInvoke();
            }
        }
    }
}
