using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LSDE.Demo.Editor
{
    /// <summary>
    /// Editor-only utility that creates SpeechBubble hierarchies on all
    /// <see cref="DialogueCharacterMarker"/> components in the scene.
    /// Accessible via the Unity menu: LSDE > Setup Speech Bubbles.
    ///
    /// Also provides a "Rebuild" option that deletes existing bubbles and recreates them.
    ///
    /// This script only runs in the Unity Editor — it is NOT included in builds.
    /// The "Editor" folder name tells Unity to exclude it from runtime compilation.
    /// </summary>
    public static class SpeechBubbleSetupEditor
    {
        // No sprite needed — ProceduralSpeechBubble draws the shape with geometry
        private const float CanvasWidthInPixels = 450f;
        private const float CanvasHeightInPixels = 350f;
        private const float WorldScaleFactor = 0.007f;
        private const float BubblePaddingHorizontal = 60f;
        private const float BubblePaddingVertical = 40f;
        private const float BubblePaddingBottom = 120f;
        private const float NameFontSize = 28f;
        private const float DialogueFontSize = 20f;

        [MenuItem("LSDE/Setup Speech Bubbles On All Characters")]
        public static void SetupSpeechBubblesOnAllCharacters()
        {
            var allCharacterMarkers = Object.FindObjectsByType<DialogueCharacterMarker>(
                FindObjectsInactive.Exclude
            );

            if (allCharacterMarkers.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "LSDE Speech Bubble Setup",
                    "No DialogueCharacterMarker components found in the scene.\n"
                        + "Add DialogueCharacterMarker to your character GameObjects first.",
                    "OK"
                );
                return;
            }

            int createdCount = 0;

            foreach (var characterMarker in allCharacterMarkers)
            {
                var anchorPoint = characterMarker.BubbleAnchorPoint;

                // Skip if a SpeechBubbleController already exists on this anchor
                var existingBubbleController =
                    anchorPoint.GetComponentInChildren<SpeechBubbleController>(true);

                if (existingBubbleController != null)
                {
                    Debug.Log(
                        $"[LSDE Setup] Skipping '{characterMarker.gameObject.name}' "
                            + "— SpeechBubbleController already exists."
                    );
                    continue;
                }

                CreateSpeechBubbleOnAnchor(anchorPoint, characterMarker.LsdeCharacterId);
                createdCount++;
            }

            Debug.Log(
                $"[LSDE Setup] Created {createdCount} speech bubbles on {allCharacterMarkers.Length} characters."
            );

            EditorUtility.DisplayDialog(
                "LSDE Speech Bubble Setup",
                $"Created {createdCount} speech bubble(s).\n"
                    + $"({allCharacterMarkers.Length - createdCount} already had bubbles.)",
                "OK"
            );
        }

        [MenuItem("LSDE/Rebuild All Speech Bubbles")]
        public static void RebuildAllSpeechBubbles()
        {
            var allCharacterMarkers = Object.FindObjectsByType<DialogueCharacterMarker>(
                FindObjectsInactive.Exclude
            );

            // Destroy existing bubbles
            foreach (var characterMarker in allCharacterMarkers)
            {
                var anchorPoint = characterMarker.BubbleAnchorPoint;
                var existingBubbleController =
                    anchorPoint.GetComponentInChildren<SpeechBubbleController>(true);

                if (existingBubbleController != null)
                {
                    Undo.DestroyObjectImmediate(existingBubbleController.gameObject);
                }
            }

            // Recreate all
            SetupSpeechBubblesOnAllCharacters();
        }

        private static void CreateSpeechBubbleOnAnchor(
            Transform bubbleAnchorPoint,
            string characterIdentifier
        )
        {
            // --- Root: SpeechBubble (empty + BillboardRotation + SpeechBubbleController) ---
            var speechBubbleRoot = new GameObject($"SpeechBubble_{characterIdentifier}");
            Undo.RegisterCreatedObjectUndo(speechBubbleRoot, "Create Speech Bubble");
            speechBubbleRoot.transform.SetParent(bubbleAnchorPoint, false);
            speechBubbleRoot.transform.localPosition = Vector3.zero;

            speechBubbleRoot.AddComponent<BillboardRotation>();
            var bubbleController = speechBubbleRoot.AddComponent<SpeechBubbleController>();

            // --- Canvas (World Space) ---
            var canvasGameObject = new GameObject("Canvas");
            canvasGameObject.transform.SetParent(speechBubbleRoot.transform, false);

            var canvas = canvasGameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            canvasGameObject.AddComponent<CanvasScaler>();
            canvasGameObject.AddComponent<GraphicRaycaster>();

            var canvasRectTransform = canvasGameObject.GetComponent<RectTransform>();
            canvasRectTransform.sizeDelta = new Vector2(CanvasWidthInPixels, CanvasHeightInPixels);
            canvasRectTransform.localScale = new Vector3(
                WorldScaleFactor,
                WorldScaleFactor,
                WorldScaleFactor
            );
            canvasRectTransform.localPosition = Vector3.zero;
            // Pivot at bottom-center so the bubble grows upward from the anchor
            canvasRectTransform.pivot = new Vector2(0.5f, 0f);

            // --- ProceduralSpeechBubble (draws rounded rect + outline + tail with geometry) ---
            // No sprite needed — the shape is resolution-independent, never pixelates.
            // Equivalent to PixiJS Graphics.quadraticCurveTo() in the TS demo.
            var bubbleBackgroundGameObject = new GameObject("BubbleBackground");
            bubbleBackgroundGameObject.transform.SetParent(canvasGameObject.transform, false);

            bubbleBackgroundGameObject.AddComponent<CanvasRenderer>();
            var proceduralBubble =
                bubbleBackgroundGameObject.AddComponent<ProceduralSpeechBubble>();
            proceduralBubble.color = Color.white;
            proceduralBubble.raycastTarget = false;

            // Background fills the entire canvas
            var bubbleBackgroundRectTransform =
                bubbleBackgroundGameObject.GetComponent<RectTransform>();
            bubbleBackgroundRectTransform.anchorMin = Vector2.zero;
            bubbleBackgroundRectTransform.anchorMax = Vector2.one;
            bubbleBackgroundRectTransform.offsetMin = Vector2.zero;
            bubbleBackgroundRectTransform.offsetMax = Vector2.zero;

            // --- TextPanel (invisible container for text layout, sits on top of background) ---
            // This panel uses VerticalLayoutGroup + ContentSizeFitter to auto-size,
            // then the canvas matches its size. Large padding keeps text inside the bubble outline.
            var textPanelGameObject = new GameObject("TextPanel", typeof(RectTransform));
            textPanelGameObject.transform.SetParent(canvasGameObject.transform, false);

            var textPanelRectTransform = textPanelGameObject.GetComponent<RectTransform>();
            textPanelRectTransform.anchorMin = Vector2.zero;
            textPanelRectTransform.anchorMax = Vector2.one;
            textPanelRectTransform.offsetMin = Vector2.zero;
            textPanelRectTransform.offsetMax = Vector2.zero;

            // ContentSizeFitter drives the canvas size based on text content
            var contentSizeFitter = textPanelGameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // VerticalLayoutGroup: stack name + dialogue vertically
            // Large padding keeps text away from the hand-drawn outline and tail
            var verticalLayout = textPanelGameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayout.padding = new RectOffset(
                (int)BubblePaddingHorizontal,
                (int)BubblePaddingHorizontal,
                (int)BubblePaddingVertical,
                (int)BubblePaddingBottom
            );
            verticalLayout.spacing = 8f;
            verticalLayout.childAlignment = TextAnchor.UpperLeft;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = true;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;

            // --- CharacterNameText (TextMeshProUGUI, bold, top) ---
            var characterNameGameObject = new GameObject("CharacterNameText");
            characterNameGameObject.transform.SetParent(textPanelGameObject.transform, false);

            var characterNameText = characterNameGameObject.AddComponent<TextMeshProUGUI>();
            characterNameText.text = "Character Name";
            characterNameText.fontSize = NameFontSize;
            characterNameText.fontStyle = FontStyles.Bold;
            characterNameText.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            characterNameText.alignment = TextAlignmentOptions.TopLeft;
            characterNameText.textWrappingMode = TextWrappingModes.NoWrap;
            characterNameText.overflowMode = TextOverflowModes.Ellipsis;

            var characterNameLayoutElement = characterNameGameObject.AddComponent<LayoutElement>();
            characterNameLayoutElement.preferredHeight = NameFontSize + 8f;

            // --- DialogueContentText (TextMeshProUGUI, body, word-wrapped) ---
            var dialogueContentGameObject = new GameObject("DialogueContentText");
            dialogueContentGameObject.transform.SetParent(textPanelGameObject.transform, false);

            var dialogueContentText = dialogueContentGameObject.AddComponent<TextMeshProUGUI>();
            dialogueContentText.text = "Dialogue text...";
            dialogueContentText.fontSize = DialogueFontSize;
            dialogueContentText.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            dialogueContentText.alignment = TextAlignmentOptions.TopLeft;
            dialogueContentText.textWrappingMode = TextWrappingModes.Normal;
            dialogueContentText.overflowMode = TextOverflowModes.Overflow;

            var dialogueContentLayoutElement =
                dialogueContentGameObject.AddComponent<LayoutElement>();
            dialogueContentLayoutElement.preferredWidth =
                CanvasWidthInPixels - BubblePaddingHorizontal * 2;
            dialogueContentLayoutElement.flexibleHeight = 1f;

            // --- Wire SpeechBubbleController references via SerializedObject ---
            var serializedBubbleController = new SerializedObject(bubbleController);
            serializedBubbleController.FindProperty("_characterNameText").objectReferenceValue =
                characterNameText;
            serializedBubbleController.FindProperty("_dialogueContentText").objectReferenceValue =
                dialogueContentText;
            serializedBubbleController.ApplyModifiedProperties();

            // Start hidden — the presenter will show it when a DIALOG block fires
            speechBubbleRoot.SetActive(false);

            EditorUtility.SetDirty(speechBubbleRoot);
            EditorUtility.SetDirty(bubbleAnchorPoint.gameObject);

            Debug.Log(
                $"[LSDE Setup] Created speech bubble on '{bubbleAnchorPoint.parent?.name ?? "unknown"}' "
                    + $"(character: {characterIdentifier})."
            );
        }
    }
}
