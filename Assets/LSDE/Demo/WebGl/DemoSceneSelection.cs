namespace LSDE.Demo
{
    /// <summary>
    /// Single-select enum for choosing one LSDE demo scene.
    /// Used by <see cref="WebGlSceneController"/> for the editor test dropdown.
    ///
    /// Unlike <see cref="DemoSceneFilter"/> (which is a <c>[Flags]</c> enum for multi-select),
    /// this enum allows selecting exactly one scene at a time — it appears as a standard
    /// dropdown in the Inspector, not as checkboxes.
    /// </summary>
    public enum DemoSceneSelection
    {
        SimpleDialogFlow = 0,
        MultiTracks = 1,
        SimpleChoices = 2,
        SimpleAction = 3,
        SimpleCondition = 4,
        ConditionDispatch = 5,
        AdvanceFullDemo = 6,
    }
}
