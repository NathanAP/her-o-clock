using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.View;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// The rule that turns a step across the board into a direction.
    ///
    /// Worth testing even though it only feeds presentation: it is arithmetic with a tie to
    /// break, the tie is a decision rather than a detail, and getting it wrong shows up as a
    /// character walking backwards, which is the kind of thing that survives a whole version
    /// because everybody assumes somebody meant it that way.
    /// </summary>
    public class FacingResolverTests
    {
        private static GridPosition Cell(int column, int row)
        {
            return new GridPosition(column, row);
        }

        [Test]
        public void HeroesStartFacingUpAndEnemiesFacingDown()
        {
            // Heroes hold rows 1 to 4 and everybody else holds 5 to 8, so the two sides begin
            // looking at each other.
            Assert.AreEqual(Facing.Up, FacingResolver.Default(Team.Heroes));
            Assert.AreEqual(Facing.Down, FacingResolver.Default(Team.Enemies));
        }

        [Test]
        public void AStepUpTheBoardFacesUp()
        {
            Assert.AreEqual(Facing.Up, FacingResolver.FromStep(Cell(3, 2), Cell(3, 3), Facing.Down));
        }

        [Test]
        public void AStepDownTheBoardFacesDown()
        {
            Assert.AreEqual(Facing.Down, FacingResolver.FromStep(Cell(3, 3), Cell(3, 2), Facing.Up));
        }

        [Test]
        public void AStepAcrossARowFacesSideways()
        {
            Assert.AreEqual(Facing.Right, FacingResolver.FromStep(Cell(2, 3), Cell(3, 3), Facing.Up));
            Assert.AreEqual(Facing.Left, FacingResolver.FromStep(Cell(3, 3), Cell(2, 3), Facing.Up));
        }

        [Test]
        public void ADiagonalStepFacesUpOrDownRatherThanSideways()
        {
            // The decision, not an accident: the board is read from the bottom up and crossing a
            // row is what the movement means, so drifting one column while doing it still reads
            // as going forward.
            Assert.AreEqual(Facing.Up, FacingResolver.FromStep(Cell(2, 2), Cell(3, 3), Facing.Down));
            Assert.AreEqual(Facing.Down, FacingResolver.FromStep(Cell(3, 3), Cell(2, 2), Facing.Up));
        }

        [Test]
        public void AStepThatGoesNowhereKeepsTheCurrentDirection()
        {
            // A character that stopped to attack should keep looking at whoever it walked to.
            Assert.AreEqual(Facing.Left, FacingResolver.FromStep(Cell(3, 3), Cell(3, 3), Facing.Left));
        }
    }

    /// <summary>
    /// The set of drawings a character carries.
    ///
    /// These are the fallbacks, and every one of them exists so that missing art degrades into
    /// something visible instead of into an invisible character.
    /// </summary>
    public class CharacterSpritesTests
    {
        [Test]
        public void ACharacterWithNoArtAtAllIsRecognised()
        {
            // This is what keeps Gadrat and the minions as coloured rectangles while Tempo has
            // a sprite, without either of them needing to know about the other.
            Assert.IsFalse(new CharacterSprites().HasAny);
        }

        [Test]
        public void OnlyTheLeftIsMirrored()
        {
            // There is no left drawing. It is the side drawing flipped, and the side drawing
            // faces right.
            Assert.IsTrue(CharacterSprites.IsMirrored(Facing.Left));
            Assert.IsFalse(CharacterSprites.IsMirrored(Facing.Right));
            Assert.IsFalse(CharacterSprites.IsMirrored(Facing.Up));
            Assert.IsFalse(CharacterSprites.IsMirrored(Facing.Down));
        }
    }
}
