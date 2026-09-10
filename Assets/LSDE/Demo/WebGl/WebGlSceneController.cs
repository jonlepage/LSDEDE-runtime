using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LSDE.Runtime;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// WebGL bridge controller that receives <c>sendMessage()</c> calls from the React wrapper
    /// and switches between LSDE demo scenes.
    ///
    /// Attach to a persistent GameObject named exactly <c>"WebGlSceneController"</c>
    /// so JavaScript can target it via <c>sendMessage("WebGlSceneController", "SelectScene", sceneName)</c>.
    ///
    /// Flow:
    /// <code>
    /// React sidebar click
    ///   → sendMessage("WebGlSceneController", "SelectScene", "simpleDialogFlow")
    ///   → this script maps "simpleDialogFlow" → the stable scene id (sc_60ql8la3)
    ///   → force-stops any active scene
    ///   → resets game state, character positions, triggers
    ///   → launches the new scene via DemoSceneTrigger
    ///   → notifies React via jslib events
    /// </code>
    ///
    /// Setup in Unity Editor:
    /// 1. Create an empty GameObject named exactly <c>"WebGlSceneController"</c>
    /// 2. Attach this script
    /// 3. Wire all serialized references in the Inspector
    /// </summary>
    public class WebGlSceneController : MonoBehaviour
    {
        [Header("Core References")]
        [SerializeField]
        [Tooltip("The DemoSceneTrigger that manages engine initialization and scene launching.")]
        private DemoSceneTrigger _demoSceneTrigger;

        [SerializeField]
        [Tooltip("The game state that holds inventory, party, and variables.")]
        private DemoGameState _demoGameState;

        [SerializeField]
        [Tooltip("The action executor for resetting camera state between scenes.")]
        private DemoActionExecutor _demoActionExecutor;

        [SerializeField]
        [Tooltip("The party follow controller on the player, for resetting follower state.")]
        private PartyFollowController _partyFollowController;

        [SerializeField]
        [Tooltip("The character registry for looking up all characters in the scene.")]
        private DialogueCharacterRegistry _characterRegistry;

        [SerializeField]
        [Tooltip("The dialogue engine bootstrap for accessing the engine API.")]
        private DialogueEngineBootstrap _dialogueEngineBootstrap;

        [Header("Editor Testing")]
        [SerializeField]
        [Tooltip(
            "Select a scene here, then right-click this component → 'Test: Switch Scene' "
                + "to test scene switching directly in Play mode (no React needed)."
        )]
        private DemoSceneSelection _editorTestScene = DemoSceneSelection.SimpleDialogFlow;

        [SerializeField]
        [Tooltip(
            "Pick a language here while in Play mode: it applies immediately and the running "
                + "demo restarts so the text on screen changes too. No React needed."
        )]
        private DemoLocale _editorTestLocale = DemoLocale.French;

        /// <summary>
        /// Maps friendly scene names (used by the React sidebar) to the STABLE scene ids of
        /// the generated LsdedeDemoTsBlueprintIds.Scenes constants. The id is what survives a
        /// rename, so it is what a value stored outside the payload should hold.
        /// Populated in <see cref="Awake"/>.
        /// </summary>
        private readonly Dictionary<string, string> _sceneNameToUuid = new();

        /// <summary>
        /// Stores the initial world positions of all characters at startup.
        /// Used to reset character positions when switching between demo scenes.
        /// Key = LSDE character ID, Value = initial world position.
        /// </summary>
        private readonly Dictionary<string, Vector3> _initialCharacterPositions = new();

        /// <summary>
        /// The friendly name of the currently active scene (e.g. "simpleDialogFlow").
        /// Null if no scene is active.
        /// </summary>
        private string _currentSceneName;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void NotifyUnityReady();

        [DllImport("__Internal")]
        private static extern void NotifySceneStarted(string sceneName);

        [DllImport("__Internal")]
        private static extern void NotifySceneCompleted(string sceneName);
#else
        /// <summary>Stub for non-WebGL builds — logs to console instead.</summary>
        private static void NotifyUnityReady()
        {
            Debug.Log("[LSDE WebGL] NotifyUnityReady (editor stub)");
        }

        /// <summary>Stub for non-WebGL builds — logs to console instead.</summary>
        private static void NotifySceneStarted(string sceneName)
        {
            Debug.Log($"[LSDE WebGL] NotifySceneStarted: {sceneName} (editor stub)");
        }

        /// <summary>Stub for non-WebGL builds — logs to console instead.</summary>
        private static void NotifySceneCompleted(string sceneName)
        {
            Debug.Log($"[LSDE WebGL] NotifySceneCompleted: {sceneName} (editor stub)");
        }
#endif

        private void Awake()
        {
            BuildSceneNameMapping();
        }

        private IEnumerator Start()
        {
            StoreInitialCharacterPositions();
            NotifyUnityReady();

            // Wait for the first physics step to complete.
            // SphereColliders created in Awake() trigger OnTriggerEnter if the player
            // is already overlapping. We let that happen, THEN reset everything clean.
            yield return new WaitForFixedUpdate();

            if (SelectionToSceneName.TryGetValue(_editorTestScene, out string initialSceneName))
            {
                SelectScene(initialSceneName);
            }
        }

        /// <summary>
        /// Called from JavaScript via <c>sendMessage("WebGlSceneController", "SetLocale", locale)</c>.
        /// Changes the engine locale at runtime so dialogue text switches language immediately.
        /// Supported locales: "fr", "en", "ja", "zh".
        ///
        /// Must be <c>public void</c> with a single <c>string</c> parameter
        /// (Unity <c>sendMessage</c> constraint).
        /// </summary>
        /// <param name="locale">The locale code (e.g. "fr", "en", "ja", "zh").</param>
        public void SetLocale(string locale)
        {
            if (_dialogueEngineBootstrap?.Engine == null)
            {
                Debug.LogWarning("[LSDE WebGL] Cannot set locale — engine not initialized.");
                return;
            }

            // Through LsdeText, which refuses a code the payload does not declare instead
            // of letting the engine throw — a language dropdown is not worth crashing a
            // scene over.
            if (LsdeText.SetCurrentLanguage(locale))
            {
                Debug.Log($"[LSDE WebGL] Locale changed to: {locale}");
            }
        }

        /// <summary>
        /// Called from JavaScript via <c>sendMessage("WebGlSceneController", "SelectScene", sceneName)</c>.
        /// Receives the friendly scene name, maps it to a UUID, resets all state,
        /// and launches the corresponding LSDE dialogue scene.
        ///
        /// Must be <c>public void</c> with a single <c>string</c> parameter
        /// (Unity <c>sendMessage</c> constraint).
        /// </summary>
        /// <param name="sceneName">
        /// The friendly scene name (e.g. "simpleDialogFlow", "advanceFullDemo").
        /// Must match a key in <see cref="_sceneNameToUuid"/>.
        /// </param>
        public void SelectScene(string sceneName)
        {
            if (!_sceneNameToUuid.TryGetValue(sceneName, out string sceneUuid))
            {
                Debug.LogWarning(
                    $"[LSDE WebGL] Unknown scene name: '{sceneName}'. "
                        + "Available: simpleDialogFlow, multiTracks, simpleChoices, "
                        + "simpleAction, simpleCondition, conditionDispatch, advanceFullDemo"
                );
                return;
            }

            Debug.Log($"[LSDE WebGL] SelectScene('{sceneName}') → UUID: {sceneUuid}");

            // Force-stop any active scene before launching the new one
            if (_demoSceneTrigger.IsDialogueSceneActive)
            {
                _demoSceneTrigger.ForceStopActiveScene();
            }

            // Reset all demo state to initial conditions
            ResetAllDemoState();

            // Update visibility of scene-filtered GameObjects
            SceneVisibilityFilter.NotifySceneChanged(sceneName);

            // Assign the selected scene UUID to all dialogue proximity triggers
            // so the same NPC (e.g. l1) launches the correct scene when clicked
            AssignSceneUuidToTriggers(sceneUuid);

            // Notify React — the scene environment is ready, waiting for player interaction
            _currentSceneName = sceneName;
            NotifySceneStarted(sceneName);
        }

        /// <summary>
        /// Called by <see cref="DemoSceneTrigger"/> when a scene exits naturally.
        /// Override the scene exit to notify React of completion.
        /// This must be wired as a listener — see <see cref="Start"/>.
        /// </summary>
        public void HandleSceneCompleted()
        {
            if (_currentSceneName != null)
            {
                NotifySceneCompleted(_currentSceneName);
            }
        }

        /// <summary>
        /// Reset all demo systems to their initial state before launching a new scene.
        /// This ensures a clean slate: no stale party members, no displaced characters,
        /// no lingering trigger states.
        /// </summary>
        private void ResetAllDemoState()
        {
            // Reset game state (inventory, party, variables)
            if (_demoGameState != null)
            {
                _demoGameState.ResetToInitialState();
            }

            // Reset party follow controller (clear followers, clear trail)
            if (_partyFollowController != null)
            {
                _partyFollowController.ResetFollowers();
            }

            // Reset camera to follow mode
            if (_demoActionExecutor != null)
            {
                _demoActionExecutor.ResetCameraState();
            }

            // Reset character positions to their initial locations
            ResetCharacterPositions();

            // Reset all triggers in the scene
            ResetAllTriggers();
        }

        /// <summary>
        /// Restore all characters to their initial positions (captured at startup).
        /// Stops any active movement first to prevent the character from walking
        /// back to a stale target.
        /// </summary>
        private void ResetCharacterPositions()
        {
            foreach (var entry in _initialCharacterPositions)
            {
                string characterId = entry.Key;
                Vector3 initialPosition = entry.Value;

                var characterMarker = _characterRegistry.FindMarkerByCharacterName(characterId);

                if (characterMarker == null)
                {
                    continue;
                }

                var movementController =
                    characterMarker.GetComponent<CharacterMovementController>();

                if (movementController != null)
                {
                    movementController.StopMovement();
                }

                characterMarker.transform.position = initialPosition;
            }
        }

        /// <summary>
        /// Find and reset all dialogue and walk-in triggers in the scene.
        /// This re-arms triggers so they can fire again in the new demo.
        /// </summary>
        private void ResetAllTriggers()
        {
            var dialogueTriggers = FindObjectsByType<DialogueProximityTrigger>();
            foreach (var trigger in dialogueTriggers)
            {
                trigger.ResetTrigger();
            }

            var walkInTriggers = FindObjectsByType<WalkInSceneTrigger>();
            foreach (var trigger in walkInTriggers)
            {
                trigger.ResetTrigger();
            }

            var recruitmentTriggers = FindObjectsByType<PartyRecruitmentTrigger>();
            foreach (var trigger in recruitmentTriggers)
            {
                trigger.ResetTrigger();
            }
        }

        /// <summary>
        /// Maps each <see cref="DemoSceneSelection"/> value to its friendly scene name.
        /// Used by the editor test button.
        /// </summary>
        private static readonly Dictionary<DemoSceneSelection, string> SelectionToSceneName = new()
        {
            { DemoSceneSelection.SimpleDialogFlow, "simpleDialogFlow" },
            { DemoSceneSelection.MultiTracks, "multiTracks" },
            { DemoSceneSelection.SimpleChoices, "simpleChoices" },
            { DemoSceneSelection.SimpleAction, "simpleAction" },
            { DemoSceneSelection.SimpleCondition, "simpleCondition" },
            { DemoSceneSelection.ConditionDispatch, "conditionDispatch" },
            { DemoSceneSelection.AdvanceFullDemo, "advanceFullDemo" },
        };

        /// <summary>
        /// Maps each <see cref="DemoLocale"/> value to its LSDE locale code string.
        /// Used by the editor test button.
        /// </summary>
        private static readonly Dictionary<DemoLocale, string> LocaleToCode = new()
        {
            { DemoLocale.French, "fr" },
            { DemoLocale.English, "en" },
            { DemoLocale.Japanese, "ja" },
            { DemoLocale.Chinese, "zh" },
        };

        /// <summary>
        /// Editor test: switch to the scene selected in <see cref="_editorTestScene"/>.
        /// Right-click this component in the Inspector → "Test: Switch Scene".
        /// Only works in Play mode.
        /// </summary>
        [ContextMenu("Test: Switch Scene")]
        private void EditorTestSwitchScene()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[LSDE WebGL] Test only works in Play mode.");
                return;
            }

            if (SelectionToSceneName.TryGetValue(_editorTestScene, out string sceneName))
            {
                SelectScene(sceneName);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor: applies <see cref="_editorTestLocale"/> the moment it is changed in the
        /// Inspector, so picking a language is all there is to do.
        ///
        /// <para>Two things made the old right-click command confusing enough to be useless.
        /// Changing the dropdown did nothing on its own — it only stored a value, and the
        /// command still had to be found in a context menu on the component header. And even
        /// then the screen did not change: the engine resolves a line's text when its block is
        /// dispatched, so a bubble already showing keeps the words it was handed. Restarting
        /// the running demo is what actually puts the new language on screen.</para>
        ///
        /// <para>The work is deferred to <see cref="Update"/> rather than done here: restarting
        /// a scene tears down and rebuilds objects, which Unity forbids during OnValidate.</para>
        /// </summary>
        private DemoLocale _appliedEditorLocale = DemoLocale.French;
        private bool _editorLocaleChangePending;

        private void OnValidate()
        {
            if (!Application.isPlaying || _editorTestLocale == _appliedEditorLocale)
            {
                return;
            }

            _editorLocaleChangePending = true;
        }

        private void Update()
        {
            if (!_editorLocaleChangePending)
            {
                return;
            }

            _editorLocaleChangePending = false;
            _appliedEditorLocale = _editorTestLocale;

            if (!LocaleToCode.TryGetValue(_editorTestLocale, out string localeCode))
            {
                return;
            }

            SetLocale(localeCode);

            if (_currentSceneName != null)
            {
                SelectScene(_currentSceneName);
            }
        }
#endif

        /// <summary>
        /// Assign the selected scene UUID to all <see cref="DialogueProximityTrigger"/>
        /// instances in the scene. This allows the same NPC (e.g. l1) to launch different
        /// LSDE scenes depending on which demo the user selected from the React sidebar.
        ///
        /// Triggers on hidden GameObjects (via <see cref="SceneVisibilityFilter"/>) are
        /// not interactable, so assigning the UUID to all triggers is safe.
        /// Walk-in triggers keep their fixed UUIDs from the Inspector.
        /// </summary>
        /// <param name="sceneUuid">The LSDE scene UUID to assign.</param>
        private void AssignSceneUuidToTriggers(string sceneUuid)
        {
            var dialogueTriggers = FindObjectsByType<DialogueProximityTrigger>();

            foreach (var trigger in dialogueTriggers)
            {
                trigger.SetSceneUuid(sceneUuid);
            }

            Debug.Log(
                $"[LSDE WebGL] Assigned scene UUID to {dialogueTriggers.Length} dialogue triggers."
            );
        }

        /// <summary>
        /// Build the friendly-name-to-id mapping from the generated
        /// <c>LsdedeDemoTsBlueprintIds.Scenes</c> constants.
        /// React sends these friendly names via <c>sendMessage</c>.
        /// </summary>
        private void BuildSceneNameMapping()
        {
            _sceneNameToUuid.Add(
                "simpleDialogFlow",
                LsdedeDemoTsBlueprintIds.Scenes.simpleDialogFlow
            );
            _sceneNameToUuid.Add("multiTracks", LsdedeDemoTsBlueprintIds.Scenes.multiTracks);
            _sceneNameToUuid.Add("simpleChoices", LsdedeDemoTsBlueprintIds.Scenes.simpleChoices);
            _sceneNameToUuid.Add("simpleAction", LsdedeDemoTsBlueprintIds.Scenes.simpleAction);
            _sceneNameToUuid.Add(
                "simpleCondition",
                LsdedeDemoTsBlueprintIds.Scenes.simpleCondition
            );
            _sceneNameToUuid.Add(
                "conditionDispatch",
                LsdedeDemoTsBlueprintIds.Scenes.conditionDispatch
            );
            _sceneNameToUuid.Add(
                "advanceFullDemo",
                LsdedeDemoTsBlueprintIds.Scenes.advanceFullDemo
            );
        }

        /// <summary>
        /// Capture the initial world position of every character in the scene.
        /// Called once at <see cref="Start"/> — these positions are used by
        /// <see cref="ResetCharacterPositions"/> to restore characters between demo switches.
        /// </summary>
        private void StoreInitialCharacterPositions()
        {
            if (_characterRegistry == null)
            {
                Debug.LogWarning(
                    "[LSDE WebGL] No DialogueCharacterRegistry assigned. "
                        + "Character position reset will not work."
                );
                return;
            }

            foreach (string characterId in DemoCharacterNames.All)
            {
                var characterMarker = _characterRegistry.FindMarkerByCharacterName(characterId);

                if (characterMarker != null)
                {
                    _initialCharacterPositions[characterId] = characterMarker.transform.position;
                }
            }

            Debug.Log(
                $"[LSDE WebGL] Stored initial positions for "
                    + $"{_initialCharacterPositions.Count} characters."
            );
        }
    }
}
