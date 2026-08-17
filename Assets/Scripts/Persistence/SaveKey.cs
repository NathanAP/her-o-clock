using System.Text;

namespace HerOClock.Persistence
{
    /// <summary>
    /// The key the save signature is computed with.
    ///
    /// **This is not a secret, and nothing in the project pretends it is.** It ships inside the
    /// binary, because the game has to be able to sign and verify without asking anyone. Whoever
    /// opens the binary finds it, and save.md says so out loud rather than hiding behind the word
    /// "encrypted".
    ///
    /// What it buys is the distance between "edit a number in a text editor" and "open the binary,
    /// find this, and recompute a hash". The first is ten seconds and no knowledge, and it is what
    /// practically everyone who tampers with a save does. The second is a different person, and
    /// nothing that runs on the player's own machine can stop them.
    ///
    /// It is internal so that no other system is tempted to treat it as a general purpose key. If
    /// something else ever needs one, it needs its own.
    ///
    /// Changing this value invalidates the signature of every save that already exists. They would
    /// still load, since a failed signature only marks the save, but every player would be marked
    /// at once and the mark would stop meaning anything.
    /// </summary>
    internal static class SaveKey
    {
        private const string Phrase = "her-o-clock/save/v1/6f2b9c41-a7d0-4e83-9155-c2ab8d3e07f9";

        internal static byte[] Bytes()
        {
            return Encoding.UTF8.GetBytes(Phrase);
        }
    }
}
