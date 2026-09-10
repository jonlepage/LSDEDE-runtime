using System.Collections.Generic;
using LSDE.Runtime;
using LsdeDialogEngine;

namespace LSDE.Demo
{
    /// <summary>
    /// Simplest possible <see cref="ICharacterResolver"/>: it keeps the first card of the cast.
    /// Used by the console presenter, where nothing is drawn and scene presence is irrelevant.
    ///
    /// <para>The visual demo uses <see cref="DialogueCharacterRegistry"/> instead, which answers
    /// from who is actually present in the Unity scene — that is the shape a real game wants.</para>
    ///
    /// <para>Note what the engine does NOT do: it never elects a first actor on its own. The order
    /// of <c>actors</c> carries no meaning in LSDE, so picking <c>[0]</c> is a decision this class
    /// makes, not a rule of the format.</para>
    /// </summary>
    public class DemoCharacterResolver : ICharacterResolver
    {
        /// <inheritdoc />
        public Card ResolveCharacter(List<Card> availableCharacters)
        {
            if (availableCharacters == null || availableCharacters.Count == 0)
            {
                return null;
            }

            return availableCharacters[0];
        }
    }
}
