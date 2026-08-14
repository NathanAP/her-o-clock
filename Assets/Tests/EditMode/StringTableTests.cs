using System.Collections.Generic;
using HerOClock.Text;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Checks the table every piece of player facing text is read from, and the validator that
    /// keeps it honest.
    ///
    /// The keys are built from ids rather than written by hand, so most of what could go wrong is
    /// a key that exists in the content but not in the file, or the other way round. Both are
    /// invisible at runtime: nothing crashes, the game just shows the wrong words.
    /// </summary>
    public class StringTableTests
    {
        private static StringTableData Data(params string[] keysAndValues)
        {
            StringEntry[] entries = new StringEntry[keysAndValues.Length / 2];

            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = new StringEntry { key = keysAndValues[i * 2], value = keysAndValues[i * 2 + 1] };
            }

            return new StringTableData { language = "en", entries = entries };
        }

        private static StringTable Build(StringTableData data, out List<string> problems)
        {
            problems = new List<string>();
            return StringTable.From(data, problems);
        }

        // --- Key shapes ---

        [Test]
        public void KeysAreBuiltFromTheId()
        {
            Assert.AreEqual("character.hero-tank.name", StringTable.CharacterName("hero-tank"));
            Assert.AreEqual("stage.act1-stage1.name", StringTable.StageName("act1-stage1"));
            Assert.AreEqual("stage.act1-stage1.lore", StringTable.StageLore("act1-stage1"));
        }

        // --- Reading ---

        [Test]
        public void AKnownKeyGivesItsText()
        {
            List<string> problems;
            StringTable strings = Build(Data("a.b", "Hello"), out problems);

            Assert.AreEqual("Hello", strings.Get("a.b"));
            CollectionAssert.IsEmpty(problems);
        }

        /// <summary>
        /// A missing key comes back visible on purpose. An empty string would simply not be drawn,
        /// and a label that quietly disappears is far harder to notice than one reading
        /// <c>#stage.act1-stage1.name#</c>.
        /// </summary>
        [Test]
        public void AMissingKeyComesBackAsSomethingImpossibleToMiss()
        {
            List<string> problems;
            StringTable strings = Build(Data("a.b", "Hello"), out problems);

            Assert.AreEqual("#nope#", strings.Get("nope"));
            Assert.IsFalse(strings.Has("nope"));
        }

        [Test]
        public void KeysAreCaseSensitive()
        {
            List<string> problems;
            StringTable strings = Build(Data("a.b", "Hello"), out problems);

            Assert.IsFalse(strings.Has("A.B"));
        }

        // --- Building ---

        [Test]
        public void AFileThatCouldNotBeReadIsReported()
        {
            List<string> problems = new List<string>();
            StringTable.From(null, problems);

            Assert.IsNotEmpty(problems);
        }

        [Test]
        public void AFileWithNoEntriesIsReported()
        {
            List<string> problems;
            Build(new StringTableData { language = "en", entries = new StringEntry[0] }, out problems);

            Assert.IsNotEmpty(problems);
        }

        [Test]
        public void AFileWithoutALanguageIsReported()
        {
            List<string> problems;
            Build(new StringTableData { language = "", entries = Data("a.b", "Hello").entries }, out problems);

            Assert.IsNotEmpty(problems);
        }

        [Test]
        public void AnEntryWithoutAKeyIsReported()
        {
            List<string> problems;
            Build(Data("", "Hello"), out problems);

            Assert.IsNotEmpty(problems);
        }

        [Test]
        public void AnEntryWithoutTextIsReported()
        {
            List<string> problems;
            Build(Data("a.b", ""), out problems);

            Assert.IsNotEmpty(problems);
        }

        /// <summary>
        /// A duplicate key is the one mistake that would otherwise resolve silently, with the
        /// winner decided by file order.
        /// </summary>
        [Test]
        public void ARepeatedKeyIsReported()
        {
            List<string> problems;
            Build(Data("a.b", "First", "a.b", "Second"), out problems);

            Assert.IsNotEmpty(problems);
        }

        // --- The validator ---

        private static readonly string[] NoIds = new string[0];

        [Test]
        public void AFileHoldingExactlyWhatIsAskedForPasses()
        {
            List<string> problems;
            StringTable strings = Build(Data(
                "character.hero-tank.name", "Tank Hero",
                "stage.one.name", "One",
                "stage.one.lore", "Something happened."), out problems);

            StringTableValidator.Validate(strings, new[] { "hero-tank" }, new[] { "one" }, problems);

            CollectionAssert.IsEmpty(problems);
        }

        [Test]
        public void ACharacterWithoutANameIsReported()
        {
            List<string> problems;
            StringTable strings = Build(Data("character.hero-tank.name", "Tank Hero"), out problems);

            StringTableValidator.Validate(strings, new[] { "hero-tank", "hero-archer" }, NoIds, problems);

            Assert.IsNotEmpty(problems);
        }

        [Test]
        public void AStageMissingItsLoreIsReported()
        {
            List<string> problems;
            StringTable strings = Build(Data("stage.one.name", "One"), out problems);

            StringTableValidator.Validate(strings, NoIds, new[] { "one" }, problems);

            Assert.IsNotEmpty(problems);
        }

        /// <summary>
        /// Text left behind by content that no longer exists is not an error, but a file full of
        /// it becomes impossible to translate, because nobody can tell which half still matters.
        /// </summary>
        [Test]
        public void TextNobodyAsksForIsReported()
        {
            List<string> problems;
            StringTable strings = Build(Data(
                "character.hero-tank.name", "Tank Hero",
                "character.deleted-hero.name", "Ghost"), out problems);

            StringTableValidator.Validate(strings, new[] { "hero-tank" }, NoIds, problems);

            Assert.IsNotEmpty(problems);
        }
    }
}
