using System.Collections.Generic;
using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Resolves which actor carries a block at runtime.
    ///
    /// <para>A block lists a CAST — card ids, in an order LSDE deliberately refuses to give a
    /// meaning to. The engine hands the whole list over and keeps whatever comes back; it does not
    /// elect a first one. Returning null is a legitimate answer: it means nobody available can
    /// carry this line.</para>
    ///
    /// <para>With <c>inPortPerCharacter</c> on the block, the list holds exactly ONE card — the
    /// actor the incoming wire named. The engine still asks, so the game can still say null, but
    /// it cannot be answered with a different actor than the one the designer wired.</para>
    /// </summary>
    public interface ICharacterResolver
    {
        /// <summary>
        /// Resolve which actor should carry the current block.
        /// </summary>
        /// <param name="availableCharacters">
        /// The cards the block cites, resolved through the export's Cards table, in file order.
        /// <see cref="Card.Name"/> is the writer's name for the actor (<c>l1</c>, <c>boss</c>) and
        /// <see cref="Card.Id"/> the id that survives a rename.
        /// </param>
        /// <returns>The actor that speaks, or null if none is available right now.</returns>
        Card ResolveCharacter(List<Card> availableCharacters);
    }
}
