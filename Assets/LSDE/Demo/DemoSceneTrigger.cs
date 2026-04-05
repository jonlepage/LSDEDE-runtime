using System.Collections.Generic;
using System.Linq;
using LSDE.Runtime;
using LsdeDialogEngine;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Demo trigger that launches <see cref="LSDE_SCENES.simpleDialogFlow"/> on scene start.
    /// Creates and injects all demo dependencies into the bootstrap, then starts the dialogue flow.
    ///
    /// Phase 1: All handlers call Next() immediately, so the entire 8-block flow
    /// completes synchronously within a single frame. The console output confirms
    /// the runtime works end-to-end.
    ///
    /// Setup in Unity Editor:
    /// 1. Create an empty GameObject named "DemoSceneTrigger"
    /// 2. Attach this script
    /// 3. Drag the DialogueEngineBootstrap GameObject onto the "Dialogue Engine Bootstrap" field
    /// </summary>
    public class DemoSceneTrigger : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Reference to the DialogueEngineBootstrap component in the scene.")]
        private DialogueEngineBootstrap _dialogueEngineBootstrap;

        /// <summary>
        /// Unity calls Start() once when the GameObject becomes active.
        /// This is where we wire dependencies, initialize the engine, and launch the demo scene.
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

            // Wire demo implementations into the bootstrap
            _dialogueEngineBootstrap.DialoguePresenter = new ConsoleDialoguePresenter();
            _dialogueEngineBootstrap.CharacterResolver = new DemoCharacterResolver();
            _dialogueEngineBootstrap.ConditionResolver = new DemoConditionResolver();

            // Initialize the engine (parse blueprint, register handlers, etc.)
            _dialogueEngineBootstrap.InitializeEngine();

            // Abort if the blueprint has errors
            if (_dialogueEngineBootstrap.LastDiagnosticReport.Errors.Count > 0)
            {
                Debug.LogError("[LSDE Demo] Blueprint has errors — aborting scene launch.");
                return;
            }

            LaunchSimpleDialogFlowScene();
        }

        private void LaunchSimpleDialogFlowScene()
        {
            Debug.Log("[LSDE Demo] Launching scene: simpleDialogFlow");

            var sceneHandle = _dialogueEngineBootstrap.Engine.Scene(LSDE_SCENES.simpleDialogFlow);

            // Register exit callback to log visited blocks and choice history.
            // IMPORTANT: sceneHandle.OnExit() OVERRIDES the global OnSceneExit handler
            // (Tier 2 replaces Tier 1), so we must call PresentSceneExit() here ourselves.
            sceneHandle.OnExit(arguments =>
            {
                _dialogueEngineBootstrap.DialoguePresenter.PresentSceneExit();
                LogSceneCompletionSummary(sceneHandle);
            });

            // Start the dialogue flow — in Phase 1, this completes synchronously
            // because all handlers call Next() immediately.
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
