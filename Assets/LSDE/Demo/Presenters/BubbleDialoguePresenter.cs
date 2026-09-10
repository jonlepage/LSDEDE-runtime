using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LSDE.Runtime;
using LsdeDialogEngine;
using UnityEngine;
using UnityEngine.Serialization;

namespace LSDE.Demo
{
    /// <summary>
    /// Visual implementation of <see cref="IDialoguePresenter"/> that displays speech bubbles
    /// above characters in the 3D scene. Several bubbles can be on screen at once, which is what
    /// parallel tracks look like.
    ///
    /// <para><b>The presenter owns the timing.</b> The engine has no timers and no game loop: it
    /// only advances when <c>Next()</c> is called. Three native properties say WHEN that should
    /// happen, and this class implements all three:</para>
    ///
    /// <list type="bullet">
    ///   <item><c>timeout</c> — the milliseconds the block STAYS <b>once its line has been
    ///     said</b>. The countdown is armed when the typewriter finishes, never when the block
    ///     arrives: a 2500 ms timeout on a 120-character line would otherwise cut it
    ///     mid-sentence. It outranks <c>waitInput</c> and outranks leaving at once, so while a
    ///     timeout is running a click may only HURRY the reveal — it can never dismiss the
    ///     bubble.</item>
    ///   <item><c>waitInput</c> — wait for the player. Used when no timeout is set.</item>
    ///   <item><c>skipIfMissingActor</c> — when nobody can carry the line, walk THROUGH the block
    ///     instead of drawing it. Walking through is what marks it finished, which is what
    ///     releases a <c>waitForBlocks</c> that names it; refusing it in the validation gate would
    ///     hang that other branch for good.</item>
    /// </list>
    ///
    /// <para><c>isAsync</c> is read here for ONE thing only: a line playing on a parallel track
    /// must not stop the player, so it does not wait for a click unless <c>waitInput</c> says so.
    /// Everything else about it belongs to the engine, which reads it on the WIRE to decide
    /// whether the target opens its own track — it says nothing about how long a bubble stays.
    /// Same reading as the reference demo (<c>src/demos/advance-full-demo/index.ts</c>).</para>
    /// </summary>
    public class BubbleDialoguePresenter : MonoBehaviour, IDialoguePresenter
    {
        [SerializeField]
        [Tooltip("The character registry that maps LSDE card names to scene GameObjects.")]
        private DialogueCharacterRegistry _characterRegistry;

        [SerializeField]
        [Tooltip("The click advancer that stores the pending Next callbacks.")]
        private DialogueClickAdvancer _dialogueClickAdvancer;

        [SerializeField]
        [Tooltip(
            "The action executor that maps function ids to game effects "
                + "(camera shake, movement, etc.)."
        )]
        private DemoActionExecutor _actionExecutor;

        [FormerlySerializedAs("_fallbackChoiceCharacterId")]
        [SerializeField]
        [Tooltip(
            "Card NAME the bubble is anchored on when a block cites nobody — a CHOICE, which "
                + "belongs to the player, or a DIALOG with no cast. The reference demo uses l4 as "
                + "this narrator anchor. Leave empty to auto-select the first option instead."
        )]
        private string _fallbackChoiceCharacterName = "l4";

        private const string LogPrefix = "[LSDE]";

        /// <summary>
        /// Conversion factor from blueprint durations (MILLISECONDS in v2) to seconds.
        /// </summary>
        private const double MillisecondsToSeconds = 1000.0;

        /// <summary>
        /// All currently visible speech bubbles, keyed by PRESENTATION key — not by block id.
        /// With <c>inPortPerCharacter</c> the same block is dispatched once per incoming wire, in
        /// parallel, each pass standing for a different actor: three bubbles, one block id.
        /// </summary>
        private readonly Dictionary<string, SpeechBubbleController> _activeBubblesByKey =
            new Dictionary<string, SpeechBubbleController>();

        /// <summary>
        /// Running timeout coroutines, keyed by presentation key. Stored so they can be cancelled
        /// when the block is cleaned up before the countdown fires (scene exit, cancel).
        /// </summary>
        private readonly Dictionary<string, Coroutine> _activeTimeoutCoroutinesByKey =
            new Dictionary<string, Coroutine>();

