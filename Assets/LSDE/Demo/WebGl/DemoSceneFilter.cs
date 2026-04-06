using System;

namespace LSDE.Demo
{
    /// <summary>
    /// Flags enum representing the 7 LSDE demo scenes.
    /// Used by <see cref="SceneVisibilityFilter"/> to control which GameObjects
    /// are visible in which demo scenes.
    ///
    /// In the Unity Inspector, this appears as a multi-select dropdown with checkboxes.
    /// By default, all scenes are selected (the GameObject is visible everywhere).
    /// Uncheck a scene to hide the GameObject when that demo is active.
    /// </summary>
    [Flags]
    public enum DemoSceneFilter
    {
        SimpleDialogFlow = 1 << 0,
        MultiTracks = 1 << 1,
        SimpleChoices = 1 << 2,
        SimpleAction = 1 << 3,
        SimpleCondition = 1 << 4,
        ConditionDispatch = 1 << 5,
        AdvanceFullDemo = 1 << 6,

        /// <summary>All scenes selected — the default. GameObject is always visible.</summary>
        All = ~0,
    }
}
