using System;
using System.Collections.Generic;
using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Abstraction layer between LSDEDE runtime handlers and the game's rendering system.
    /// Handlers delegate all presentation and flow control to this interface — they never
    /// render or call Next() directly. The presenter decides when to advance the dialogue
    /// (immediately for console mode, on player click or timeout for visual mode).
    /// This pattern matches the official LSDEDE handler design where Next is deferred.
    ///
    /// <para><b>Presentation keys.</b> Every call carries a <c>presentationKey</c> instead of a
    /// block id. A block can be on screen MORE THAN ONCE at the same time: with
    /// <c>inPortPerCharacter</c>, several wires reach one block and each stands for a different
    /// actor, so the engine dispatches it once per wire, in parallel. The key identifies one
    /// presentation — one bubble — and is the same string in
    /// <see cref="PresentDialogueBlock"/> and in the matching <see cref="PresentBlockCleanup"/>.
    /// Build it with <see cref="LsdePresentationKey.For"/>; never key visuals by block id alone.</para>
    /// </summary>
    public interface IDialoguePresenter
    {
        /// <summary>
        /// Present a DIALOG block — display character dialogue text.
        /// The presenter is responsible for calling <paramref name="advanceToNextBlock"/>
        /// when it is ready to advance.
        ///
        /// <para>Three native properties say WHEN to advance, and the presenter owns all three —
        /// the engine enforces none of them. Read them with
        /// <c>LsdeUtils.GetNativeProperties( block )</c>:</para>
        /// <list type="bullet">
        ///   <item><c>Timeout</c> — MILLISECONDS the block STAYS once its line has been SAID.
        ///     The countdown is armed at the END of the reveal, never on arrival, or a line
        ///     slower to type than the timeout allows gets cut mid-sentence. It outranks
        ///     <c>WaitInput</c> and outranks leaving at once, so a click may only HURRY the
        ///     reveal, never dismiss the bubble.</item>
        ///   <item><c>WaitInput</c> — wait for the player instead of leaving on its own.</item>
        ///   <item><c>IsAsync</c> — read by the ENGINE on the wire, not here. It only tells the
        ///     presenter that this bubble coexists with others.</item>
        /// </list>
        /// </summary>
        /// <param name="presentationKey">
        /// Identifies this one presentation of the block. Use it as the key of any per-bubble
        /// state; the same value comes back in <see cref="PresentBlockCleanup"/>.
        /// </param>
        /// <param name="block">The dialog block being executed.</param>
        /// <param name="resolvedCharacter">
        /// The actor the game picked through <c>OnResolveCharacter</c>, or null when nobody could
        /// carry the line. With <c>inPortPerCharacter</c> the engine offered only the actor the
        /// wire named, so this is the speaker that path stands for.
        /// </param>
        /// <param name="localizedText">The dialogue text in the current locale.</param>
        /// <param name="advanceToNextBlock">Callback to advance the engine. Must be called exactly once.</param>
        void PresentDialogueBlock(
            string presentationKey,
            BlueprintBlock block,
            Card resolvedCharacter,
            string localizedText,
            Action advanceToNextBlock
        );

        /// <summary>
        /// Present a CHOICE block — display the answers for the player.
        /// The presenter calls <paramref name="selectChoiceAndAdvance"/> with the chosen option id
        /// when the player decides. That id IS the exit port the flow leaves by.
        /// </summary>
        /// <param name="presentationKey">Identifies this one presentation of the block.</param>
        /// <param name="block">The choice block being executed.</param>
        /// <param name="resolvedCharacter">The actor the game picked, or null.</param>
        /// <param name="cast">
        /// Every card the block cites, resolved. Used as a fallback when no single actor was
        /// picked — a choice still has to appear somewhere.
        /// </param>
        /// <param name="offeredOptions">
        /// The options to show: those whose <c>Visible</c> is not false. The engine hands over ALL
        /// of them tagged, so keeping the hidden ones to grey them out is equally valid.
        /// </param>
        /// <param name="selectChoiceAndAdvance">
        /// Callback that selects an option BY ITS ID and advances the engine. Exactly once.
        /// </param>
        void PresentChoiceBlock(
            string presentationKey,
            BlueprintBlock block,
            Card resolvedCharacter,
            IReadOnlyList<Card> cast,
            IReadOnlyList<RuntimeChoiceItem> offeredOptions,
            Action<string> selectChoiceAndAdvance
        );

        /// <summary>
        /// Present a CONDITION block — invisible routing, useful to log.
        /// The engine has already picked the exit port from these pre-evaluated cases; the handler
        /// is a logging or override hook, nothing more.
        /// </summary>
        /// <param name="block">The condition block being executed.</param>
        /// <param name="cases">The block's cases, each with its port and its pre-evaluated Result.</param>
        void PresentConditionBlock(BlueprintBlock block, IReadOnlyList<RuntimeConditionCase> cases);

        /// <summary>
        /// Present a ROUTER block — the sixth block type, and the one with NO handler of its own.
        ///
        /// <para>A router carries the same <c>cases</c> as a condition and reads them the opposite
        /// way: EVERY case is evaluated, each true one launches its own port, and the flow then
        /// always continues — by <c>then</c> when they all held, by <c>catch</c> when any did not.
        /// All of that happens before a handler could speak, which is why the engine requires
        /// none. A game that wants to WATCH one goes through <c>handle.OnBlock( id )</c> — see
        /// <see cref="RouterBlockObserver"/>. There is nothing to resolve and nothing to override.</para>
        /// </summary>
        /// <param name="block">The router block being traversed.</param>
        /// <param name="cases">Every case, with its port and its pre-evaluated Result. ALL of them ran.</param>
        /// <param name="launchedPorts">The ports the router is launching, continuation last.</param>
        void PresentRouterBlock(
            BlueprintBlock block,
            IReadOnlyList<RuntimeConditionCase> cases,
            IReadOnlyList<string> launchedPorts
        );

        /// <summary>
        /// Present an ACTION block — execute the game calls and control flow via callbacks.
        /// The presenter runs every call (in parallel), then calls
        /// <paramref name="resolveAndAdvance"/> on success or <paramref name="rejectAndAdvance"/>
        /// on failure.
        /// </summary>
        /// <param name="presentationKey">Identifies this one presentation of the block.</param>
        /// <param name="block">The action block being executed.</param>
        /// <param name="calls">
        /// What the block asks the game to run, in order, with their arguments BY NAME.
        /// An argument the writer left empty is simply absent from the bag.
        /// </param>
        /// <param name="resolveAndAdvance">Success: the flow leaves by "then". Exactly once.</param>
        /// <param name="rejectAndAdvance">
        /// Failure: the flow leaves by "catch", or by "then" when no error branch was drawn.
        /// The error is optional and the engine does nothing with it.
        /// </param>
        void PresentActionBlock(
            string presentationKey,
            BlueprintBlock block,
            IReadOnlyList<ActionCall> calls,
            Action resolveAndAdvance,
            Action<object> rejectAndAdvance
        );

        /// <summary>Called when a scene starts executing.</summary>
        /// <param name="sceneHandle">The scene handle that just started.</param>
        void PresentSceneEnter(ISceneHandle sceneHandle);

        /// <summary>Called when a scene finishes executing.</summary>
        void PresentSceneExit();

        /// <summary>
        /// Called before each block is executed (from OnBeforeBlock).
        /// </summary>
        /// <param name="block">The block about to be executed.</param>
        /// <param name="nativeProperties">
        /// The block's native properties, already read out of <c>block.Props</c> by the engine.
        /// </param>
        void PresentBeforeBlock(BlueprintBlock block, NativeProperties nativeProperties);

        /// <summary>
        /// Called when a block's cleanup function fires — the engine has LEFT the block, which is
        /// also what marks it finished for a <c>waitForBlocks</c> elsewhere.
        /// </summary>
        /// <param name="presentationKey">The key given to the matching Present* call.</param>
        /// <param name="block">The block being cleaned up.</param>
        void PresentBlockCleanup(string presentationKey, BlueprintBlock block);

        /// <summary>
        /// Called when a scene completes — display visited blocks and choice history.
        /// </summary>
        /// <param name="visitedBlockLabels">Labels of the blocks the flow reached.</param>
        /// <param name="choiceHistory">Map of CHOICE block id to the option ids the player picked.</param>
        void PresentSceneComplete(
            IReadOnlyList<string> visitedBlockLabels,
            IReadOnlyDictionary<string, IReadOnlyList<string>> choiceHistory
        );
    }
}
