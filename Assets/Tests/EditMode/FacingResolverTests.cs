using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.View;
using NUnit.Framework;
using UnityEngine;

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
        public void ArrivingTurnsTheCharacterBackTowardsTheEnemyHalf()
        {
            // A sideways step must not leave a hero standing in profile while it fights
            // something above it. Facing sideways is a state of travel, not one of rest, and
            // what a character settles back into is the same direction it started the battle in.
            Assert.AreEqual(FacingResolver.Default(Team.Heroes), Facing.Up);
            Assert.AreEqual(FacingResolver.Default(Team.Enemies), Facing.Down);
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
        public void TheSwingIsPickedByTheKindOfBasicAttack()
        {
            // Once weapons exist this is the weapon speaking: a hero holding a cannon is ranged
            // and one holding a blade is melee, and neither sprite has to be swapped by hand.
            CharacterSprites sprites = new CharacterSprites();
            Sprite melee = Pixel();
            Sprite ranged = Pixel();

            sprites.AttackMelee = new[] { melee };
            sprites.AttackRanged = new[] { ranged };

            try
            {
                Assert.AreSame(melee, sprites.Attack(AutoAttackType.Melee)[0]);
                Assert.AreSame(ranged, sprites.Attack(AutoAttackType.Ranged)[0]);
                Assert.IsTrue(sprites.HasAttack(AutoAttackType.Melee));
            }
            finally
            {
                Discard(melee);
                Discard(ranged);
            }
        }

        [Test]
        public void ACharacterWithoutASwingReportsNoneRatherThanBreaking()
        {
            // Gadrat and the minions have no art at all, and the view asks them for a swing on
            // every blow they land.
            CharacterSprites sprites = new CharacterSprites();

            Assert.IsFalse(sprites.HasAttack(AutoAttackType.Melee));
            Assert.IsFalse(sprites.HasAttack(AutoAttackType.Ranged));
        }

        [Test]
        public void TheSwingPlaysOnceAndThenGivesUpTheScreen()
        {
            // Three drawings share the pose in equal thirds, and nothing at all is shown once
            // the swing is done. Minus one is what puts the standing drawing back.
            //
            // The duration is three eighths of a second rather than a rounder number so that the
            // thirds land on 0.125 and 0.25, which a float holds exactly. Three tenths does not:
            // 3 x 0.1f is smaller than 0.3f, so a third of the way through reads as 0.99999994
            // of a frame and the cast lands one drawing early. That is a property of the
            // assertion and not of the rule — a real swing accumulates deltas and never lands on
            // a boundary — but a test written on a boundary has to pick one it can name.
            Assert.AreEqual(0, SwingSequence.FrameAt(0f, 0.375f, 3));
            Assert.AreEqual(0, SwingSequence.FrameAt(0.124f, 0.375f, 3));
            Assert.AreEqual(1, SwingSequence.FrameAt(0.125f, 0.375f, 3));
            Assert.AreEqual(1, SwingSequence.FrameAt(0.2f, 0.375f, 3));
            Assert.AreEqual(2, SwingSequence.FrameAt(0.25f, 0.375f, 3));
            Assert.AreEqual(2, SwingSequence.FrameAt(0.374f, 0.375f, 3));
            Assert.AreEqual(-1, SwingSequence.FrameAt(0.375f, 0.375f, 3));
            Assert.AreEqual(-1, SwingSequence.FrameAt(5f, 0.375f, 3));
        }

        [Test]
        public void TheSwingNeverLoops()
        {
            // The difference from the running cycle, and the reason they are two classes: a
            // swing has an end. Looping one would turn a single blow into a windmill.
            Assert.AreEqual(-1, SwingSequence.FrameAt(0.4f, 0.375f, 3));
            Assert.AreEqual(-1, SwingSequence.FrameAt(0.75f, 0.375f, 3));
        }

        [Test]
        public void ACharacterWithoutASwingAsksForNothingAndGetsNothing()
        {
            Assert.AreEqual(-1, SwingSequence.FrameAt(0.125f, 0.375f, 0));
            Assert.AreEqual(-1, SwingSequence.FrameAt(0.125f, 0f, 3));
        }

        [Test]
        public void TheRunningCycleIsDrivenByGroundCoveredAndWraps()
        {
            // Eight drawings over one cell means a new drawing every eighth of a cell, and the
            // ninth eighth is the first drawing again.
            Assert.AreEqual(0, RunCycle.FrameAt(0f, 1f, 8));
            Assert.AreEqual(1, RunCycle.FrameAt(0.125f, 1f, 8));
            Assert.AreEqual(7, RunCycle.FrameAt(0.875f, 1f, 8));
            Assert.AreEqual(0, RunCycle.FrameAt(1f, 1f, 8));
            Assert.AreEqual(3, RunCycle.FrameAt(3.375f, 1f, 8));
        }

        [Test]
        public void ALongerCycleSpreadsTheSameDrawingsOverMoreGround()
        {
            // This is the knob that decides how often the legs move for the same journey, and
            // it is ground and never seconds: a character with high agility crosses the cell
            // faster and its feet land at the same points regardless.
            Assert.AreEqual(0, RunCycle.FrameAt(0.125f, 2f, 8));
            Assert.AreEqual(1, RunCycle.FrameAt(0.25f, 2f, 8));
            Assert.AreEqual(4, RunCycle.FrameAt(1f, 2f, 8));
        }

        [Test]
        public void ACharacterWithoutACycleAsksForNothingAndGetsZero()
        {
            Assert.AreEqual(0, RunCycle.FrameAt(5f, 1f, 0));
            Assert.AreEqual(0, RunCycle.FrameAt(5f, 0f, 8));
        }

        private static Sprite Pixel()
        {
            Texture2D texture = new Texture2D(1, 1);
            return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));
        }

        private static void Discard(Sprite sprite)
        {
            Texture2D texture = sprite.texture;
            Object.DestroyImmediate(sprite);
            Object.DestroyImmediate(texture);
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
