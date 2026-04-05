using System.Collections.Generic;
using LSDE.Runtime;
using LsdeDialogEngine;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Discovers all <see cref="DialogueCharacterMarker"/> components in the scene at startup
    /// and builds a mapping from LSDE character IDs to their scene GameObjects.
    /// Also implements <see cref="ICharacterResolver"/> — the engine uses this to determine
    /// which character is available at runtime based on scene presence.
    ///
    /// Replaces <see cref="DemoCharacterResolver"/> from Phase 1.
    /// </summary>
    public class DialogueCharacterRegistry : MonoBehaviour, ICharacterResolver
    {
        private readonly Dictionary<string, DialogueCharacterMarker> _characterMarkersByIdentifier =
            new Dictionary<string, DialogueCharacterMarker>();

        /// <summary>
        /// Unity calls Awake before Start. We scan the scene for all character markers
        /// and index them by their LSDE character ID for O(1) lookups.
        /// </summary>
        private void Awake()
        {
            var allCharacterMarkers = FindObjectsByType<DialogueCharacterMarker>(
                FindObjectsInactive.Exclude
            );

            foreach (var characterMarker in allCharacterMarkers)
            {
                if (string.IsNullOrEmpty(characterMarker.LsdeCharacterId))
                {
                    Debug.LogWarning(
                        $"[LSDE] DialogueCharacterMarker on '{characterMarker.gameObject.name}' "
                            + "has no LSDE character ID assigned. Skipping.",
                        characterMarker
                    );
                    continue;
                }

                if (_characterMarkersByIdentifier.ContainsKey(characterMarker.LsdeCharacterId))
                {
                    Debug.LogWarning(
                        $"[LSDE] Duplicate character ID '{characterMarker.LsdeCharacterId}' "
                            + $"found on '{characterMarker.gameObject.name}'. Using first occurrence.",
                        characterMarker
                    );
                    continue;
                }

                _characterMarkersByIdentifier[characterMarker.LsdeCharacterId] = characterMarker;
            }

            Debug.Log(
                $"[LSDE] Character registry initialized: {_characterMarkersByIdentifier.Count} characters found."
            );
        }

        /// <summary>
        /// Find the scene marker for a given LSDE character ID.
        /// Used by <see cref="BubbleDialoguePresenter"/> to position speech bubbles.
        /// </summary>
        /// <param name="characterId">The LSDE character ID (e.g. "l1", "boss").</param>
        /// <returns>The marker component, or null if no character with this ID exists in the scene.</returns>
        public DialogueCharacterMarker FindMarkerByCharacterId(string characterId)
        {
            if (
                characterId != null
                && _characterMarkersByIdentifier.TryGetValue(characterId, out var characterMarker)
            )
            {
                return characterMarker;
            }
            return null;
        }

        /// <inheritdoc />
        /// <remarks>
        /// Returns the first character from the available list whose ID matches
        /// a marker present in the scene. If none match, returns null —
        /// the engine may invalidate the block via OnInvalidateBlock.
        /// </remarks>
        public BlockCharacter ResolveCharacter(List<BlockCharacter> availableCharacters)
        {
            if (availableCharacters == null)
            {
                return null;
            }

            foreach (var blockCharacter in availableCharacters)
            {
                if (_characterMarkersByIdentifier.ContainsKey(blockCharacter.Id))
                {
                    return blockCharacter;
                }
            }

            return null;
        }
    }
}
