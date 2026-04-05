using System.Collections.Generic;
using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Resolves which character is authorized to use a block at runtime.
    /// The LSDE engine passes the list of characters assigned by the narrative designer.
    /// The game decides which one is active based on its current state (party composition, zone, etc.).
    /// Returning null means no character is available — the engine may invalidate the block.
    /// </summary>
    public interface ICharacterResolver
    {
        /// <summary>
        /// Resolve which character should be active for the current block.
        /// </summary>
        /// <param name="availableCharacters">
        /// Characters assigned to this block by the narrative designer in LSDE.
        /// Corresponds to constants in <see cref="lsdeCharacter"/>.
        /// </param>
        /// <returns>The authorized character, or null if none is available at this moment.</returns>
        BlockCharacter ResolveCharacter(List<BlockCharacter> availableCharacters);
    }
}
