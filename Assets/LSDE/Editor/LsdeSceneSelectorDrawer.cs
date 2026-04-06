using System.Collections.Generic;
using System.Reflection;
using LSDE.Runtime;
using UnityEditor;
using UnityEngine;

namespace LSDE.Editor
{
    /// <summary>
    /// Custom property drawer for <see cref="LsdeSceneSelectorAttribute"/>.
    /// Displays a dropdown of all scene names from <c>LSDE_SCENES</c> instead of
    /// a raw UUID text field. The UUID is stored in the string field automatically.
    ///
    /// If the LSDE_SCENES class is not found or has no constants, falls back to
    /// the default string text field.
    /// </summary>
    [CustomPropertyDrawer(typeof(LsdeSceneSelectorAttribute))]
    public class LsdeSceneSelectorDrawer : PropertyDrawer
    {
        private static readonly List<string> CachedSceneNames = new List<string>();
        private static readonly List<string> CachedSceneUuids = new List<string>();
        private static bool _isCacheInitialized;

        /// <summary>
        /// Build the scene name/UUID lists from LSDE_SCENES via reflection.
        /// Cached once per domain reload for performance.
        /// </summary>
        private static void EnsureCacheInitialized()
        {
            if (_isCacheInitialized)
            {
                return;
            }

            _isCacheInitialized = true;
            CachedSceneNames.Clear();
            CachedSceneUuids.Clear();

            // Add a "None" option at index 0
            CachedSceneNames.Add("(none)");
            CachedSceneUuids.Add("");

            // Find LSDE_SCENES class via reflection (it's auto-generated, not in a namespace)
            var scenesClass = FindTypeByName("LSDE_SCENES");
            if (scenesClass == null)
            {
                Debug.LogWarning(
                    "[LSDE] LSDE_SCENES class not found. "
                        + "Make sure BlueprintEnums.cs is in the project."
                );
                return;
            }

            var sceneConstantFields = scenesClass.GetFields(
                BindingFlags.Public | BindingFlags.Static
            );

            foreach (var field in sceneConstantFields)
            {
                if (field.IsLiteral && field.FieldType == typeof(string))
                {
                    var sceneUuid = (string)field.GetRawConstantValue();
                    CachedSceneNames.Add(field.Name);
                    CachedSceneUuids.Add(sceneUuid);
                }
            }
        }

        /// <summary>
        /// Search all loaded assemblies for a type by name (LSDE_SCENES has no namespace).
        /// </summary>
        private static System.Type FindTypeByName(string typeName)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var foundType = assembly.GetType(typeName);
                if (foundType != null)
                {
                    return foundType;
                }
            }

            return null;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EnsureCacheInitialized();

            // If no scenes found, fall back to plain text field
            if (CachedSceneNames.Count <= 1)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            // Find current selection index from the stored UUID
            string currentUuid = property.stringValue;
            int selectedIndex = CachedSceneUuids.IndexOf(currentUuid);
            if (selectedIndex < 0)
            {
                // UUID exists but not in our list — show it as-is with a warning
                selectedIndex = 0;
            }

            EditorGUI.BeginProperty(position, label, property);

            int newSelectedIndex = EditorGUI.Popup(
                position,
                label.text,
                selectedIndex,
                CachedSceneNames.ToArray()
            );

            if (newSelectedIndex != selectedIndex)
            {
                property.stringValue = CachedSceneUuids[newSelectedIndex];
            }

            EditorGUI.EndProperty();
        }
    }
}
