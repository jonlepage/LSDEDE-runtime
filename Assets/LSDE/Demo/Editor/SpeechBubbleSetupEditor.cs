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
    /// This script only runs in the Unity Editor — it is NOT included in builds.
    /// The "Editor" folder name tells Unity to exclude it from runtime compilation.
    /// </summary>
    public static class SpeechBubbleSetupEditor
    {
        [MenuItem("LSDE/Setup Speech Bubbles On All Characters")]
        public static void SetupSpeechBubblesOnAllCharacters()
        {
            var allCharacterMarkers = Object.FindObjectsByType<DialogueCharacterMarker>(
                FindObjectsSortMode.None
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

        private static void CreateSpeechBubbleOnAnchor(
            Transform bubbleAnchorPoint,
            string characterIdentifier
        )
        {
            // --- Root: SpeechBubble (empty + BillboardRotation) ---
            var speechBubbleRoot = new GameObject($"SpeechBubble_{characterIdentifier}");
            speechBubbleRoot.transform.SetParent(bubbleAnchorPoint, false);
            speechBubbleRoot.transform.localPosition = Vector3.zero;

            var billboardRotation = speechBubbleRoot.AddComponent<BillboardRotation>();
            var bubbleController = speechBubbleRoot.AddComponent<SpeechBubbleController>();

            // --- Canvas (World Space) ---
            var canvasGameObject = new GameObject("Canvas");
            canvasGameObject.transform.SetParent(speechBubbleRoot.transform, false);

            var canvas = canvasGameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var canvasScaler = canvasGameObject.AddComponent<CanvasScaler>();
            canvasScaler.dynamicPixelsPerUnit = 10f;

            // Size the canvas in world units (small because it lives in 3D space)
            var canvasRectTransform = canvasGameObject.GetComponent<RectTransform>();
            canvasRectTransform.sizeDelta = new Vector2(300f, 150f);
            canvasRectTransform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            canvasRectTransform.localPosition = Vector3.zero;

            // --- BubblePanel (background Image) ---
            var bubblePanelGameObject = new GameObject("BubblePanel");
            bubblePanelGameObject.transform.SetParent(canvasGameObject.transform, false);

            var bubblePanelImage = bubblePanelGameObject.AddComponent<Image>();
            bubblePanelImage.color = new Color(0.95f, 0.95f, 0.95f, 0.9f);

            var bubblePanelRectTransform = bubblePanelGameObject.GetComponent<RectTransform>();
            bubblePanelRectTransform.anchorMin = Vector2.zero;
            bubblePanelRectTransform.anchorMax = Vector2.one;
            bubblePanelRectTransform.offsetMin = Vector2.zero;
            bubblePanelRectTransform.offsetMax = Vector2.zero;

            // Add vertical layout for name + text stacking
            var verticalLayout = bubblePanelGameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayout.padding = new RectOffset(15, 15, 10, 10);
            verticalLayout.spacing = 5f;
            verticalLayout.childAlignment = TextAnchor.UpperLeft;
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = true;
            verticalLayout.childForceExpandWidth = true;
            verticalLayout.childForceExpandHeight = false;

            // --- CharacterNameText (TextMeshProUGUI, bold, top) ---
            var characterNameGameObject = new GameObject("CharacterNameText");
            characterNameGameObject.transform.SetParent(bubblePanelGameObject.transform, false);

            var characterNameText = characterNameGameObject.AddComponent<TextMeshProUGUI>();
            characterNameText.text = "Character Name";
            characterNameText.fontSize = 24f;
            characterNameText.fontStyle = FontStyles.Bold;
            characterNameText.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            characterNameText.alignment = TextAlignmentOptions.TopLeft;

            var characterNameLayoutElement = characterNameGameObject.AddComponent<LayoutElement>();
            characterNameLayoutElement.preferredHeight = 30f;

            // --- DialogueContentText (TextMeshProUGUI, body) ---
            var dialogueContentGameObject = new GameObject("DialogueContentText");
            dialogueContentGameObject.transform.SetParent(bubblePanelGameObject.transform, false);

            var dialogueContentText = dialogueContentGameObject.AddComponent<TextMeshProUGUI>();
            dialogueContentText.text = "Dialogue text goes here...";
            dialogueContentText.fontSize = 18f;
            dialogueContentText.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            dialogueContentText.alignment = TextAlignmentOptions.TopLeft;
            dialogueContentText.enableWordWrapping = true;

            var dialogueContentLayoutElement =
                dialogueContentGameObject.AddComponent<LayoutElement>();
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

            // Mark the scene as dirty so Unity knows to save changes
            EditorUtility.SetDirty(speechBubbleRoot);
            EditorUtility.SetDirty(bubbleAnchorPoint.gameObject);

            Debug.Log(
                $"[LSDE Setup] Created speech bubble on '{bubbleAnchorPoint.parent?.name ?? "unknown"}' "
                    + $"(character: {characterIdentifier})."
            );
        }
    }
}
