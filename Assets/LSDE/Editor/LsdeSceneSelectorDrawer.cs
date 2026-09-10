using System;
using System.Collections.Generic;
using System.Reflection;
using LSDE.Runtime;
using UnityEditor;
using UnityEngine;

namespace LSDE.Editor
{
    /// <summary>
    /// Custom property drawer for <see cref="LsdeSceneSelectorAttribute"/>.
    /// Shows a dropdown of the payload's scenes instead of a raw id text field, and stores the
    /// stable scene id in the string field.
    ///
    /// <para>The list comes from the generated ids file — <c>&lt;Project&gt;BlueprintIds.Scenes</c>,
    /// e.g. <c>LsdedeDemoTsBlueprintIds.Scenes</c> — found by reflection so the drawer keeps
    /// working when the project is renamed or a second export is added. The field NAME is the
    /// scene path a writer reads (<c>simpleDialogFlow</c>); the constant's VALUE is the id that
    /// survives a rename (<c>sc_60ql8la3</c>), which is what gets stored.</para>
    ///
    /// If no generated ids class is found, this falls back to a plain text field.
    /// </summary>
    [CustomPropertyDrawer(typeof(LsdeSceneSelectorAttribute))]
    public class LsdeSceneSelectorDrawer : PropertyDrawer
    {
        /// <summary>Suffix of the generated ids class, e.g. LsdedeDemoTsBlueprintIds.</summary>
        private const string GeneratedIdsClassSuffix = "BlueprintIds";

        /// <summary>Name of the nested class holding the scenes.</summary>
        private const string ScenesNestedClassName = "Scenes";

        private static readonly List<string> CachedSceneNames = new List<string>();
        private static readonly List<string> CachedSceneIds = new List<string>();
        private static bool _isCacheInitialized;

        /// <summary>
        /// Build the scene name/id lists from the generated ids class via reflection.
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
            CachedSceneIds.Clear();

            // Index 0 is always "no scene": the field is legitimately empty when something else
            // (a proximity trigger, the WebGL sidebar) decides which scene to launch.
            CachedSceneNames.Add("(none)");
            CachedSceneIds.Add("");

            var scenesClass = FindGeneratedScenesClass();
            if (scenesClass == null)
            {
                Debug.LogWarning(
                    "[LSDE] No generated ids class found. Make sure the "
                        + "*.blueprints.ids.cs file exported by LSDE is in the project."
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
                    CachedSceneNames.Add(field.Name);
                    CachedSceneIds.Add((string)field.GetRawConstantValue());
                }
            }
        }

        /// <summary>
        /// Find the <c>Scenes</c> class nested in the generated ids class.
        /// The generated class has no namespace and its name depends on the LSDE project name, so
        /// we look for any type whose name ends with <c>BlueprintIds</c> and take its nested
        /// <c>Scenes</c>.
        /// </summary>
        private static Type FindGeneratedScenesClass()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] assemblyTypes;

                try
                {
                    assemblyTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    // A partially loaded assembly is not where the generated file lives.
                    continue;
                }

                foreach (var type in assemblyTypes)
                {
                    if (
                        type.Namespace == null
                        && type.IsAbstract
                        && type.IsSealed
                        && type.Name.EndsWith(GeneratedIdsClassSuffix, StringComparison.Ordinal)
                    )
                    {
                        var nestedScenes = type.GetNestedType(
                            ScenesNestedClassName,
                            BindingFlags.Public
                        );

                        if (nestedScenes != null)
                        {
                            return nestedScenes;
                        }
                    }
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

            // No scenes found: fall back to a plain text field rather than an empty dropdown.
            if (CachedSceneNames.Count <= 1)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            string currentSceneId = property.stringValue;
            int selectedIndex = CachedSceneIds.IndexOf(currentSceneId);

            // A stored value the current payload does not know — a leftover from another export,
            // or a v1 uuid. Fall back to "(none)" so the field reads as "nothing picked yet"
            // rather than showing a scene it will not launch.
            if (selectedIndex < 0)
            {
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
                property.stringValue = CachedSceneIds[newSelectedIndex];
            }

            EditorGUI.EndProperty();
        }
    }
}