        /// <summary>
        /// Warm up all speech bubbles at startup so TextMeshPro initializes
        /// its font atlas during scene load. This avoids the lag spike that occurs
        /// when the first bubble is shown via SetActive for the first time.
        /// </summary>
        private void Start()
        {
            var allBubbleControllers = FindObjectsByType<SpeechBubbleController>(
                FindObjectsInactive.Include
            );

            foreach (var bubbleController in allBubbleControllers)
            {
                bubbleController.WarmUp();
            }
        }

        /// <inheritdoc />
        public void PresentDialogueBlock(
            string presentationKey,
            BlueprintBlock block,
            Card resolvedCharacter,
            string localizedText,
            Action advanceToNextBlock
        )
        {
            // Do NOT hide the other bubbles — parallel tracks must coexist.
            // Cleanup is per dispatch, in PresentBlockCleanup.

            var nativeProperties = LsdeUtils.GetNativeProperties(block);
            var blockLabel = LsdeUtils.GetBlockLabel(block);

            // The actor the game picked, or the anchor character when the block cites nobody —
            // a line still has to come from somewhere on screen.
            var speakerName =
                resolvedCharacter != null ? resolvedCharacter.Name : _fallbackChoiceCharacterName;

            var characterMarker = _characterRegistry.FindMarkerByCharacterName(speakerName);

            // `skipIfMissingActor`: the game is the only one that knows whether that actor is on
            // stage right now. Walking THROUGH the block is what marks it finished, which is what
            // releases a `waitForBlocks` naming it — refusing it in the validation gate would hang
            // that branch for good.
            if (nativeProperties.SkipIfMissingActor == true && characterMarker == null)
            {
                Debug.Log(
                    $"{LogPrefix} DIALOG  {blockLabel} — '{speakerName}' is not on stage, "
                        + "skipped (skipIfMissingActor)."
                );
                advanceToNextBlock();
                return;
            }

            // Nothing to show. Either the writer deliberately emptied the line — a beat with no
            // dialogue, which is legitimate — or no locale carries it at all. Either way an empty
            // bubble is worse than none, and walking through the block is what marks it finished.
            if (string.IsNullOrWhiteSpace(localizedText))
            {
                Debug.Log(
                    $"{LogPrefix} DIALOG  {blockLabel} — no line to say. Walking through it."
                );
                advanceToNextBlock();
                return;
            }

            // How the line LEAVES. `timeout` comes first because it wins over the other two: all
            // three answer WHEN the block is left, and the one written on the card is the most
            // specific answer.
            //
            //   timeout                   → stay N ms AFTER the reveal, then leave, and refuse a
            //                               click that would close it early
            //   waitInput, or not isAsync → wait for a click
            //   isAsync, no waitInput     → leave on its own, in the frame it arrived
            //
            // `isAsync` appears here for one reason only: a line playing on a parallel track must
            // not stop the player. It is the ENGINE that reads it on the wire to open that track.
            var timeoutMilliseconds = nativeProperties.Timeout;
            bool hasTimeout = timeoutMilliseconds.HasValue && timeoutMilliseconds.Value > 0;
            bool waitsForClick =
                nativeProperties.WaitInput == true || nativeProperties.IsAsync != true;

            var bubbleController =
                characterMarker != null
                    ? characterMarker.BubbleAnchorPoint.GetComponentInChildren<SpeechBubbleController>(
                        true
                    )
                    : null;

            var entryNote =
                nativeProperties.InPortPerCharacter == true
                    ? "  (speaker named by the wire — inPortPerCharacter)"
                    : "";

            Debug.Log(
                $"{LogPrefix} DIALOG  {blockLabel} — {speakerName}: "
                    + $"\"{TruncateText(localizedText, 50)}\"{entryNote}"
            );

            // Armed at the END of the reveal, never on arrival: `timeout` is how long the line
            // STAYS once it has been said. Counting from arrival cuts a line whose text takes
            // longer to type than the timeout allows — 2500 ms on a 120-character line.
            Action armTimeout = hasTimeout
                ? () =>
                    ArmTimeout(
                        presentationKey,
                        blockLabel,
                        advanceToNextBlock,
                        timeoutMilliseconds.Value
                    )
                : (Action)null;

            if (bubbleController != null)
            {
                _activeBubblesByKey[presentationKey] = bubbleController;

                var bounceAnimation = characterMarker.GetComponent<CharacterBounceAnimation>();
                if (bounceAnimation != null)
                {
                    bounceAnimation.PlayBounce();
                }

                // Registered BEFORE the reveal starts, so the very first click can hurry it.
                // With a timeout, that is all a click may ever do.
                if (hasTimeout || !waitsForClick)
                {
                    _dialogueClickAdvancer.SetRevealOnly(presentationKey, bubbleController);
                }
                else
                {
                    _dialogueClickAdvancer.SetPendingAdvance(
                        presentationKey,
                        advanceToNextBlock,
                        bubbleController
                    );
                }

                // Fade-in and typewriter run in parallel; the callback fires when the last
                // character has been revealed — naturally, or because a click hurried it.
                bubbleController.ShowDialogue(speakerName, localizedText, armTimeout);
            }
            else
            {
                Debug.LogWarning(
                    $"{LogPrefix} DIALOG  {blockLabel} — nothing on stage can show this line. "
                        + "It keeps its timing and shows nothing."
                );

                if (waitsForClick && !hasTimeout)
                {
                    _dialogueClickAdvancer.SetPendingAdvance(presentationKey, advanceToNextBlock);
                }

                // No reveal to wait for, so the countdown starts at once.
                if (armTimeout != null)
                {
                    armTimeout();
                }
            }

            if (!hasTimeout && !waitsForClick)
            {
                // Neither timing: the block leaves in the frame it arrived, so the bubble flashes
                // and is gone. That is the drawing, not a fault — a writer who wants the line read
                // gives it a `timeout`.
                advanceToNextBlock();
            }
        }

