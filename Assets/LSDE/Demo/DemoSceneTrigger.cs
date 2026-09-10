using System.Collections.Generic;
using System.Linq;
using LSDE.Runtime;
using LsdeDialogEngine;
using UnityEngine;
using UnityEngine.Serialization;

namespace LSDE.Demo
{
    /// <summary>
    /// Demo trigger that launches LSDE dialogue scenes.
    /// Supports two presenter modes toggled via the Inspector:
    /// - Visual mode: speech bubbles above characters, click to advance
    /// - Console mode: Debug.Log output, auto-advance
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

        [Header("Resolvers")]
        [SerializeField]
        [Tooltip(
            "Answers game-state tests for CONDITION, ROUTER and option visibility. "
                + "If not assigned, every test answers false."
        )]
        private DemoConditionResolver _conditionResolver;

        [Header("Visual Presenter")]
        [SerializeField]
        [Tooltip(
            "Enable to use speech bubbles above characters. "
                + "Disable to use console-only output."
        )]
        private bool _useVisualPresenter = true;

        [SerializeField]
        [Tooltip("Registry that maps LSDE card names to scene GameObjects.")]
        private DialogueCharacterRegistry _characterRegistry;

        [SerializeField]
        [Tooltip("Click handler that advances dialogue on mouse click.")]
        private DialogueClickAdvancer _dialogueClickAdvancer;

        [SerializeField]
        [Tooltip("Visual presenter that displays speech bubbles above characters.")]
        private BubbleDialoguePresenter _bubbleDialoguePresenter;

        [Header("Auto-Launch (optional)")]
        [FormerlySerializedAs("_autoLaunchSceneUuid")]
        [SerializeField]
        [LsdeSceneSelector]
        [Tooltip(
            "If set, this scene is launched automatically on Start(). "
                + "Leave on '(none)' to wait for an external trigger."
        )]
        private string _autoLaunchSceneRef;

        /// <summary>
        /// The currently active scene handle. Stored so it can be force-stopped
        /// when switching between demo scenes via <see cref="ForceStopActiveScene"/>.
        /// </summary>
        private ISceneHandle _currentSceneHandle;

        /// <summary>
        /// The blueprint scene currently being played. Kept to resolve block labels: a block is
        /// identified by (scene, id), and ids repeat across scenes — DIALOG-009 exists in three of
        /// them here — so looking a label up in the wrong scene silently gives the wrong name.
        /// </summary>
        private BlueprintScene _currentBlueprintScene;

        /// <summary>
        /// Whether a dialogue scene is currently active (started but not yet exited).
        /// Used by triggers to prevent re-triggering while dialogue is in progress.
        /// </summary>
        public bool IsDialogueSceneActive { get; private set; }

        /// <summary>
        /// Unity calls Start() once when the GameObject becomes active.
        /// Wires dependencies and initializes the engine. Optionally launches a scene
        /// if <see cref="_autoLaunchSceneRef"/> is set.
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

            if (_useVisualPresenter)
            {
                WireVisualPresenter();
            }
            else
            {
                WireConsolePresenter();
            }

            _dialogueEngineBootstrap.InitializeEngine();

            if (_dialogueEngineBootstrap.LastDiagnosticReport.Errors.Count > 0)
            {
                Debug.LogError("[LSDE Demo] Blueprint has errors — aborting scene launch.");
                return;
            }

            if (!string.IsNullOrEmpty(_autoLaunchSceneRef))
            {
                LaunchDialogueScene(_autoLaunchSceneRef);
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
            _dialogueEngineBootstrap.ConditionResolver = ResolveConditionResolver();

            Debug.Log("[LSDE Demo] Using visual presenter (speech bubbles).");
        }

        /// <summary>
        /// Wire the console presenter: Debug.Log output, auto-advance.
        /// </summary>
        private void WireConsolePresenter()
        {
            _dialogueEngineBootstrap.DialoguePresenter = new ConsoleDialoguePresenter();
            _dialogueEngineBootstrap.CharacterResolver = new DemoCharacterResolver();
            _dialogueEngineBootstrap.ConditionResolver = ResolveConditionResolver();

            Debug.Log("[LSDE Demo] Using console presenter (Debug.Log).");
        }

