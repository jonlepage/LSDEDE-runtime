using System.Collections.Generic;
using LSDE.Runtime;
using LsdeDialogEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Demo implementation of <see cref="ICharacterResolver"/> that always authorizes
    /// the first character in the list. In a real game, this resolver would check
    /// the party composition, character availability, zone presence, etc.
    /// </summary>
    public class DemoCharacterResolver : ICharacterResolver
    {
        /// <inheritdoc />
        public BlockCharacter ResolveCharacter(List<BlockCharacter> availableCharacters)
        {
            if (availableCharacters == null || availableCharacters.Count == 0)
            {
                return null;
            }

            return availableCharacters[0];
        }
    }
}