        /// <summary>
        /// Start the block's <c>timeout</c> countdown. Called when the reveal ends — or at once
        /// when there was no bubble to reveal.
        /// </summary>
        /// <param name="presentationKey">The dispatch this countdown belongs to.</param>
        /// <param name="blockLabel">Readable block label, for logs.</param>
        /// <param name="advanceToNextBlock">The engine's Next callback.</param>
        /// <param name="timeoutMilliseconds">How long the line stays, in milliseconds.</param>
        private void ArmTimeout(
            string presentationKey,
            string blockLabel,
            Action advanceToNextBlock,
            double timeoutMilliseconds
        )
        {
            var timeoutInSeconds = (float)(timeoutMilliseconds / MillisecondsToSeconds);

            Debug.Log(
                $"{LogPrefix}   {blockLabel} — line said, staying {timeoutInSeconds}s "
                    + "(timeout armed at the END of the reveal)"
            );

            _activeTimeoutCoroutinesByKey[presentationKey] = StartCoroutine(
                LeaveAfterTimeoutCoroutine(presentationKey, advanceToNextBlock, timeoutInSeconds)
            );
        }

        /// <summary>
        /// Wait out the block's <c>timeout</c>, then leave. Started when the reveal ends.
        /// </summary>
        private IEnumerator LeaveAfterTimeoutCoroutine(
            string presentationKey,
            Action advanceToNextBlock,
            float timeoutInSeconds
        )
        {
            yield return new WaitForSeconds(timeoutInSeconds);

            _activeTimeoutCoroutinesByKey.Remove(presentationKey);
            _dialogueClickAdvancer.ClearPendingAdvanceForBlock(presentationKey);

            advanceToNextBlock();
        }

