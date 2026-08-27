using System.Collections.Generic;
using HerOClock.Characters;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// Checks how a set of worn equipment turns into the one class number the four per class
    /// formulas need, against "A classe do personagem" in attributes.md.
    ///
    /// Every expected value here was copied from the spec's own table and worked example, never
    /// from running the code.
    /// </summary>
    public class EquipmentCompositionTests
    {
        private const float Tolerance = 0.01f;

        /// <summary>The three evasion constants of attributes.md, in Blend's argument order.</summary>
        private static float Evasion(EquipmentComposition composition)
        {
            return composition.Blend(100f, 200f, 500f);
        }

        private static EquipmentComposition Worn(params ItemClass[] classes)
        {
            return EquipmentComposition.Of(new List<ItemClass>(classes), EquipmentClass.Light);
        }

        /// <summary>
        /// attributes.md: a character in a single pure class lands exactly on that class's number.
        ///
        /// This is the case the whole game ran on before mixing existed, so it is also the reason
        /// the balance snapshot did not move when it arrived.
        /// </summary>
        [TestCase(ItemClass.Light, 100f)]
        [TestCase(ItemClass.Special, 200f)]
        [TestCase(ItemClass.Heavy, 500f)]
        public void APureClassLandsOnItsOwnNumber(ItemClass worn, float expected)
        {
            Assert.AreEqual(expected, Evasion(Worn(worn, worn, worn)), Tolerance);
        }

        /// <summary>
        /// attributes.md: a hybrid hands in half a slice to each of its two sides. So one medium
        /// item is worth exactly one light plus one heavy, and both land halfway between 100 and
        /// 500.
        /// </summary>
        [Test]
        public void AHybridIsHalfOfEachOfItsTwoSides()
        {
            Assert.AreEqual(300f, Evasion(Worn(ItemClass.Medium)), Tolerance);
            Assert.AreEqual(300f, Evasion(Worn(ItemClass.Light, ItemClass.Heavy)), Tolerance);
        }

        [TestCase(ItemClass.Medium, 100f, 500f)]
        [TestCase(ItemClass.LightSpecial, 100f, 200f)]
        [TestCase(ItemClass.HeavySpecial, 500f, 200f)]
        public void EveryHybridSitsBetweenTheTwoItIsMadeOf(ItemClass hybrid, float first, float second)
        {
            Assert.AreEqual((first + second) / 2f, Evasion(Worn(hybrid)), Tolerance);
        }

        /// <summary>
        /// attributes.md: an empty slot does not count. A character wearing two items is the mix
        /// of those two, and not a mix of two items and six holes.
        ///
        /// Without this the rule would be unwriteable, because an empty slot has no class to
        /// contribute and any value invented for it would be a seventh class nobody declared.
        /// </summary>
        [Test]
        public void AnEmptySlotDoesNotDiluteTheMix()
        {
            EquipmentComposition two = Worn(ItemClass.Light, ItemClass.Heavy);

            Assert.AreEqual(2f, two.Total, Tolerance);
            Assert.AreEqual(300f, Evasion(two), Tolerance);
        }

        /// <summary>
        /// attributes.md: a character with nothing worn uses the class declared on its sheet.
        ///
        /// This is every minion, villain and NPC, since none of them ever wears anything, and a
        /// hero before it has items.
        /// </summary>
        [TestCase(EquipmentClass.Light, 100f)]
        [TestCase(EquipmentClass.Special, 200f)]
        [TestCase(EquipmentClass.Heavy, 500f)]
        public void WearingNothingFallsBackToTheSheetClass(EquipmentClass sheet, float expected)
        {
            Assert.AreEqual(expected, Evasion(EquipmentComposition.Of(null, sheet)), Tolerance);
            Assert.AreEqual(expected, Evasion(EquipmentComposition.Of(new List<ItemClass>(), sheet)), Tolerance);
        }

        /// <summary>
        /// The worked example of attributes.md: two heavy casings, two light casings, one medium
        /// weapon and one special controller. The slices come to 2.5 light, 1 special and 2.5
        /// heavy, over a total of 6.
        /// </summary>
        private static EquipmentComposition SpecExample()
        {
            return Worn(
                ItemClass.Heavy,
                ItemClass.Heavy,
                ItemClass.Light,
                ItemClass.Light,
                ItemClass.Medium,
                ItemClass.Special);
        }

        [Test]
        public void TheSpecExampleHandsInTheSlicesTheSpecSays()
        {
            EquipmentComposition mixed = SpecExample();

            Assert.AreEqual(6f, mixed.Total, Tolerance);
            Assert.AreEqual(2.5f, mixed.ShareOf(EquipmentClass.Light), Tolerance);
            Assert.AreEqual(1f, mixed.ShareOf(EquipmentClass.Special), Tolerance);
            Assert.AreEqual(2.5f, mixed.ShareOf(EquipmentClass.Heavy), Tolerance);
        }

        /// <summary>
        /// The four results of the spec's example table. They are four separate numbers on
        /// purpose: one rule has to answer all four formulas, and a mistake that only shows up in
        /// one of them is exactly what this catches.
        /// </summary>
        [Test]
        public void TheSpecExampleProducesTheFourNumbersInItsTable()
        {
            EquipmentComposition mixed = SpecExample();

            Assert.AreEqual(283.33f, mixed.Blend(100f, 200f, 500f), Tolerance);
            Assert.AreEqual(156.67f, mixed.Blend(60f, 40f, 300f), Tolerance);
            Assert.AreEqual(0.005833f, mixed.Blend(0.01f, 0.005f, 0.002f), 0.000001f);
            Assert.AreEqual(0.0075f, mixed.Blend(0.01f, 0.0075f, 0.005f), 0.000001f);
        }

        /// <summary>
        /// attributes.md: what is mixed is the constant, never the result, and the consequence is
        /// that mixing pays a little less than averaging the results would.
        ///
        /// The spec's own example is the case: mixing the constants gives 26.09% of evasion at 100
        /// AGI, while averaging the three results by the same slices would give 33.3%. The test
        /// exists so that swapping the two — which looks harmless and reads almost the same — is
        /// caught rather than quietly rebalancing every character in the game.
        /// </summary>
        [Test]
        public void MixingTheConstantPaysLessThanMixingTheResults()
        {
            EquipmentComposition mixed = SpecExample();

            float constant = mixed.Blend(100f, 200f, 500f);
            float fromConstant = 100f * 100f / (100f + constant);

            float fromResults = mixed.Blend(
                100f * 100f / (100f + 100f),
                100f * 100f / (100f + 200f),
                100f * 100f / (100f + 500f));

            Assert.AreEqual(26.09f, fromConstant, Tolerance);
            Assert.Less(fromConstant, fromResults);
        }
    }
}
