using UnityEngine;
using UnityEngine.Serialization;

namespace LSDE.Demo
{
    /// <summary>
    /// Marks a GameObject in the scene as a dialogue character and links it to an LSDE card.
    /// Attach this to each character prefab instance in the scene and fill in the character
    /// name in the Inspector (e.g. "l1", "l4", "boss").
    ///
    /// <para>The value is the card's <b>Name</b> — what the writer typed in LSDE — not its uuid.
    /// That is the identity the rest of the payload speaks: the <c>party</c> dictionary asks about
    /// <c>party.l1</c>, and <c>moveCharacterAt</c> is called with <c>id: "l1"</c>. Rename a card
    /// in LSDE and this field has to follow.</para>
    ///
    /// The <see cref="DialogueCharacterRegistry"/> discovers these markers at startup
    /// to build the character-to-GameObject mapping.
    /// </summary>
    public class DialogueCharacterMarker : MonoBehaviour
    {
        [FormerlySerializedAs("_lsdeCharacterId")]
        [SerializeField]
        [Tooltip(
            "The LSDE card NAME this GameObject represents — l1, l2, l3, l4, boss. "
                + "It must match the card name in the blueprint, which is also the key the "
                + "party dictionary and the moveCharacterAt calls use."
        )]
        private string _lsdeCharacterName;

        [SerializeField]
        [Tooltip(
            "A child Transform positioned above the character's head. "
                + "Speech bubbles will appear at this position."
        )]
        private Transform _bubbleAnchorPoint;

        [SerializeField]
        [Tooltip(
            "Optional child Transform that the camera targets during moveCameraToLabel actions. "
                + "Place it further from the character to get a wider camera shot. "
                + "If not assigned, defaults to this GameObject's own Transform."
        )]
        private Transform _cameraAnchorPoint;

        /// <summary>
        /// The LSDE card name this marker represents (e.g. "l1", "boss") — the value
        /// <c>Card.Name</c> carries, and the key the <c>party</c> dictionary uses.
        /// </summary>
        public string LsdeCharacterName => _lsdeCharacterName;

        /// <summary>
        /// The world-space Transform where speech bubbles should be positioned.
        /// If not assigned, defaults to this GameObject's own Transform.
        /// </summary>
        public Transform BubbleAnchorPoint =>
            _bubbleAnchorPoint != null ? _bubbleAnchorPoint : transform;

        /// <summary>
        /// The world-space Transform the camera should target for this character.
        /// Used by moveCameraToLabel actions. If not assigned, defaults to
        /// this GameObject's own Transform.
        /// </summary>
        public Transform CameraAnchorPoint =>
            _cameraAnchorPoint != null ? _cameraAnchorPoint : transform;
    }
}