        /// <inheritdoc />
        public void PresentChoiceBlock(
            string presentationKey,
            BlueprintBlock block,
            Card resolvedCharacter,
            IReadOnlyList<Card> cast,
            IReadOnlyList<RuntimeChoiceItem> offeredOptions,
            Action<string> selectChoiceAndAdvance
        )
        {
            // Clear the click advancer — options have their own buttons, not click-anywhere.
            _dialogueClickAdvancer.ClearAllPendingAdvances();

            var blockLabel = LsdeUtils.GetBlockLabel(block);

            if (offeredOptions.Count == 0)
            {
                Debug.LogWarning(
                    $"{LogPrefix} CHOICE  {blockLabel} — nothing to offer. The flow stops here."
                );
                return;
            }

            // Who hosts the bubble: the actor the game picked, else the first of the cast, else
            // the configured fallback. A choice block often follows an ACTION and cites nobody.
            var choiceCharacter =
                resolvedCharacter ?? (cast != null ? cast.FirstOrDefault() : null);

            DialogueCharacterMarker characterMarker = null;

            if (choiceCharacter != null)
            {
                characterMarker = _characterRegistry.FindMarkerByCharacterName(
                    choiceCharacter.Name
                );
            }

            if (characterMarker == null && !string.IsNullOrEmpty(_fallbackChoiceCharacterName))
            {
                characterMarker = _characterRegistry.FindMarkerByCharacterName(
                    _fallbackChoiceCharacterName
                );

                if (characterMarker != null)
                {
                    Debug.Log(
                        $"{LogPrefix} CHOICE  {blockLabel} — hosted by the fallback character "
                            + $"'{_fallbackChoiceCharacterName}'."
                    );
                }
            }

            if (characterMarker == null)
            {
                Debug.LogWarning(
                    $"{LogPrefix} CHOICE  {blockLabel} — nobody to host the options. "
                        + "Auto-selecting the first one."
                );
                selectChoiceAndAdvance(offeredOptions[0].Id);
                return;
            }

            var bubbleController =
                characterMarker.BubbleAnchorPoint.GetComponentInChildren<SpeechBubbleController>(
                    true
                );

            if (bubbleController == null)
            {
                Debug.LogWarning(
                    $"{LogPrefix} CHOICE  {blockLabel} — no SpeechBubbleController on "
                        + $"'{characterMarker.LsdeCharacterName}'. Auto-selecting the first option."
                );
                selectChoiceAndAdvance(offeredOptions[0].Id);
                return;
            }

            var speakerName =
                choiceCharacter != null ? choiceCharacter.Name : characterMarker.LsdeCharacterName;

            _activeBubblesByKey[presentationKey] = bubbleController;

            // The option id IS the exit port, so it is what the button carries.
            var optionDisplayItems = new List<(string uuid, string localizedText)>();
            foreach (var option in offeredOptions)
            {
                var optionText = LsdeText.Localized(option.Text);
                optionDisplayItems.Add(
                    (option.Id, string.IsNullOrEmpty(optionText) ? option.Id : optionText)
                );
            }

            bubbleController.ShowChoices(speakerName, optionDisplayItems, selectChoiceAndAdvance);

            Debug.Log(
                $"{LogPrefix} CHOICE  {blockLabel} — {speakerName}: "
                    + $"{offeredOptions.Count} option(s) offered"
            );
        }

        /// <inheritdoc />
        public void PresentConditionBlock(
            BlueprintBlock block,
            IReadOnlyList<RuntimeConditionCase> cases
        )
        {
            // A condition is invisible routing — the engine already picked the port from these
            // pre-evaluated cases. Log only.
            var caseReport = string.Join(
                " ",
                cases.Select(conditionCase => $"{conditionCase.Port}={conditionCase.Result}")
            );

            Debug.Log($"{LogPrefix} CONDITION  {LsdeUtils.GetBlockLabel(block)} — {caseReport}");
        }

        /// <inheritdoc />
        public void PresentRouterBlock(
            BlueprintBlock block,
            IReadOnlyList<RuntimeConditionCase> cases,
            IReadOnlyList<string> launchedPorts
        )
        {
            // A tally, not a choice: every case ran, each true one launches its port, and the
            // continuation (`then` if they all held, `catch` otherwise) comes LAST.
            var caseReport = string.Join(
                " ",
                cases.Select(conditionCase => $"{conditionCase.Port}={conditionCase.Result}")
            );

            Debug.Log(
                $"{LogPrefix} ROUTER  {LsdeUtils.GetBlockLabel(block)} — cases: {caseReport}\n"
                    + $"{LogPrefix}   launching: {string.Join(" → ", launchedPorts)}"
            );
        }

