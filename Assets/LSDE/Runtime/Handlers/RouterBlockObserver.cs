using System.Collections.Generic;
using System.Linq;
using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Watches the ROUTER blocks of one scene.
    ///
    /// <para>A router is the only block type with <b>no handler</b>: the engine evaluates every
    /// case, launches the port of each true one, then continues by <c>then</c> (all held) or
    /// <c>catch</c> (any did not) — all before a handler could speak. <c>Start()</c> requires no
    /// router handler and there is nothing to resolve or override, which is why there is no
    /// <c>engine.OnRouter()</c> to register.</para>
    ///
    /// <para>A game that wants to SEE what a router did targets it by id instead, through
    /// <c>handle.OnBlock( id )</c>, and reads the pre-evaluated cases off an
    /// <see cref="IRouterContext"/>. That is what this class does for every router of a scene, so
    /// the demo can log the fan-out. Registering a per-block handler makes it THE handler for that
    /// block, so it must call <c>Next()</c> itself — otherwise the flow would stop there.</para>
    ///
    /// <para>Purely observational: removing this class changes nothing about how the scene plays.</para>
    /// </summary>
    public class RouterBlockObserver
    {
        private readonly IDialoguePresenter _dialoguePresenter;

        /// <summary>
        /// Create an observer that reports through the given presenter.
        /// </summary>
        /// <param name="dialoguePresenter">The presenter that will report each router.</param>
        public RouterBlockObserver(IDialoguePresenter dialoguePresenter)
        {
            _dialoguePresenter = dialoguePresenter;
        }

        /// <summary>
        /// Register a watcher on every ROUTER block of the scene.
        /// Call it after <c>engine.Scene( ref )</c> and before <c>handle.Start()</c>.
        /// </summary>
        /// <param name="sceneHandle">The scene about to be played.</param>
        /// <param name="sceneBlocks">
        /// The blocks of that scene, from the payload. A block is identified by (scene, id): ids
        /// repeat across scenes, so the list must be the one of THIS scene.
        /// </param>
        public void ObserveRouters(
            ISceneHandle sceneHandle,
            IEnumerable<BlueprintBlock> sceneBlocks
        )
        {
            if (sceneHandle == null || sceneBlocks == null || _dialoguePresenter == null)
            {
                return;
            }

            foreach (var block in sceneBlocks.Where(LsdeUtils.IsRouterBlock))
            {
                var routerBlock = block;

                sceneHandle.OnBlock(
                    routerBlock.Id,
                    arguments =>
                    {
                        var routerContext = arguments.Context as IRouterContext;

                        if (routerContext != null)
                        {
                            // Every case ran — a false one in the middle hides nothing after it.
                            var caseResults = routerContext
                                .Cases.Select(conditionCase => conditionCase.Result == true)
                                .ToList();

                            // The same reading the engine just did: the port of each true case,
                            // then `then`/`catch` LAST. Last is what keeps the continuation as the
                            // main flow when the case routes are async.
                            var launchedPorts = LsdeUtils.PickRouterPorts(
                                routerBlock.Cases,
                                caseResults
                            );

                            _dialoguePresenter.PresentRouterBlock(
                                routerBlock,
                                routerContext.Cases,
                                launchedPorts
                            );
                        }

                        // Nothing to resolve: the exits are a tally, not a choice.
                        arguments.Next();
                        return null;
                    }
                );
            }
        }
    }
}
