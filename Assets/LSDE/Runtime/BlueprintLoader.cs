using System;
using LsdeDialogEngine;
using LsdeDialogEngine.Newtonsoft;
using UnityEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Parses a Unity TextAsset containing blueprint JSON into a <see cref="BlueprintExport"/>.
    /// Uses <see cref="LsdeJson.Parse"/> which configures polymorphic deserialization
    /// for BlueprintBlock subtypes (DialogBlock, ChoiceBlock, ConditionBlock, ActionBlock, NoteBlock).
    /// Never use JsonConvert.DeserializeObject directly — it will not handle block type discrimination.
    /// </summary>
    public static class BlueprintLoader
    {
        /// <summary>
        /// Parse a TextAsset containing blueprint JSON into a <see cref="BlueprintExport"/>.
        /// </summary>
        /// <param name="blueprintTextAsset">
        /// The TextAsset referencing blueprint.json. Assign it via the Unity Inspector
        /// by dragging the blueprint.json file onto the field.
        /// </param>
        /// <returns>The parsed blueprint export ready for engine initialization.</returns>
        /// <exception cref="ArgumentException">Thrown when the TextAsset is null or empty.</exception>
        public static BlueprintExport Parse(TextAsset blueprintTextAsset)
        {
            if (blueprintTextAsset == null || string.IsNullOrEmpty(blueprintTextAsset.text))
            {
                throw new ArgumentException(
                    "Blueprint TextAsset is null or empty. "
                        + "Assign blueprint.json in the Unity Inspector."
                );
            }

            return LsdeJson.Parse(blueprintTextAsset.text);
        }
    }
}
