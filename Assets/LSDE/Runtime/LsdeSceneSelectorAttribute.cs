using UnityEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Attribute that marks a string field to display a dropdown of available
    /// LSDE scene names in the Unity Inspector, instead of a raw UUID text field.
    /// The selected scene's UUID is stored in the string field automatically.
    ///
    /// Usage:
    /// <code>
    /// [SerializeField]
    /// [LsdeSceneSelector]
    /// private string _sceneUuidToLaunch;
    /// </code>
    ///
    /// The dropdown reads all public const string fields from the generated
    /// <c>&lt;Project&gt;BlueprintIds.Scenes</c> class
    /// and displays them as selectable options.
    /// </summary>
    public class LsdeSceneSelectorAttribute : PropertyAttribute { }
}