        /// <inheritdoc />
        public void PresentActionBlock(
            string presentationKey,
            BlueprintBlock block,
            IReadOnlyList<ActionCall> calls,
            Action resolveAndAdvance,
            Action<object> rejectAndAdvance
        )
        {
            var callCount = calls?.Count ?? 0;

            Debug.Log(
                $"{LogPrefix} ACTION  {LsdeUtils.GetBlockLabel(block)} — {callCount} call(s)"
            );

            if (calls == null || callCount == 0 || _actionExecutor == null)
            {
                if (_actionExecutor == null && callCount > 0)
                {
                    Debug.LogWarning(
                        $"{LogPrefix} No IActionExecutor assigned on BubbleDialoguePresenter — "
                            + "resolving the action block without running anything."
                    );
                }
                resolveAndAdvance();
                return;
            }

            // Run every call in parallel (the Unity equivalent of Promise.all), then resolve or
            // reject once they are all done.
            StartCoroutine(
                ExecuteAllCallsInParallelCoroutine(calls, resolveAndAdvance, rejectAndAdvance)
            );
        }

        /// <summary>
        /// Execute all calls of an ACTION block in parallel and wait for all to complete.
        /// A shared counter tracks progress; the first error wins and routes to <c>catch</c>.
        /// </summary>
        private IEnumerator ExecuteAllCallsInParallelCoroutine(
            IReadOnlyList<ActionCall> calls,
            Action resolveAndAdvance,
            Action<object> rejectAndAdvance
        )
        {
            int totalCallCount = calls.Count;
            int completedCallCount = 0;
            bool hasAnyCallFailed = false;
            object firstEncounteredError = null;

            foreach (var call in calls)
            {
                StartCoroutine(
                    ExecuteSingleCallWithCompletionTracking(
                        call,
                        onCallCompleted: () =>
                        {
                            completedCallCount++;
                        },
                        onCallFailed: error =>
                        {
                            if (!hasAnyCallFailed)
                            {
                                hasAnyCallFailed = true;
                                firstEncounteredError = error;
                            }
                            completedCallCount++;
                        }
                    )
                );
            }

            while (completedCallCount < totalCallCount)
            {
                yield return null;
            }

            if (hasAnyCallFailed)
            {
                Debug.LogError($"{LogPrefix} Action block failed: {firstEncounteredError}");
                rejectAndAdvance(firstEncounteredError);
            }
            else
            {
                resolveAndAdvance();
            }
        }

        /// <summary>
        /// Execute a single call via <see cref="_actionExecutor"/> and report completion or
        /// failure through callbacks.
        ///
        /// Unity does not allow <c>try/catch</c> around <c>yield return</c>, so this method
        /// manually advances the <see cref="IEnumerator"/> with <c>MoveNext()</c> inside a
        /// <c>try/catch</c> — the standard pattern for exception-safe coroutine delegation.
        /// It keeps one failing call from taking down the whole parallel batch.
        /// </summary>
        private IEnumerator ExecuteSingleCallWithCompletionTracking(
            ActionCall call,
            Action onCallCompleted,
            Action<object> onCallFailed
        )
        {
            IEnumerator callCoroutine;

            try
            {
                callCoroutine = _actionExecutor.ExecuteAction(call);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"{LogPrefix} Call '{call.Fn}' threw during setup: " + exception.Message
                );
                onCallFailed(exception);
                yield break;
            }

            while (true)
            {
                bool hasMoreSteps;
                try
                {
                    hasMoreSteps = callCoroutine.MoveNext();
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"{LogPrefix} Call '{call.Fn}' threw during execution: " + exception.Message
                    );
                    onCallFailed(exception);
                    yield break;
                }

                if (!hasMoreSteps)
                {
                    break;
                }

                yield return callCoroutine.Current;
            }

