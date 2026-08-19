using System.Collections.Generic;
using HerOClock.Progression;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Who the player owns, who is fielded and who sits out, against "#### Equipe, banco e slots"
    /// in characters.md.
    ///
    /// The three are easy to confuse and the spec separates them on purpose: slots are how many
    /// positions exist, owned is who exists to fill them, and the team is the ordered few who walk
    /// into a stage. The stage's own limit is a fourth number and belongs to the stage.
    /// </summary>
    public class RosterTests
    {
        [Test]
        public void ANewGameHasOneSlotAndOneHero()
        {
            Roster roster = new Roster();
            roster.Unlock("tempo");

            Assert.AreEqual(1, roster.Slots);
            CollectionAssert.AreEqual(new[] { "tempo" }, roster.Team);
            CollectionAssert.IsEmpty(roster.Benched());
        }

        /// <summary>
        /// With nothing able to move a hero by hand yet, one that arrived and sat invisibly on the
        /// bench would look like the unlock had simply failed.
        /// </summary>
        [Test]
        public void AnUnlockedHeroTakesAFreePositionOnItsOwn()
        {
            Roster roster = new Roster(2);
            roster.Unlock("tempo");
            roster.Unlock("gadrat");

            CollectionAssert.AreEqual(new[] { "tempo", "gadrat" }, roster.Team);
        }

        [Test]
        public void AHeroUnlockedWithNoFreePositionWaitsOnTheBench()
        {
            Roster roster = new Roster();
            roster.Unlock("tempo");
            roster.Unlock("gadrat");

            CollectionAssert.AreEqual(new[] { "tempo" }, roster.Team);
            CollectionAssert.AreEqual(new[] { "gadrat" }, roster.Benched());
            CollectionAssert.AreEqual(new[] { "tempo", "gadrat" }, roster.Owned);
        }

        /// <summary>
        /// The case act 1 actually produces: clearing a stage both introduces a hero and opens the
        /// position for them, and the two are separate events landing together.
        /// </summary>
        [Test]
        public void AGrantedSlotPullsWhoeverHasBeenWaitingLongest()
        {
            Roster roster = new Roster();
            roster.Unlock("tempo");
            roster.Unlock("gadrat");

            roster.GrantSlot();

            Assert.AreEqual(2, roster.Slots);
            CollectionAssert.AreEqual(new[] { "tempo", "gadrat" }, roster.Team);
            CollectionAssert.IsEmpty(roster.Benched());
        }

        [Test]
        public void TheSameHeroIsNeverOwnedTwice()
        {
            Roster roster = new Roster();

            Assert.IsTrue(roster.Unlock("tempo"));
            Assert.IsFalse(roster.Unlock("tempo"));

            Assert.AreEqual(1, roster.Owned.Count);
        }

        [Test]
        public void TheTeamNeverGrowsPastTheSlotsOrTheCap()
        {
            Roster roster = new Roster(99);

            Assert.AreEqual(Roster.MaxSlots, roster.Slots, "The cap on positions is four.");

            for (int i = 0; i < 10; i++)
            {
                roster.Unlock("hero-" + i);
            }

            Assert.AreEqual(Roster.MaxSlots, roster.Team.Count);
            Assert.AreEqual(10, roster.Owned.Count);
        }

        // --- What a stage takes ---

        /// <summary>
        /// A stage that accepts fewer heroes than the team has takes the first of them, so the
        /// order the player chose is what decides who goes.
        /// </summary>
        [Test]
        public void AStageLimitTakesTheFrontOfTheTeam()
        {
            Roster roster = new Roster(4);
            roster.Unlock("tempo");
            roster.Unlock("gadrat");
            roster.Unlock("third");

            CollectionAssert.AreEqual(new[] { "tempo" }, roster.PartyFor(1));
            CollectionAssert.AreEqual(new[] { "tempo", "gadrat" }, roster.PartyFor(2));
        }

        /// <summary>A stage that declares no limit takes everybody fielded.</summary>
        [Test]
        public void NoLimitTakesTheWholeTeam()
        {
            Roster roster = new Roster(4);
            roster.Unlock("tempo");
            roster.Unlock("gadrat");

            CollectionAssert.AreEqual(new[] { "tempo", "gadrat" }, roster.PartyFor(0));
        }

        /// <summary>
        /// A limit above the team is not an error. Replaying a late stage with a small team is
        /// exactly the case, and it takes whoever there is.
        /// </summary>
        [Test]
        public void ALimitLargerThanTheTeamTakesWhoeverThereIs()
        {
            Roster roster = new Roster();
            roster.Unlock("tempo");

            CollectionAssert.AreEqual(new[] { "tempo" }, roster.PartyFor(4));
        }

        // --- Coming back from a save ---

        [Test]
        public void RestoringPutsTheTeamBackInOrder()
        {
            Roster roster = new Roster();

            roster.Restore(2, new List<string> { "tempo", "gadrat" }, new List<string> { "gadrat", "tempo" });

            Assert.AreEqual(2, roster.Slots);
            CollectionAssert.AreEqual(new[] { "gadrat", "tempo" }, roster.Team);
        }

        /// <summary>A file naming somebody on the team it does not own is a file that lost track.</summary>
        [Test]
        public void RestoringIgnoresATeamMemberThatIsNotOwned()
        {
            Roster roster = new Roster();

            roster.Restore(2, new List<string> { "tempo" }, new List<string> { "tempo", "ghost" });

            CollectionAssert.AreEqual(new[] { "tempo" }, roster.Team);
        }
    }
}