        /// <summary>
        /// Launch a dialogue scene. Can be called externally by triggers
        /// (e.g. <see cref="DialogueProximityTrigger"/>) or internally via auto-launch.
        /// </summary>
        /// <param name="sceneRef">
        /// The scene's PATH (<c>simple-dialog-flow</c>) or its stable id (<c>sc_60ql8la3</c>).
        /// Prefer the id anywhere it is stored outside the payload — a serialized path stops
        /// resolving the day someone renames the scene, with no compiler to catch it. Use the
        /// generated <c>LsdedeDemoTsBlueprintIds.Scenes</c> constants.
        /// </param>
        public void LaunchDialogueScene(string sceneRef)
        {
            if (IsDialogueSceneActive)
            {
                Debug.LogWarning(
                    "[LSDE Demo] A dialogue scene is already active — ignoring launch request."
                );
                return;
            }

            Debug.Log($"[LSDE Demo] Launching scene: {sceneRef}");
            IsDialogueSceneActive = true;

            var sceneHandle = _dialogueEngineBootstrap.Engine.Scene(sceneRef);
            _currentSceneHandle = sceneHandle;
            _currentBlueprintScene = FindBlueprintScene(sceneRef);

            // Watch the ROUTER blocks of this scene. Purely observational — a router has no
            // handler and needs none; this is how a game gets to SEE one fan out.
            if (_currentBlueprintScene != null)
            {
                var routerObserver = new RouterBlockObserver(
                    _dialogueEngineBootstrap.DialoguePresenter
                );
                routerObserver.ObserveRouters(sceneHandle, _currentBlueprintScene.Blocks);
            }

            // Register the exit callback to log visited blocks and choice history.
            // IMPORTANT: handle.OnExit() OVERRIDES the global OnSceneExit (Tier 2 replaces
            // Tier 1), so we must call PresentSceneExit() here ourselves.
            sceneHandle.OnExit(arguments =>
            {
                IsDialogueSceneActive = false;
                _currentSceneHandle = null;
                _dialogueEngineBootstrap.DialoguePresenter.PresentSceneExit();
                LogSceneCompletionSummary(sceneHandle);
                _currentBlueprintScene = null;
            });

            sceneHandle.Start();

            Debug.Log($"[LSDE Demo] Engine running: {_dialogueEngineBootstrap.Engine.IsRunning()}");
        }

        /// <summary>
        /// Force-stop the currently active dialogue scene immediately.
        /// Used by <see cref="WebGlSceneController"/> when switching between demo scenes
        /// from the React sidebar.
        /// </summary>
        public void ForceStopActiveScene()
        {
            if (!IsDialogueSceneActive)
            {
                return;
            }

            Debug.Log("[LSDE Demo] Force-stopping active dialogue scene.");

            // Stop the engine — cancels every live scene, and every track inside them.
            _dialogueEngineBootstrap.Engine.Stop();

            _dialogueEngineBootstrap.DialoguePresenter.PresentSceneExit();

            IsDialogueSceneActive = false;
            _currentSceneHandle = null;
            _currentBlueprintScene = null;
        }

        private void LogSceneCompletionSummary(ISceneHandle sceneHandle)
        {
            var visitedBlockIds = sceneHandle.GetVisitedBlocks();
            var visitedBlockLabels = ResolveBlockLabels(visitedBlockIds);
            var choiceHistory = ConvertChoiceHistory(sceneHandle.GetChoiceHistory());

            _dialogueEngineBootstrap.DialoguePresenter.PresentSceneComplete(
                visitedBlockLabels,
                choiceHistory
            );
        }

        /// <summary>
        /// Find the payload scene a ref designates. The engine accepts a path OR the stable id,
        /// so this accepts both too.
        /// </summary>
        private BlueprintScene FindBlueprintScene(string sceneRef)
        {
            var blueprintData = _dialogueEngineBootstrap.BlueprintData;

            if (blueprintData == null || blueprintData.Scenes == null)
            {
                return null;
            }

            return blueprintData.Scenes.FirstOrDefault(scene =>
                scene.Scene == sceneRef || scene.Id == sceneRef
            );
        }

        /// <summary>
        /// Map visited block ids to their readable labels, WITHIN the scene that was played.
        /// Falls back to the id itself when the block carries no label.
        /// </summary>
        private List<string> ResolveBlockLabels(IEnumerable<string> blockIds)
        {
            var resolvedLabels = new List<string>();

            foreach (var blockId in blockIds)
            {
                var matchingBlock = _currentBlueprintScene?.Blocks.FirstOrDefault(block =>
                    block.Id == blockId
                );

                resolvedLabels.Add(
                    matchingBlock != null ? LsdeUtils.GetBlockLabel(matchingBlock) : blockId
                );
            }

            return resolvedLabels;
        }

        /// <summary>
        /// Get the condition resolver — the serialized reference if available, otherwise a
        /// resolver that answers false to everything.
        /// </summary>
        private IConditionResolver ResolveConditionResolver()
        {
            if (_conditionResolver != null)
            {
                return _conditionResolver;
            }

            Debug.LogWarning(
                "[LSDE Demo] DemoConditionResolver is not assigned. Every test will answer "
                    + "false, so conditions take their default branch and no router case fires. "
                    + "Assign it in the Inspector to read the game state."
            );
            return new AlwaysFalseConditionResolver();
        }

        /// <summary>
        /// Minimal resolver that answers false to every test.
        ///
        /// <para>False, not true: a test nobody can answer must not open a branch. It is also what
        /// the engine itself does for routing — an unanswerable test is false — while option
        /// visibility stays <c>undefined</c> in that case, because saying false about a question
        /// nobody could answer would HIDE an answer.</para>
        /// </summary>
        private class AlwaysFalseConditionResolver : IConditionResolver
        {
            public bool EvaluateCondition(ConditionTest test) => false;
        }

        /// <summary>
        /// Convert the engine's choice history to the read-only shape
        /// <see cref="IDialoguePresenter.PresentSceneComplete"/> expects.
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
