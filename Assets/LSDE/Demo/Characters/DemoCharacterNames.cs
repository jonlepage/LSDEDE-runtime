namespace LSDE.Demo
{
    /// <summary>
    /// The card NAMES this Unity scene knows about.
    ///
    /// <para>The generated <c>LsdedeDemoTsBlueprintIds.Cards</c> constants hold card <b>uuids</b>,
    /// which is what <c>Card.Id</c> carries and what survives a rename. The names below are the
    /// other identity — <c>Card.Name</c> — and they are what the rest of the payload speaks: the
    /// <c>party</c> dictionary asks about <c>party.l1</c>, <c>moveCharacterAt</c> is called with
    /// <c>id: "l1"</c>, and <see cref="DialogueCharacterMarker"/> carries one per GameObject.</para>
    ///
    /// <para>The generated file cannot provide these as one list — a name only appears there when
    /// a dictionary happens to use it — so this is the demo's own table. Rename a card in LSDE and
    /// this file has to follow, which is exactly the trade-off of keying a game by name.</para>
    /// </summary>
    public static class DemoCharacterNames
    {
        /// <summary>The rabbit who starts most scenes.</summary>
        public const string L1 = "l1";

        /// <summary>Second party member.</summary>
        public const string L2 = "l2";

        /// <summary>Third party member, the one the ROUTER scenes single out.</summary>
        public const string L3 = "l3";

        /// <summary>The player character — the party leader, never a follower.</summary>
        public const string L4 = "l4";

        /// <summary>The creature.</summary>
        public const string Boss = "boss";

        /// <summary>Every name above, in one list, for iteration.</summary>
        public static readonly string[] All = { L1, L2, L3, L4, Boss };
    }
}
