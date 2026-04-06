using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Minimal game state for the LSDE demo — holds inventory, party membership, and variables.
    /// The LSDE condition resolver queries this state to evaluate blueprint conditions like
    /// <c>inventory.carrot >= 1</c>.
    ///
    /// In a real game, this would be replaced by your own inventory system, save manager,
    /// or game state architecture. This demo class shows the minimal API the condition
    /// resolver needs: quantity lookups, party checks, and variable reads.
    ///
    /// Toggle <see cref="_playerHasCarrot"/> in the Inspector between Play sessions to
    /// observe different branching paths in the <c>simpleCondition</c> scene.
    /// </summary>
    public class DemoGameState : MonoBehaviour
    {
        [Header("Inventory")]
        [SerializeField]
        [Tooltip(
            "Demo toggle: does the player have a carrot? "
                + "Toggle this between Play sessions to test different condition paths. "
                + "A real game would manage inventory through its own system."
        )]
        private bool _playerHasCarrot = false;

        [Header("Party")]
        [SerializeField]
        [Tooltip("Character IDs currently in the player's party.")]
        private List<string> _partyMembers = new() { "l1", "l4" };

        /// <summary>
        /// Runtime inventory store. Maps item keys (from <c>lsdeDictionaryinventory</c> constants)
        /// to their quantities. Built from Inspector toggles in <see cref="Awake"/>.
        /// </summary>
        private readonly Dictionary<string, int> _inventory = new();

        /// <summary>
        /// Runtime party membership set. Built from <see cref="_partyMembers"/> in <see cref="Awake"/>.
        /// </summary>
        private readonly HashSet<string> _partyMemberSet = new();

        /// <summary>
        /// Generic game variables for condition evaluation (e.g. score, quest flags).
        /// Empty by default — extend as needed for future demo scenes.
        /// </summary>
        private readonly Dictionary<string, float> _variables = new();

        /// <summary>
        /// Snapshot of the party members list as configured in the Inspector at startup.
        /// Used by <see cref="ResetToInitialState"/> to restore the party to its original
        /// composition when switching between demo scenes.
        /// </summary>
        private List<string> _initialPartyMembers;

        private void Awake()
        {
            // Snapshot the initial party composition before any runtime changes
            _initialPartyMembers = new List<string>(_partyMembers);

            // Build inventory from the Inspector toggle.
            // A real game would load this from a save file or persistent state.
            if (_playerHasCarrot)
            {
                _inventory[lsdeDictionaryinventory.carrot] = 1;
            }

            // Build party set from the serialized list
            foreach (string memberId in _partyMembers)
            {
                _partyMemberSet.Add(memberId);
            }
        }

        /// <summary>
        /// Get the quantity of an item in the inventory.
        /// Returns 0 if the item is not present — condition operators like <c>&gt;= 1</c>
        /// naturally evaluate to false when the item is absent.
        /// </summary>
        /// <param name="itemKey">The item key (e.g. <c>lsdeDictionaryinventory.carrot</c>).</param>
        /// <returns>The item quantity, or 0 if not in inventory.</returns>
        public int GetItemQuantity(string itemKey)
        {
            return _inventory.TryGetValue(itemKey, out int quantity) ? quantity : 0;
        }

        /// <summary>
        /// Add an item to the inventory or increase its quantity.
        /// </summary>
        /// <param name="itemKey">The item key to add.</param>
        /// <param name="quantity">How many to add (default 1).</param>
        public void AddItem(string itemKey, int quantity = 1)
        {
            if (_inventory.ContainsKey(itemKey))
            {
                _inventory[itemKey] += quantity;
            }
            else
            {
                _inventory[itemKey] = quantity;
            }

            Debug.Log(
                $"[LSDE GameState] Added {quantity}x {itemKey} — total: {_inventory[itemKey]}"
            );
        }

        /// <summary>
        /// Remove an item from the inventory entirely.
        /// </summary>
        /// <param name="itemKey">The item key to remove.</param>
        public void RemoveItem(string itemKey)
        {
            if (_inventory.Remove(itemKey))
            {
                Debug.Log($"[LSDE GameState] Removed {itemKey} from inventory.");
            }
        }

        /// <summary>
        /// Raised when a new character joins the party at runtime via <see cref="AddToParty"/>.
        /// Subscribers (e.g. <see cref="PartyFollowController"/>) use this to start
        /// tracking the new member as a follower.
        /// The string parameter is the character ID that was added.
        /// </summary>
        public event Action<string> OnPartyMemberAdded;

        /// <summary>
        /// Check whether a character is currently in the player's party.
        /// Used by the condition resolver for <c>party.*</c> dictionary keys.
        /// </summary>
        /// <param name="memberId">The character ID (e.g. <c>lsdeCharacter.l1</c>).</param>
        /// <returns>True if the character is in the party.</returns>
        public bool IsInParty(string memberId)
        {
            return _partyMemberSet.Contains(memberId);
        }

        /// <summary>
        /// Add a character to the player's party at runtime.
        /// Adds to both the serialized list (visible in Inspector) and the runtime HashSet,
        /// then raises <see cref="OnPartyMemberAdded"/> so other systems (follow, UI) can react.
        /// No effect if the character is already in the party.
        /// </summary>
        /// <param name="memberId">The character ID to add (e.g. <c>lsdeCharacter.l1</c>).</param>
        public void AddToParty(string memberId)
        {
            if (_partyMemberSet.Contains(memberId))
            {
                Debug.Log($"[LSDE GameState] {memberId} is already in the party.");
                return;
            }

            _partyMembers.Add(memberId);
            _partyMemberSet.Add(memberId);

            Debug.Log(
                $"[LSDE GameState] {memberId} joined the party! "
                    + $"Members: {_partyMemberSet.Count}"
            );

            OnPartyMemberAdded?.Invoke(memberId);
        }

        /// <summary>
        /// Get the value of a generic game variable.
        /// Returns 0 if the variable has not been set — this means unset variables
        /// naturally fail conditions like <c>score >= 10</c>.
        /// </summary>
        /// <param name="variableName">The full variable key (e.g. <c>variables.score</c>).</param>
        /// <returns>The variable value, or 0 if not set.</returns>
        public float GetVariable(string variableName)
        {
            return _variables.TryGetValue(variableName, out float value) ? value : 0f;
        }

        /// <summary>
        /// Reset all game state back to the initial conditions captured at <see cref="Awake"/>.
        /// Called by <see cref="WebGlSceneController"/> when switching between demo scenes
        /// to ensure a clean slate — no leftover inventory items, party members, or variables
        /// from the previous demo.
        /// </summary>
        public void ResetToInitialState()
        {
            // Reset inventory
            _inventory.Clear();
            if (_playerHasCarrot)
            {
                _inventory[lsdeDictionaryinventory.carrot] = 1;
            }

            // Reset party to initial composition
            _partyMembers.Clear();
            _partyMemberSet.Clear();
            foreach (string memberId in _initialPartyMembers)
            {
                _partyMembers.Add(memberId);
                _partyMemberSet.Add(memberId);
            }

            // Reset variables
            _variables.Clear();

            Debug.Log(
                $"[LSDE GameState] Reset to initial state. "
                    + $"Party: [{string.Join(", ", _partyMembers)}], "
                    + $"Inventory items: {_inventory.Count}"
            );
        }
    }
}
