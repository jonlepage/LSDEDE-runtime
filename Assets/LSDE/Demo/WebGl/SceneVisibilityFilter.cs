using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Controls the visibility of this GameObject based on the active demo scene.
    ///
    /// Attach this component to any GameObject (NPC, trigger, decoration, etc.)
    /// that should only be visible in specific demo scenes. In the Inspector,
    /// use the <see cref="_visibleInScenes"/> dropdown to check/uncheck scenes.
    ///
    /// By default all scenes are checked — the GameObject is always visible.
    /// Uncheck a scene to hide the GameObject (via <c>SetActive(false)</c>)
    /// when that demo is the active one.
    ///
    /// This component listens to the static <see cref="OnActiveSceneChanged"/> event
    /// fired by <see cref="WebGlSceneController"/> when the user selects a demo
    /// from the React sidebar.
    ///
    /// Setup in Unity Editor:
    /// 1. Add this component to any GameObject you want to filter
    /// 2. In the Inspector, uncheck scenes where this object should be hidden
    /// 3. That's it — the rest is automatic
    /// </summary>
    public class SceneVisibilityFilter : MonoBehaviour
    {
        [SerializeField]
        [Tooltip(
            "Which demo scenes this GameObject should be visible in. "
                + "Uncheck a scene to hide this object when that demo is active. "
                + "All scenes are checked by default."
        )]
        private DemoSceneFilter _visibleInScenes = DemoSceneFilter.All;

        /// <summary>
        /// Static event fired by <see cref="WebGlSceneController"/> when the active demo changes.
        /// All <see cref="SceneVisibilityFilter"/> instances subscribe to this.
        /// The string parameter is the friendly scene name (e.g. "simpleDialogFlow").
        /// </summary>
        public static event Action<string> OnActiveSceneChanged;

        /// <summary>
        /// Maps friendly scene names to their corresponding <see cref="DemoSceneFilter"/> flag.
        /// Built once, shared by all instances.
        /// </summary>
        private static readonly Dictionary<string, DemoSceneFilter> SceneNameToFlag = new()
        {
            { "simpleDialogFlow", DemoSceneFilter.SimpleDialogFlow },
            { "multiTracks", DemoSceneFilter.MultiTracks },
            { "simpleChoices", DemoSceneFilter.SimpleChoices },
            { "simpleAction", DemoSceneFilter.SimpleAction },
            { "simpleCondition", DemoSceneFilter.SimpleCondition },
            { "conditionDispatch", DemoSceneFilter.ConditionDispatch },
            { "advanceFullDemo", DemoSceneFilter.AdvanceFullDemo },
        };

        /// <summary>
        /// Call this from <see cref="WebGlSceneController"/> to notify all filters
        /// that the active demo scene has changed.
        /// </summary>
        /// <param name="sceneName">The friendly scene name (e.g. "simpleDialogFlow").</param>
        public static void NotifySceneChanged(string sceneName)
        {
            OnActiveSceneChanged?.Invoke(sceneName);
        }

        private void Awake()
        {
            OnActiveSceneChanged += HandleSceneChanged;
        }

        private void OnDestroy()
        {
            OnActiveSceneChanged -= HandleSceneChanged;
        }

        /// <summary>
        /// React to a scene change: show or hide this GameObject based on whether
        /// the new scene is checked in <see cref="_visibleInScenes"/>.
        /// </summary>
        private void HandleSceneChanged(string sceneName)
        {
            if (!SceneNameToFlag.TryGetValue(sceneName, out DemoSceneFilter sceneFlag))
            {
                return;
            }

            bool shouldBeVisible = (_visibleInScenes & sceneFlag) != 0;
            gameObject.SetActive(shouldBeVisible);
        }
    }
}
