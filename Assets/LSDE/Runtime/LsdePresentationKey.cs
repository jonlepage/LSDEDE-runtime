using LsdeDialogEngine;

namespace LSDE.Runtime
{
    /// <summary>
    /// Builds the key that identifies ONE dispatch of a block.
    ///
    /// <para>A block id is not enough. Ids repeat between scenes — <c>DIALOG-001</c> legitimately
    /// exists in several — and, more importantly, the engine can dispatch one block twice at the
    /// same moment: with <c>inPortPerCharacter</c> several wires reach a single block and each
    /// names a different actor, so it runs once per wire, in parallel. Keying a bubble by block id
    /// alone makes the second dispatch overwrite the first, and the cleanup of one path closes the
    /// bubble of another.</para>
    ///
    /// <para>The id is the half the game cannot reconstruct; the other half is a counter. Pairing
    /// the block with the actor would look tidier but is not enough either — the same block can be
    /// reached twice for the same actor, through a loop or through two wires that name it. A plain
    /// monotonic counter cannot collide, which is what the reference TypeScript demo uses
    /// (<c>`${block.id}#${++dispatchSeq}`</c>).</para>
    ///
    /// <para>The handler asks for a key once, hands it to the presenter, and captures it in the
    /// cleanup it returns — so both ends of one dispatch agree.</para>
    /// </summary>
    public static class LsdePresentationKey
    {
        /// <summary>
        /// How many keys have been handed out. Unity runs game code on one thread, so a plain
        /// counter is enough.
        /// </summary>
        private static int _dispatchSequence;

        /// <summary>
        /// Take the next key for a dispatch of this block.
        /// </summary>
        /// <param name="block">The block being dispatched.</param>
        /// <returns>A key no other dispatch can carry.</returns>
        public static string Next(BlueprintBlock block)
        {
            _dispatchSequence++;
            return block.Id + "#" + _dispatchSequence;
        }
    }
}
