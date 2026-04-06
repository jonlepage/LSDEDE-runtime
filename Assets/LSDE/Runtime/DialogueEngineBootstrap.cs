using System;
using System.Collections;
using LsdeDialogEngine;
using UnityEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Central orchestrator that creates the LSDEDE engine, loads blueprint data,
    /// and wires all handlers and resolvers. This MonoBehaviour must be initialized
    /// exactly ONCE per game session — attach it to a single GameObject in your scene.
    ///
    /// Dependencies (IDialoguePresenter, ICharacterResolver, IConditionResolver) must be
    /// set before calling <see cref="InitializeEngine"/>. The demo trigger handles this wiring.
    /// </summary>
    public class DialogueEngineBootstrap : MonoBehaviour
    {
        [SerializeField]
        [Tooltip(
            "Drag blueprint.json here from Assets/LSDE/blueprints/. "
                + "This TextAsset is parsed into the engine's BlueprintExport data structure."
        )]
        private TextAsset _blueprintTextAsset;

        private DialogueEngine _dialogueEngine;
        private BlueprintExport _blueprintExport;
        private DiagnosticReport _lastDiagnosticReport;

        /// <summary>
        /// The initialized LSDEDE engine instance. Null until <see cref="InitializeEngine"/> is called.
        /// </summary>
        public DialogueEngine Engine => _dialogueEngine;

        /// <summary>
        /// The parsed blueprint data. Useful for UUID-to-label mapping when logging visited blocks.
        /// </summary>
        public BlueprintExport BlueprintData => _blueprintExport;

        /// <summary>
        /// The diagnostic report from the last <see cref="InitializeEngine"/> call.
        /// Contains errors, warnings, and statistics about the loaded blueprint.
        /// </summary>
        public DiagnosticReport LastDiagnosticReport => _lastDiagnosticReport;

        /// <summary>
        /// The presenter that handles all visual/audio output from dialogue blocks.
        /// Must be set before calling <see cref="InitializeEngine"/>.
        /// </summary>
        public IDialoguePresenter DialoguePresenter { get; set; }

        /// <summary>
        /// The resolver that determines which character is authorized at runtime.
        /// Must be set before calling <see cref="InitializeEngine"/>.
        /// </summary>
        public ICharacterResolver CharacterResolver { get; set; }

        /// <summary>
        /// The resolver that evaluates game-state conditions (inventory, flags, variables).
        /// Must be set before calling <see cref="InitializeEngine"/>.
        /// </summary>
        public IConditionResolver ConditionResolver { get; set; }

        /// <summary>
        /// Initialize the LSDEDE engine with the assigned blueprint and dependencies.
        /// Call this exactly ONCE after setting all dependencies (presenter, resolvers).
        /// Order: parse → init → locale → resolvers → handlers.
        /// </summary>
        public void InitializeEngine()
        {
            ValidateDependencies();

            // Step 1: Parse blueprint JSON via the safe polymorphic deserializer
            _blueprintExport = BlueprintLoader.Parse(_blueprintTextAsset);

            // Step 2: Create and initialize the engine
            _dialogueEngine = new DialogueEngine();
            _lastDiagnosticReport = _dialogueEngine.Init(
                new InitOptions { Data = _blueprintExport }
            );

            LogDiagnosticReport(_lastDiagnosticReport);

            if (_lastDiagnosticReport.Errors.Count > 0)
            {
                Debug.LogError("[LSDE] Blueprint has errors — engine may not function correctly.");
                return;
            }

            // Step 3: Set locale for text resolution
            _dialogueEngine.SetLocale("fr");

            // Step 4: Register resolvers (game-state callbacks)
            RegisterResolvers();

            // Step 5: Register the 4 mandatory block handlers
            RegisterBlockHandlers();

            // Step 6: Register optional lifecycle handlers
            RegisterLifecycleHandlers();

            Debug.Log("[LSDE] Engine initialized successfully.");
        }

        private void ValidateDependencies()
        {
            if (_blueprintTextAsset == null)
            {
                throw new InvalidOperationException(
                    "Blueprint TextAsset is not assigned. "
                        + "Drag blueprint.json onto the DialogueEngineBootstrap component in the Inspector."
                );
            }

            if (DialoguePresenter == null)
            {
                throw new InvalidOperationException(
                    "DialoguePresenter is not set. Assign it before calling InitializeEngine()."
                );
            }

            if (CharacterResolver == null)
            {
                throw new InvalidOperationException(
                    "CharacterResolver is not set. Assign it before calling InitializeEngine()."
                );
            }

            if (ConditionResolver == null)
            {
                throw new InvalidOperationException(
                    "ConditionResolver is not set. Assign it before calling InitializeEngine()."
                );
            }
        }

        private void RegisterResolvers()
        {
            _dialogueEngine.OnResolveCharacter(availableCharacters =>
                CharacterResolver.ResolveCharacter(availableCharacters)
            );

            _dialogueEngine.OnResolveCondition(condition =>
                ConditionResolver.EvaluateCondition(condition)
            );
        }

        private void RegisterBlockHandlers()
        {
            var dialogHandler = new DialogBlockHandler(DialoguePresenter);
            var choiceHandler = new ChoiceBlockHandler(DialoguePresenter);
            var conditionHandler = new ConditionBlockHandler(DialoguePresenter);
            var actionHandler = new ActionBlockHandler(DialoguePresenter);

            _dialogueEngine.OnDialog(dialogHandler.HandleDialogBlock);
            _dialogueEngine.OnChoice(choiceHandler.HandleChoiceBlock);
            _dialogueEngine.OnCondition(conditionHandler.HandleConditionBlock);
            _dialogueEngine.OnAction(actionHandler.HandleActionBlock);
        }

        /// <summary>
        /// Conversion factor from blueprint delay values (milliseconds) to seconds.
        /// Blueprint JSON stores durations in milliseconds (e.g. 1000 for 1 second).
        /// Unity's WaitForSeconds expects seconds.
        /// </summary>
        private const double MillisecondsToSeconds = 1000.0;

        private void RegisterLifecycleHandlers()
        {
            // OnBeforeBlock: MUST call Resolve() or the flow blocks permanently.
            // Resolve is a property on BeforeBlockArgs (not on Context).
            // If the block has a delay (NativeProperties.Delay), we wait that duration
            // before resolving — this defers the block's execution as intended by the
            // narrative designer. The engine does NOT enforce delay automatically.
            _dialogueEngine.OnBeforeBlock(arguments =>
            {
                DialoguePresenter.PresentBeforeBlock(arguments.Block);

                var delayMilliseconds = arguments.Block.NativeProperties?.Delay;
                if (delayMilliseconds.HasValue && delayMilliseconds.Value > 0)
                {
                    var delayInSeconds = (float)(delayMilliseconds.Value / MillisecondsToSeconds);
                    StartCoroutine(DelayedResolveCoroutine(delayInSeconds, arguments.Resolve));
                }
                else
                {
                    arguments.Resolve();
                }
            });

            _dialogueEngine.OnSceneEnter(arguments =>
            {
                DialoguePresenter.PresentSceneEnter(arguments.Scene);
            });

            _dialogueEngine.OnSceneExit(arguments =>
            {
                DialoguePresenter.PresentSceneExit();
            });

            _dialogueEngine.OnValidateNextBlock(arguments =>
            {
                return new ValidationResult { Valid = true };
            });

            _dialogueEngine.OnInvalidateBlock(arguments =>
            {
                Debug.LogError($"[LSDE] Block invalidated: {arguments.Reason}");
                arguments.Scene.Cancel();
            });
        }

        /// <summary>
        /// Coroutine that waits for the specified delay then calls the resolve callback.
        /// Used by OnBeforeBlock to defer block execution when a delay is specified
        /// in the block's <see cref="NativeProperties.Delay"/>.
        /// </summary>
        /// <param name="delayInSeconds">How long to wait before resolving.</param>
        /// <param name="resolve">The resolve callback that unblocks the engine flow.</param>
        private IEnumerator DelayedResolveCoroutine(float delayInSeconds, Action resolve)
        {
            yield return new WaitForSeconds(delayInSeconds);
            resolve();
        }

        private static void LogDiagnosticReport(DiagnosticReport report)
        {
            Debug.Log(
                $"[LSDE] Init — {report.Errors.Count} errors, {report.Warnings.Count} warnings"
            );

            foreach (var warning in report.Warnings)
            {
                Debug.LogWarning($"[LSDE]   Warning {warning.Code}: {warning.Message}");
            }

            foreach (var error in report.Errors)
            {
                Debug.LogError($"[LSDE]   Error {error.Code}: {error.Message}");
            }

            Debug.Log(
                $"[LSDE] Stats — "
                    + $"scenes: {report.Stats.SceneCount}, "
                    + $"blocks: {report.Stats.BlockCount}, "
                    + $"connections: {report.Stats.ConnectionCount}"
            );
        }
    }
}
