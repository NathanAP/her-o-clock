using System.Collections.Generic;
using HerOClock.Characters;
using HerOClock.Items;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Attribute = HerOClock.Characters.Attribute;

namespace HerOClock.Tests
{
    /// <summary>
    /// Wearing equipment: what a piece demands, what it defends, which pieces count, and what the
    /// character ends up with.
    ///
    /// The numbers come from `Assets/Items/slots.json` and from attributes.md, and are repeated
    /// here rather than read from the file. That duplication is the point: two independent
    /// statements of the same value, so that a disagreement shows up instead of quietly winning.
    /// </summary>
    public class EquipmentTests
    {
        private static ItemRules Rules()
        {
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Items/slots.json");
            Assert.IsNotNull(asset, "There is no Assets/Items/slots.json.");

            return new ItemRules(JsonUtility.FromJson<ItemSlots>(asset.text));
        }

        private static Item Piece(
            string id, string slot, string itemClass, int level, params ItemModifierRoll[] modifiers)
        {
            return new Item(id, slot, itemClass, string.Empty, "conventional", level,
                new List<ItemModifierRoll>(modifiers));
        }

        private static ItemModifierRoll Roll(string id, float value)
        {
            return new ItemModifierRoll(id, "hardware", 1, value);
        }

        // --- The requirement, from slots.json: 1.5 per item level, hybrids at 0.6 of each side ---

        /// <summary>
        /// A pure class demands one attribute at the full rate. A level 10 piece asks for 15.
        /// </summary>
        [TestCase("heavy", Attribute.Power)]
        [TestCase("light", Attribute.Agility)]
        [TestCase("special", Attribute.Specialty)]
        public void APureClassDemandsOneAttributeAtTheFullRate(string itemClass, Attribute attribute)
        {
            Dictionary<Attribute, int> demanded = Rules().RequirementOf(Piece("i", "chassis", itemClass, 10));

            Assert.AreEqual(1, demanded.Count);
            Assert.AreEqual(15, demanded[attribute]);
        }

        /// <summary>
        /// items.md: a hybrid asks for both attributes, each at a share of the full value, so it
        /// costs more in total and less on each side. That is what makes the choice exist instead
        /// of hybrid being strictly worse.
        /// </summary>
        [Test]
        public void AHybridDemandsBothOfItsSidesAtAShare()
        {
            Dictionary<Attribute, int> demanded = Rules().RequirementOf(Piece("i", "chassis", "medium", 10));

            Assert.AreEqual(2, demanded.Count);
            Assert.AreEqual(9, demanded[Attribute.Power], "15 at the 0.6 share.");
            Assert.AreEqual(9, demanded[Attribute.Agility]);
            Assert.Greater(9 + 9, 15, "Hybrid costs more in total than a pure class.");
        }

        /// <summary>
        /// Rounded up. A requirement of 10.5 that a character with 10 points could meet would not
        /// be a requirement.
        /// </summary>
        [Test]
        public void ARequirementIsRoundedUp()
        {
            Dictionary<Attribute, int> demanded = Rules().RequirementOf(Piece("i", "chassis", "heavy", 7));

            Assert.AreEqual(11, demanded[Attribute.Power], "1.5 x 7 is 10.5.");
        }

        // --- The base defence: budget x slot weight x class share ---

        /// <summary>
        /// slots.json: armour is 12 with 12 per level, evasion 18 with 18, elemental 5 with 5, and
        /// the chassis carries the full weight. At level 10 that is ten times the base.
        /// </summary>
        [TestCase("heavy", 120, 0, 0)]
        [TestCase("light", 0, 180, 0)]
        [TestCase("special", 0, 0, 50)]
        public void APureClassTurnsTheWholeBudgetIntoItsOwnDefence(
            string itemClass, int armour, int evasion, int elemental)
        {
            ItemDefence defence = Rules().DefenceOf(Piece("i", "chassis", itemClass, 10));

            Assert.AreEqual(armour, defence.Armour, 0.01f);
            Assert.AreEqual(evasion, defence.Evasion, 0.01f);
            Assert.AreEqual(elemental, defence.ElementalResistance, 0.01f);
        }

        /// <summary>
        /// slots.json: a hybrid takes 0.55 of each of two, so it adds up to 1.1 against the 1.0 of
        /// a pure class. The reward for covering two fronts is small on purpose.
        /// </summary>
        [Test]
        public void AHybridSplitsTheBudgetAndGetsSlightlyMoreInTotal()
        {
            ItemDefence defence = Rules().DefenceOf(Piece("i", "chassis", "medium", 10));

            Assert.AreEqual(66, defence.Armour, 0.01f, "120 at the 0.55 share.");
            Assert.AreEqual(99, defence.Evasion, 0.01f, "180 at the 0.55 share.");
        }

