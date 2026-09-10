using System.Collections.Generic;
using LSDE.Runtime;
using LsdeDialogEngine;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Discovers all <see cref="DialogueCharacterMarker"/> components in the scene at startup
    /// and builds a mapping from LSDE character names to their scene GameObjects.
    /// Also implements <see cref="ICharacterResolver"/> — the engine uses this to decide which
    /// actor is available at runtime, based on who is actually present in the Unity scene.
    ///
    /// <para><b>Name, not id.</b> A v2 card has two identities: <c>Id</c>, a uuid that survives a
    /// rename, and <c>Name</c>, what the writer typed (<c>l1</c>, <c>boss</c>). This demo indexes
    /// by NAME because that is the identity the rest of the payload speaks: the <c>party</c>
    /// dictionary asks about <c>party.l1</c>, and <c>moveCharacterAt</c> is called with
    /// <c>id: "l1"</c>. Indexing by uuid would be sturdier against renames but would then need a
    /// uuid↔name table to answer those two.</para>
    /// </summary>
    public class DialogueCharacterRegistry : MonoBehaviour, ICharacterResolver
    {
        private readonly Dictionary<string, DialogueCharacterMarker> _characterMarkersByName =
            new Dictionary<string, DialogueCharacterMarker>();

        /// <summary>
        /// Unity calls Awake before Start. We scan the scene for all character markers
        /// and index them by their LSDE character name for O(1) lookups.
        /// </summary>
        private void Awake()
        {
            var allCharacterMarkers = FindObjectsByType<DialogueCharacterMarker>(
                FindObjectsInactive.Exclude
            );

            foreach (var characterMarker in allCharacterMarkers)
            {
                if (string.IsNullOrEmpty(characterMarker.LsdeCharacterName))
                {
                    Debug.LogWarning(
                        $"[LSDE] DialogueCharacterMarker on '{characterMarker.gameObject.name}' "
                            + "has no LSDE character name assigned. Skipping.",
                        characterMarker
                    );
                    continue;
                }

                if (_characterMarkersByName.ContainsKey(characterMarker.LsdeCharacterName))
                {
                    Debug.LogWarning(
                        $"[LSDE] Duplicate character name '{characterMarker.LsdeCharacterName}' "
                            + $"found on '{characterMarker.gameObject.name}'. Using first occurrence.",
                        characterMarker
                    );
                    continue;
                }

                _characterMarkersByName[characterMarker.LsdeCharacterName] = characterMarker;
            }

            Debug.Log(
                $"[LSDE] Character registry initialized: {_characterMarkersByName.Count} characters found."
            );
        }

        /// <summary>
        /// Find the scene marker for a given LSDE character name.
        /// Used by <see cref="BubbleDialoguePresenter"/> to position speech bubbles and by
        /// <see cref="DemoActionExecutor"/> to move a character or aim the camera at one.
        /// </summary>
        /// <param name="characterName">The LSDE character name (e.g. "l1", "boss").</param>
        /// <returns>The marker component, or null if nobody with that name is in the scene.</returns>
        public DialogueCharacterMarker FindMarkerByCharacterName(string characterName)
        {
            if (
                characterName != null
                && _characterMarkersByName.TryGetValue(characterName, out var characterMarker)
            )
            {
                return characterMarker;
            }
            return null;
        }

        /// <inheritdoc />
        /// <remarks>
        /// Returns the first card of the cast that is actually present in the Unity scene. If none
        /// is, returns null — a legitimate answer meaning "nobody available can carry this line".
        ///
        /// <para>With <c>inPortPerCharacter</c> the list holds exactly one card, the one the wire
        /// named, so this method either confirms that actor or says nobody. That is the point of
        /// the property: the engine still asks, but the answer cannot be a different actor.</para>
        /// </remarks>
        public Card ResolveCharacter(List<Card> availableCharacters)
        {
            if (availableCharacters == null)
            {
                return null;
            }

            foreach (var card in availableCharacters)
            {
                if (_characterMarkersByName.ContainsKey(card.Name))
                {
                    return card;
                }
            }

            return null;
        }
    }
}
