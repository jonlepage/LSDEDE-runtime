using System.Collections.Generic;
using System.Linq;
using LSDE.Runtime;
using LsdeDialogEngine;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Demo trigger that launches LSDE dialogue scenes.
    /// Supports two presenter modes toggled via the Inspector:
    /// - Visual mode: speech bubbles above characters, click to advance
    /// - Console mode: Debug.Log output, auto-advance (Phase 1 behavior)
    ///
    /// The engine is initialized on Start(). Scenes can then be launched externally
    /// via <see cref="LaunchDialogueScene"/> (e.g. by a <see cref="DialogueProximityTrigger"/>).
    ///
    /// Setup in Unity Editor:
    /// 1. Create an empty GameObject named "DemoSceneTrigger"
    /// 2. Attach this script
    /// 3. Drag the DialogueEngineBootstrap onto the "Dialogue Engine Bootstrap" field
    /// 4. For visual mode: also assign CharacterRegistry, ClickAdvancer, BubblePresenter
    /// </summary>
    public class DemoSceneTrigger : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField]
        [Tooltip("Reference to the DialogueEngineBootstrap component in the scene.")]
        private DialogueEngineBootstrap _dialogueEngineBootstrap;

        [Header("Visual Presenter (Phase 2b)")]
        [SerializeField]
        [Tooltip(
            "Enable to use speech bubbles above characters. "
                + "Disable to use console-only output (Phase 1 mode)."
        )]
        private bool _useVisualPresenter = true;

        [SerializeField]
        [Tooltip("Registry that maps LSDE character IDs to scene GameObjects.")]
        private DialogueCharacterRegistry _characterRegistry;

        [SerializeField]
        [Tooltip("Click handler that advances dialogue on mouse click.")]
        private DialogueClickAdvancer _dialogueClickAdvancer;

        [SerializeField]
        [Tooltip("Visual presenter that displays speech bubbles above characters.")]
        private BubbleDialoguePresenter _bubbleDialoguePresenter;

        [Header("Auto-Launch (optional)")]
        [SerializeField]
        [LsdeSceneSelector]
        [Tooltip(
            "If set, this scene will be launched automatically on Start(). "
                + "Leave on '(none)' to wait for an external trigger (e.g. DialogueProximityTrigger)."
        )]
        private string _autoLaunchSceneUuid;

        /// <summary>
        /// Whether a dialogue scene is currently active (started but not yet exited).
        /// Used by triggers to prevent re-triggering while dialogue is in progress.
        /// </summary>
        public bool IsDialogueSceneActive { get; private set; }

        /// <summary>
        /// Unity calls Start() once when the GameObject becomes active.
        /// Wires dependencies and initializes the engine. Optionally launches a scene
        /// if <see cref="_autoLaunchSceneUuid"/> is set.
        /// </summary>
        private void Start()
        {
            if (_dialogueEngineBootstrap == null)
            {
                Debug.LogError(
                    "[LSDE Demo] DialogueEngineBootstrap reference is missing. "
                        + "Drag it onto this component in the Inspector."
                );
                return;
            }

            // Wire presenter and resolvers based on the selected mode
            if (_useVisualPresenter)
            {
                WireVisualPresenter();
            }
            else
            {
                WireConsolePresenter();
            }

            // Initialize the engine (parse blueprint, register handlers, etc.)
            _dialogueEngineBootstrap.InitializeEngine();

            // Abort if the blueprint has errors
            if (_dialogueEngineBootstrap.LastDiagnosticReport.Errors.Count > 0)
            {
                Debug.LogError("[LSDE Demo] Blueprint has errors — aborting scene launch.");
                return;
            }

            // Auto-launch a scene if configured, otherwise wait for external trigger
            if (!string.IsNullOrEmpty(_autoLaunchSceneUuid))
            {
                LaunchDialogueScene(_autoLaunchSceneUuid);
            }
        }

        /// <summary>
        /// Wire the visual presenter: speech bubbles above characters, click to advance.
        /// Uses the scene-based character registry instead of the simple demo resolver.
        /// </summary>
        private void WireVisualPresenter()
        {
            if (
                _characterRegistry == null
                || _dialogueClickAdvancer == null
                || _bubbleDialoguePresenter == null
            )
            {
                Debug.LogWarning(
                    "[LSDE Demo] Visual presenter references are missing. "
                        + "Falling back to console mode."
                );
                WireConsolePresenter();
                return;
            }

            _dialogueEngineBootstrap.DialoguePresenter = _bubbleDialoguePresenter;
            _dialogueEngineBootstrap.CharacterResolver = _characterRegistry;
            _dialogueEngineBootstrap.ConditionResolver = new DemoConditionResolver();

            Debug.Log("[LSDE Demo] Using visual presenter (speech bubbles).");
        }

        /// <summary>
        /// Wire the console presenter: Debug.Log output, auto-advance (Phase 1 behavior).
        /// </summary>
        private void WireConsolePresenter()
        {
            _dialogueEngineBootstrap.DialoguePresenter = new ConsoleDialoguePresenter();
            _dialogueEngineBootstrap.CharacterResolver = new DemoCharacterResolver();
            _dialogueEngineBootstrap.ConditionResolver = new DemoConditionResolver();

            Debug.Log("[LSDE Demo] Using console presenter (Debug.Log).");
        }

        /// <summary>
        /// Launch a dialogue scene by its UUID. Can be called externally by triggers
        /// (e.g. <see cref="DialogueProximityTrigger"/>) or internally via auto-launch.
        /// </summary>
        /// <param name="sceneUuid">The UUID of the LSDE scene to launch (use LSDE_SCENES constants).</param>
        public void LaunchDialogueScene(string sceneUuid)
        {
            if (IsDialogueSceneActive)
            {
                Debug.LogWarning(
                    "[LSDE Demo] A dialogue scene is already active — ignoring launch request."
                );
                return;
            }

            Debug.Log($"[LSDE Demo] Launching scene: {sceneUuid}");
            IsDialogueSceneActive = true;

            var sceneHandle = _dialogueEngineBootstrap.Engine.Scene(sceneUuid);

            // Register exit callback to log visited blocks and choice history.
            // IMPORTANT: sceneHandle.OnExit() OVERRIDES the global OnSceneExit handler
            // (Tier 2 replaces Tier 1), so we must call PresentSceneExit() here ourselves.
            sceneHandle.OnExit(arguments =>
            {
                IsDialogueSceneActive = false;
                _dialogueEngineBootstrap.DialoguePresenter.PresentSceneExit();
                LogSceneCompletionSummary(sceneHandle);
            });

            sceneHandle.Start();

            Debug.Log($"[LSDE Demo] Engine running: {_dialogueEngineBootstrap.Engine.IsRunning()}");
        }

        private void LogSceneCompletionSummary(ISceneHandle sceneHandle)
        {
            var visitedBlockUuids = sceneHandle.GetVisitedBlocks();
            var visitedBlockLabels = ResolveBlockLabels(visitedBlockUuids);
            var choiceHistory = ConvertChoiceHistory(sceneHandle.GetChoiceHistory());

            _dialogueEngineBootstrap.DialoguePresenter.PresentSceneComplete(
                visitedBlockLabels,
                choiceHistory
            );
        }

        /// <summary>
        /// Map visited block UUIDs to their human-readable labels using the blueprint data.
        /// Falls back to the first 8 characters of the UUID if no label is found.
        /// </summary>
        private List<string> ResolveBlockLabels(IEnumerable<string> blockUuids)
        {
            var blueprintData = _dialogueEngineBootstrap.BlueprintData;
            var resolvedLabels = new List<string>();

            foreach (var blockUuid in blockUuids)
            {
                string resolvedLabel = null;

                foreach (var scene in blueprintData.Scenes)
                {
                    var matchingBlock = scene.Blocks.FirstOrDefault(block =>
                        block.Uuid == blockUuid
                    );

                    if (matchingBlock != null)
                    {
                        resolvedLabel = matchingBlock.Label ?? blockUuid.Substring(0, 8);
                        break;
                    }
                }

                resolvedLabels.Add(resolvedLabel ?? blockUuid.Substring(0, 8));
            }

            return resolvedLabels;
        }

        /// <summary>
        /// Convert the engine's choice history dictionary to a read-only format
        /// compatible with <see cref="IDialoguePresenter.PresentSceneComplete"/>.
        /// </summary>
        private static IReadOnlyDictionary<string, IReadOnlyList<string>> ConvertChoiceHistory(
            IReadOnlyDictionary<string, IReadOnlyList<string>> engineChoiceHistory
        )
        {
            var convertedHistory = new Dictionary<string, IReadOnlyList<string>>();

            foreach (var entry in engineChoiceHistory)
            {
                convertedHistory[entry.Key] = entry.Value;
            }

            return convertedHistory;
        }
    }
}