        /// <summary>slots.json: the head carries 0.6 of the defence the chest carries.</summary>
        [Test]
        public void ASlotCarriesItsOwnShareOfTheDefence()
        {
            Assert.AreEqual(72, Rules().DefenceOf(Piece("i", "cranialCasing", "heavy", 10)).Armour, 0.01f);
        }

        /// <summary>
        /// A weapon and the two utility slots carry no base defence at all. They exist to carry
        /// modifiers, which is the whole reason a Controller is worth wearing.
        /// </summary>
        [TestCase("mainHand")]
        [TestCase("controller")]
        [TestCase("firmware")]
        public void ASlotWithNoDefenceWeightGrantsNone(string slot)
        {
            ItemDefence defence = Rules().DefenceOf(Piece("i", slot, "heavy", 10));

            Assert.AreEqual(0, defence.Armour, 0.01f);
            Assert.AreEqual(0, defence.Evasion, 0.01f);
            Assert.AreEqual(0, defence.ElementalResistance, 0.01f);
        }

        // --- Activation ---

        private static EquipmentResolution Resolve(Equipment worn, int pow, int agi)
        {
            return worn.Resolve(Rules(), new[] { pow, agi, 0, 0 });
        }

        [Test]
        public void APieceWhoseRequirementIsMetIsActive()
        {
            Equipment worn = new Equipment();
            worn.Put(Piece("i", "chassis", "heavy", 10));

            EquipmentResolution resolved = Resolve(worn, 15, 0);

            Assert.AreEqual(1, resolved.Active.Count);
            Assert.IsEmpty(resolved.Inactive);
        }

        [Test]
        public void APieceWhoseRequirementIsNotMetIsInactive()
        {
            Equipment worn = new Equipment();
            worn.Put(Piece("i", "chassis", "heavy", 10));

            EquipmentResolution resolved = Resolve(worn, 14, 0);

            Assert.IsEmpty(resolved.Active);
            Assert.AreEqual(1, resolved.Inactive.Count);
        }

        /// <summary>
        /// An inactive piece is disregarded **whole**: no defence, no attribute, and no slice of
        /// equipment class. Half counting it would be the worst of both worlds — a player would
        /// see a red border and still be wearing part of the thing.
        /// </summary>
        [Test]
        public void AnInactivePieceContributesNothingAtAll()
        {
            Equipment worn = new Equipment();
            worn.Put(Piece("i", "chassis", "heavy", 10, Roll("con", 30f)));

            EquipmentResolution resolved = Resolve(worn, 0, 0);

            Assert.AreEqual(0, resolved.Totals.Armour);
            Assert.AreEqual(0, resolved.Totals.Of(Attribute.Constitution));
            Assert.IsEmpty(resolved.Classes);
        }

        /// <summary>
        /// A piece already switched on can pay for the next one, which is what "one at a time, in
        /// some order" means. The heavy piece needs 15 POW and grants the AGI the light one needs.
        /// </summary>
        [Test]
        public void AnActivePieceCanPayForAnother()
        {
            Equipment worn = new Equipment();
            worn.Put(Piece("heavy", "chassis", "heavy", 10, Roll("agi", 20f)));
            worn.Put(Piece("light", "cranialCasing", "light", 10));

            EquipmentResolution resolved = Resolve(worn, 15, 0);

            Assert.AreEqual(2, resolved.Active.Count, "The heavy piece switched on and paid for the light one.");
            Assert.IsEmpty(resolved.Inactive);
        }

        /// <summary>
        /// Two pieces that only satisfy each other stay **both** off, because there is no first one
        /// to put on. Sorting that out is the player's job.
        ///
        /// This is the case that decides the whole shape of the rule. Growing from nothing gives
        /// this answer; shrinking from everything would leave the pair switched on, in a state the
        /// player could never have reached by equipping one at a time.
        /// </summary>
        [Test]
        public void TwoPiecesThatOnlySatisfyEachOtherStayOff()
        {
            Equipment worn = new Equipment();
            worn.Put(Piece("heavy", "chassis", "heavy", 10, Roll("agi", 20f)));
            worn.Put(Piece("light", "cranialCasing", "light", 10, Roll("pow", 20f)));

            EquipmentResolution resolved = Resolve(worn, 0, 0);

            Assert.IsEmpty(resolved.Active);
            Assert.AreEqual(2, resolved.Inactive.Count);
        }

        /// <summary>And the same pair, with one attribute more, switches both on.</summary>
        [Test]
        public void TheSamePairSwitchesOnWithOneAttributeMore()
        {
            Equipment worn = new Equipment();
            worn.Put(Piece("heavy", "chassis", "heavy", 10, Roll("agi", 20f)));
            worn.Put(Piece("light", "cranialCasing", "light", 10, Roll("pow", 20f)));

            EquipmentResolution resolved = Resolve(worn, 15, 0);

            Assert.AreEqual(2, resolved.Active.Count);
        }

