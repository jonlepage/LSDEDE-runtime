using System;
using LsdeDialogEngine;
using LsdeDialogEngine.Newtonsoft;
using UnityEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Parses a Unity TextAsset containing blueprint JSON into a <see cref="BlueprintExport"/>.
    ///
    /// <para>Uses <see cref="LsdeJson.Parse"/>, which is configured for the camelCase keys the
    /// exporter writes. The polymorphic block converter of v1 is gone: v2 has ONE block type whose
    /// optional fields depend on its <c>Type</c>, so there is nothing left to dispatch on while
    /// reading. That is also why the keys must not be renamed — <c>Text</c>, <c>Props</c> and
    /// <c>Args</c> are bags whose KEYS are the game's own data.</para>
    ///
    /// <para>Unity's own <c>JsonUtility</c> cannot read this file: it handles neither dictionaries
    /// nor object values. Newtonsoft (or System.Text.Json) is required.</para>
    /// </summary>
    public static class BlueprintLoader
    {
        /// <summary>
        /// Parse a TextAsset containing blueprint JSON into a <see cref="BlueprintExport"/>.
        /// </summary>
        /// <param name="blueprintTextAsset">
        /// The TextAsset referencing the .blueprints.json. Assign it via the Unity Inspector
        /// by dragging the file onto the field.
        /// </param>
        /// <returns>The parsed blueprint export ready for engine initialization.</returns>
        /// <exception cref="ArgumentException">Thrown when the TextAsset is null or empty.</exception>
        public static BlueprintExport Parse(TextAsset blueprintTextAsset)
        {
            if (blueprintTextAsset == null || string.IsNullOrEmpty(blueprintTextAsset.text))
            {
                throw new ArgumentException(
                    "Blueprint TextAsset is null or empty. "
                        + "Assign the .blueprints.json in the Unity Inspector."
                );
            }

            return LsdeJson.Parse(blueprintTextAsset.text);
        }
    }
}