            onCallCompleted();
        }

        /// <inheritdoc />
        public void PresentSceneEnter(ISceneHandle sceneHandle)
        {
            Debug.Log($"{LogPrefix} === Scene Enter === running={sceneHandle.IsRunning()}");
        }

        /// <inheritdoc />
        public void PresentSceneExit()
        {
            HideAllActiveBubbles();
            CancelAllActiveTimeoutCoroutines();
            _dialogueClickAdvancer.ClearAllPendingAdvances();

            // Reset camera to normal follow mode (clears shake, resumes follow) — a scene may have
            // left it parked on a non-player character.
            if (_actionExecutor != null)
            {
                _actionExecutor.ResetCameraState();
            }

            Debug.Log($"{LogPrefix} === Scene Exit ===");
        }

        /// <inheritdoc />
        public void PresentBeforeBlock(BlueprintBlock block, NativeProperties nativeProperties)
        {
            // `debug` is the writer asking to see this block in the log while they work on it.
            if (nativeProperties != null && nativeProperties.Debug == true)
            {
                Debug.Log(
                    $"{LogPrefix}   debug: entering {LsdeUtils.GetBlockLabel(block)} "
                        + $"({block.Type})"
                );
            }
        }

        /// <inheritdoc />
        public void PresentBlockCleanup(string presentationKey, BlueprintBlock block)
        {
            // Hide only THIS presentation's bubble. The other tracks' bubbles stay untouched.
            if (_activeBubblesByKey.TryGetValue(presentationKey, out var bubbleController))
            {
                _activeBubblesByKey.Remove(presentationKey);

                // A character owns one bubble, and two parallel tracks can both be speaking
                // through the same character — multi-tracks does exactly that with l3. Hiding on
                // the first cleanup would take the other track's line off screen with it, so the
                // bubble only closes once nothing else is using it.
                if (
                    bubbleController != null
                    && bubbleController.IsVisible
                    && !IsBubbleStillInUse(bubbleController)
                )
                {
                    bubbleController.HideDialogue();
                }
            }

            // Cancel a still-running timeout, so it cannot advance a block twice.
            if (
                _activeTimeoutCoroutinesByKey.TryGetValue(presentationKey, out var timeoutCoroutine)
            )
            {
                if (timeoutCoroutine != null)
                {
                    StopCoroutine(timeoutCoroutine);
                }
                _activeTimeoutCoroutinesByKey.Remove(presentationKey);
            }

            _dialogueClickAdvancer.ClearPendingAdvanceForBlock(presentationKey);
        }

        /// <inheritdoc />
        public void PresentSceneComplete(
            IReadOnlyList<string> visitedBlockLabels,
            IReadOnlyDictionary<string, IReadOnlyList<string>> choiceHistory
        )
        {
            Debug.Log($"{LogPrefix} Visited: {string.Join(", ", visitedBlockLabels)}");
        }

        /// <summary>
        /// Whether another live presentation is still showing through this bubble.
        /// </summary>
        /// <param name="bubbleController">The bubble about to be hidden.</param>
        private bool IsBubbleStillInUse(SpeechBubbleController bubbleController)
        {
            foreach (var activeBubble in _activeBubblesByKey.Values)
            {
                if (ReferenceEquals(activeBubble, bubbleController))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Hide every visible bubble and clear the tracking dictionary.
        /// </summary>
        private void HideAllActiveBubbles()
        {
            foreach (var bubbleController in _activeBubblesByKey.Values)
            {
                if (bubbleController != null && bubbleController.IsVisible)
                {
                    bubbleController.HideDialogue();
                }
            }
            _activeBubblesByKey.Clear();
        }

        /// <summary>
        /// Cancel every running timeout coroutine and clear the tracking dictionary.
        /// </summary>
        private void CancelAllActiveTimeoutCoroutines()
        {
            foreach (var timeoutCoroutine in _activeTimeoutCoroutinesByKey.Values)
            {
                if (timeoutCoroutine != null)
                {
                    StopCoroutine(timeoutCoroutine);
                }
            }
            _activeTimeoutCoroutinesByKey.Clear();
        }

        private static string TruncateText(string text, int maximumLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maximumLength)
            {
                return text ?? "";
            }
            return text.Substring(0, maximumLength) + "...";
        }
    }
}