        // --- What the character ends up with ---

        private static CharacterStats Wearing(EquipmentTotals totals, int pow, int con, float multiplier)
        {
            CharacterStats stats = new CharacterStats
            {
                BasePower = pow,
                BaseConstitution = con,
                Equipment = EquipmentClass.Light
            };

            stats.ApplyInstance(1, new AttributeGrowth(), multiplier);
            stats.UseEquipment(EquipmentComposition.Of(EquipmentClass.Light), totals);
            return stats;
        }

        /// <summary>
        /// The roadmap's worked example: 20 from the sheet, an item giving 10, a stage multiplier
        /// of 1.5. The item lands **after** the multiplier, so the total is 40 and not 45.
        ///
        /// The multiplier exists for a stage to adjust a sheet, and equipment is not sheet. Letting
        /// it be multiplied would make a hard stage punish a well equipped hero more than a bare
        /// one — and, more generally, anything multiplied tends to run away from whoever wrote the
        /// multiplier.
        /// </summary>
        [Test]
        public void EquipmentLandsAfterTheStageMultiplier()
        {
            EquipmentTotals totals = new EquipmentTotals();
            totals.AddAttribute(Attribute.Power, 10);

            Assert.AreEqual(40, Wearing(totals, 20, 0, 1.5f).TotalOf(Attribute.Power));
        }

        /// <summary>
        /// attributes.md: `CON x 10 + vida vinda de outras fontes`. A character with 40 CON and an
        /// item granting 50 has 450.
        ///
        /// CON stays the only attribute that grants health. Equipment adds, it never converts.
        /// </summary>
        [Test]
        public void EquipmentAddsLifeOnTopOfConstitution()
        {
            EquipmentTotals totals = new EquipmentTotals();
            totals.AddLife(50);

            Assert.AreEqual(450, Wearing(totals, 0, 40, 1f).MaxHealth);
        }

        [Test]
        public void EquipmentAddsToEveryDefence()
        {
            EquipmentTotals totals = new EquipmentTotals();
            totals.AddArmour(100);
            totals.AddEvasion(60);
            totals.AddAllResistances(25);

            CharacterStats stats = Wearing(totals, 0, 0, 1f);

            Assert.AreEqual(100, stats.PhysicalArmor);
            Assert.AreEqual(60, stats.EvasionPoints);
            Assert.AreEqual(25, stats.FireResistance);
            Assert.AreEqual(25, stats.WaterResistance);
            Assert.AreEqual(25, stats.ElectricResistance);
        }

        /// <summary>
        /// Thorns is a share of the damage, so the sheet's own value and the equipment's simply
        /// add. Nothing outside the stats reads the sheet field on its own any more.
        /// </summary>
        [Test]
        public void EquipmentAddsToTheSheetsOwnThorns()
        {
            EquipmentTotals totals = new EquipmentTotals();
            totals.AddThornsPercent(4f);

            CharacterStats stats = Wearing(totals, 0, 0, 1f);
            stats.BaseThornsPercent = 10f;

            Assert.AreEqual(14f, stats.ThornsPercent, 0.001f);
        }

        // --- The seam that stops a modifier being forgotten ---

        /// <summary>
        /// Every modifier the data declares is either applied or knowingly waiting for a seam.
        ///
        /// Without this, a modifier added to `modifiers.json` that nobody wires up would roll onto
        /// items and do **nothing**, with no symptom at all beyond a player wondering why their
        /// item feels weak.
        /// </summary>
        [Test]
        public void EveryModifierInTheDataIsEitherAppliedOrKnowinglyWaiting()
        {
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Items/modifiers.json");
            Assert.IsNotNull(asset, "There is no Assets/Items/modifiers.json.");

            ItemModifiers modifiers = JsonUtility.FromJson<ItemModifiers>(asset.text);
            List<string> unaccounted = new List<string>();

            Check(modifiers.hardware, unaccounted);
            Check(modifiers.software, unaccounted);

            Assert.IsEmpty(unaccounted,
                "These modifiers exist in the data and nothing applies them:\n"
                + string.Join("\n", unaccounted));
        }

        private static void Check(ItemModifier[] list, List<string> unaccounted)
        {
            for (int i = 0; i < list.Length; i++)
            {
                string id = list[i].id;

                bool known = false;

                for (int h = 0; h < ItemContribution.Handled.Count && !known; h++)
                {
                    known = ItemContribution.Handled[h] == id;
                }

                for (int w = 0; w < ItemContribution.HandledByWeapon.Count && !known; w++)
                {
                    known = ItemContribution.HandledByWeapon[w] == id;
                }

                for (int d = 0; d < ItemContribution.Deferred.Count && !known; d++)
                {
                    known = ItemContribution.Deferred[d] == id;
                }

                if (!known)
                {
                    unaccounted.Add(id);
                }
            }
        }
    }
}
