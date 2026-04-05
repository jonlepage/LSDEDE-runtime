using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Marks a GameObject in the scene as a dialogue character and links it to
    /// an LSDE character ID from <see cref="lsdeCharacter"/>.
    /// Attach this to each character prefab instance in the scene and fill in the
    /// character ID in the Inspector (e.g. "l1", "l4", "boss").
    ///
    /// The <see cref="DialogueCharacterRegistry"/> discovers these markers at startup
    /// to build the character-to-GameObject mapping.
    /// </summary>
    public class DialogueCharacterMarker : MonoBehaviour
    {
        [SerializeField]
        [Tooltip(
            "The LSDE character ID this GameObject represents. "
                + "Must match a value from lsdeCharacter (e.g. l1, l2, l3, l4, boss)."
        )]
        private string _lsdeCharacterId;

        [SerializeField]
        [Tooltip(
            "A child Transform positioned above the character's head. "
                + "Speech bubbles will appear at this position."
        )]
        private Transform _bubbleAnchorPoint;

        /// <summary>
        /// The LSDE character ID this marker represents (e.g. "l1", "boss").
        /// Corresponds to constants in <see cref="lsdeCharacter"/>.
        /// </summary>
        public string LsdeCharacterId => _lsdeCharacterId;

        /// <summary>
        /// The world-space Transform where speech bubbles should be positioned.
        /// If not assigned, defaults to this GameObject's own Transform.
        /// </summary>
        public Transform BubbleAnchorPoint =>
            _bubbleAnchorPoint != null ? _bubbleAnchorPoint : transform;
    }
}
