using TMPro;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Writes a character's LSDE card name across its belly — <c>l1</c>, <c>l2</c>, <c>l3</c>,
    /// <c>l4</c>, <c>boss</c>.
    ///
    /// <para>The demo's whole point is watching a blueprint decide who speaks and who is missing,
    /// and none of that can be followed if the five rabbits are only told apart by a tint. The
    /// reference PixiJS demo carries the same label for the same reason
    /// (<c>src/renderer/sprite-overlay.ts</c>, <c>attachIdLabel</c>), and this is its Unity
    /// counterpart.</para>
    ///
    /// <para>It builds its own <c>TextMeshPro</c> child at startup: the name comes from
    /// <see cref="DialogueCharacterMarker"/>, which is already the one place a card name is stored,
    /// so there is nothing to wire in the Inspector and nothing that can drift out of sync with the
    /// blueprint. Drop the component on a character and it is done.</para>
    ///
    /// <para>Setup in Unity Editor: select the character GameObject (the one carrying
    /// <see cref="DialogueCharacterMarker"/>), then Inspector → <b>Add Component</b> → search
    /// "Character Id Label". Nothing else to fill in.</para>
    /// </summary>
    [RequireComponent(typeof(DialogueCharacterMarker))]
    public class CharacterIdLabel : MonoBehaviour
    {
        /// <summary>The name of the child this component creates, so it can be found again.</summary>
        private const string LabelChildName = "IdLabel";

        [SerializeField]
        [Tooltip(
            "How big the name reads in WORLD units, whatever the character's own scale. "
                + "The boss is 2.2x the rabbits; without this its label would be 2.2x too."
        )]
        private float _worldFontSize = 3.8f;

        [SerializeField]
        [Tooltip(
            "How far the label sits in FRONT of the sprite, in world units. The camera looks "
                + "along +Z, so the label is pushed toward -Z. Zero makes it fight the sprite."
        )]
        private float _towardCameraOffset = 0.06f;

        [SerializeField]
        [Tooltip("Fill colour of the name.")]
        private Color _textColor = Color.white;

        [SerializeField]
        [Tooltip(
            "Outline thickness, 0..1. The rabbits are tinted white, red, orange, green and "
                + "purple — only an outline stays readable on all five."
        )]
        [Range(0f, 1f)]
        private float _outlineWidth = 0.25f;

        [SerializeField]
        [Tooltip("Outline colour.")]
        private Color _outlineColor = Color.black;

        private TextMeshPro _labelText;
        private Camera _cachedMainCamera;

        private void Awake()
        {
            BuildLabel();
        }

        /// <summary>
        /// Create the label child and place it on the body.
        /// </summary>
        private void BuildLabel()
        {
            var marker = GetComponent<DialogueCharacterMarker>();
            if (marker == null || string.IsNullOrEmpty(marker.LsdeCharacterName))
            {
                Debug.LogWarning(
                    $"[LSDE] {name} has no card name on its DialogueCharacterMarker — "
                        + "no id label drawn."
                );
                return;
            }

            // Reuse the child if this component is re-added or the scene is replayed in the
            // editor, rather than stacking a second label on top of the first.
            var existing = transform.Find(LabelChildName);
            var labelObject =
                existing != null ? existing.gameObject : new GameObject(LabelChildName);
            labelObject.transform.SetParent(transform, false);

            _labelText = labelObject.GetComponent<TextMeshPro>();
            if (_labelText == null)
            {
                _labelText = labelObject.AddComponent<TextMeshPro>();
            }

            _labelText.text = marker.LsdeCharacterName;
            _labelText.alignment = TextAlignmentOptions.Center;
            _labelText.fontStyle = FontStyles.Bold;
            _labelText.color = _textColor;
            _labelText.textWrappingMode = TextWrappingModes.NoWrap;
            // A name is not a target. Nothing here carries a Collider, so the click raycast
            // cannot hit it — but the rect must not grow either, or a label wider than the
            // rabbit would push the layout around.
            _labelText.rectTransform.sizeDelta = new Vector2(4f, 1f);

            ApplyOutline();
            PlaceOnBody(labelObject.transform);
        }

        /// <summary>
        /// Turn the outline on. It lives on the font MATERIAL, and <c>fontMaterial</c> is what
        /// hands back a per-object instance — writing to <c>sharedMaterial</c> would outline every
        /// piece of text in the project that uses this font.
        /// </summary>
        private void ApplyOutline()
        {
            if (_outlineWidth <= 0f)
            {
                return;
            }

            var material = _labelText.fontMaterial;
            material.EnableKeyword(ShaderUtilities.Keyword_Outline);
            material.SetFloat(ShaderUtilities.ID_OutlineWidth, _outlineWidth);
            material.SetColor(ShaderUtilities.ID_OutlineColor, _outlineColor);
            _labelText.UpdateMeshPadding();
        }

        /// <summary>
        /// Centre the label on the sprite and cancel the character's scale.
        ///
        /// <para>The offset is measured from the renderer's own BOUNDS rather than assumed from the
        /// pivot: these sprites hang below their transform, and a character whose art is authored
        /// differently would otherwise wear its name in mid-air.</para>
        ///
        /// <para>The counter-scale is what makes <see cref="_worldFontSize"/> mean world units. A
        /// child inherits its parent's scale, so on the boss — 2.21x — an uncorrected label would
        /// render 2.21x too big.</para>
        /// </summary>
        private void PlaceOnBody(Transform labelTransform)
        {
            var scale = transform.lossyScale;
            labelTransform.localScale = new Vector3(
                scale.x != 0f ? 1f / scale.x : 1f,
                scale.y != 0f ? 1f / scale.y : 1f,
                scale.z != 0f ? 1f / scale.z : 1f
            );

            _labelText.fontSize = _worldFontSize;

            var spriteRenderer = GetComponent<SpriteRenderer>();
            var bodyCenter =
                spriteRenderer != null ? spriteRenderer.bounds.center : transform.position;

            labelTransform.position = new Vector3(
                bodyCenter.x,
                bodyCenter.y,
                bodyCenter.z - _towardCameraOffset
            );
        }

        /// <summary>
        /// Keep the name facing the camera, like the interaction hint does. The characters are flat
        /// sprites at zero rotation and the camera is tilted, so text left unturned reads slanted.
        /// </summary>
        private void LateUpdate()
        {
            if (_labelText == null)
            {
                return;
            }

            if (_cachedMainCamera == null)
            {
                _cachedMainCamera = Camera.main;
                if (_cachedMainCamera == null)
                {
                    return;
                }
            }

            _labelText.transform.rotation = _cachedMainCamera.transform.rotation;
        }
    }
}
