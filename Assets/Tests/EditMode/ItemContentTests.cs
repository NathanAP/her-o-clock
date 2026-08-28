using System.Collections.Generic;
using HerOClock.Items;
using HerOClock.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// The item data files, checked against the rules items.md states about them.
    ///
    /// Until this existed, **not one line of those files was checked by anything**. They were
    /// written months before any code read them, which is exactly the situation where a table
    /// stops adding up and nobody notices: the invariants in `tiers.json` were prose sitting
    /// beside the numbers they described, and the damage budget derivation had been verified once,
    /// by hand.
    ///
    /// Nothing here reaches combat, so nothing here can move the balance snapshot.
    /// </summary>
    public class ItemContentTests
    {
        private const string Folder = "Assets/Items/";

        private static T Read<T>(string file) where T : class
        {
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(Folder + file);

            Assert.IsNotNull(asset, "There is no " + Folder + file + ".");

            T data = JsonUtility.FromJson<T>(asset.text);

            Assert.IsNotNull(data, Folder + file + " could not be read as " + typeof(T).Name + ".");
            return data;
        }

        private static ItemSlots Slots()
        {
            return Read<ItemSlots>("slots.json");
        }

        private static ItemSubtypes Subtypes()
        {
            return Read<ItemSubtypes>("subtypes.json");
        }

        private static ItemModifiers Modifiers()
        {
            return Read<ItemModifiers>("modifiers.json");
        }

        private static ItemTiers Tiers()
        {
            return Read<ItemTiers>("tiers.json");
        }

        private static ItemUniqueRules Uniques()
        {
            return Read<ItemUniqueRules>("uniques.json");
        }

        // --- The files as a set ---

        [Test]
        public void TheValidatorFindsNothingWrongWithTheRealFiles()
        {
            List<string> problems = ItemValidator.Validate(
                Slots(), Subtypes(), Modifiers(), Tiers(), Uniques());

            Assert.IsEmpty(problems, string.Join("\n", problems));
        }

        /// <summary>
        /// The validator has to actually refuse something, or a green run above would mean nothing.
        /// </summary>
        [Test]
        public void TheValidatorRefusesAModifierThatFallsOnNothing()
        {
            ItemModifiers modifiers = Modifiers();
            modifiers.hardware[0].slots = "aGroupThatDoesNotExist";

            List<string> problems = ItemValidator.Validate(
                Slots(), Subtypes(), modifiers, Tiers(), Uniques());

            Assert.IsNotEmpty(problems);
            StringAssert.Contains("aGroupThatDoesNotExist", string.Join("\n", problems));
        }

        // --- tiers.json, against the invariants written inside it ---

        /// <summary>tiers.json: "Toda linha soma exatamente 100."</summary>
        [Test]
        public void EveryTierRowSumsToOneHundred()
        {
            TierChances[] rows = Tiers().distribution.byItemLevel;

            for (int i = 0; i < rows.Length; i++)
            {
                Assert.AreEqual(100f, rows[i].Total, 0.001f,
                    "The row for item level " + rows[i].itemLevel + " does not sum to 100.");
            }
        }

        /// <summary>
        /// tiers.json: "A camada 1 cai de forma monotonica" and "As camadas 3, 4 e 5 sobem de
        /// forma monotonica".
        ///
        /// These two are what make a higher item level actually mean better modifiers. Without
        /// them the table could drift into a shape where levelling made drops worse and the only
        /// symptom would be a feeling.
        /// </summary>
        [TestCase(1, -1)]
        [TestCase(3, 1)]
        [TestCase(4, 1)]
        [TestCase(5, 1)]
        public void TheOuterTiersMoveInOneDirectionOnly(int tier, int direction)
        {
            TierChances[] rows = Tiers().distribution.byItemLevel;

            for (int i = 1; i < rows.Length; i++)
            {
                float previous = rows[i - 1].Of(tier);
                float current = rows[i].Of(tier);

                if (direction < 0)
                {
                    Assert.Less(current, previous,
                        "Tier " + tier + " rose from item level " + rows[i - 1].itemLevel
                        + " to " + rows[i].itemLevel + ".");
                }
                else
                {
                    Assert.Greater(current, previous,
                        "Tier " + tier + " fell from item level " + rows[i - 1].itemLevel
                        + " to " + rows[i].itemLevel + ".");
                }
            }
        }

        /// <summary>
        /// tiers.json: tier 2 is unimodal — it rises, may hold at its peak, and then falls, but
        /// never falls and rises again.
        ///
        /// The flat top is allowed on purpose: item levels 40 and 60 both sit at 34, and two
        /// neighbouring rows at the same value are the turning point rather than a defect. What
        /// the rule forbids is a dip in the middle, which would mean a range of item levels where
        /// improving the item made this tier rarer and then common again for no reason.
        /// </summary>
        [Test]
        public void TheMiddleTierIsUnimodal()
        {
            TierChances[] rows = Tiers().distribution.byItemLevel;

            bool falling = false;
            bool rose = false;
            bool fell = false;

            for (int i = 1; i < rows.Length; i++)
            {
                float previous = rows[i - 1].tier2;
                float current = rows[i].tier2;

                if (current > previous)
                {
                    Assert.IsFalse(falling,
                        "Tier 2 rises again at item level " + rows[i].itemLevel + " after having fallen.");

                    rose = true;
                }
                else if (current < previous)
                {
                    falling = true;
                    fell = true;
                }
            }

            Assert.IsTrue(rose, "Tier 2 never rises, so it is not the middle tier the file describes.");
            Assert.IsTrue(fell, "Tier 2 never gives way to the tiers above it.");
        }

        // --- subtypes.json, against the damage budget it declares ---

        /// <summary>
        /// items.md: "A faixa de dano de cada subtipo e derivada de um orcamento de dano por
        /// segundo, e nao escolhida solta." Average damage times attack speed equals the family's
        /// budget, within the tolerance the file states.
        ///
        /// Two discounts apply on top, and both are checked here rather than trusted:
        ///
        /// - a **special** weapon pays one, because whoever carries it does not fight with the
        ///   basic attack;
        /// - a **melee family weapon that reaches past one cell** pays another, because it strikes
        ///   from a distance without giving up striking up close, and that has no downside.
        ///
        /// It is checked at level 1 and at level 100 because a derivation that only holds at the
        /// bottom would let a subtype quietly outgrow its family.
        /// </summary>
        [TestCase(1)]
        [TestCase(100)]
        public void EveryWeaponMeetsItsFamilyBudget(int level)
        {
            ItemSubtypes subtypes = Subtypes();
            DamageBudget budget = subtypes.damageBudget;

            for (int i = 0; i < subtypes.weapons.Length; i++)
            {
                WeaponSubtype weapon = subtypes.weapons[i];
                FamilyBudget family = FamilyOf(budget, weapon.family);

                float allowed = family.At(level);

                if (weapon.naturalClass == "special")
                {
                    allowed *= budget.specialMultiplier;
                }

                if (weapon.family.Contains("Melee") && weapon.maxRange > 1)
                {
                    allowed *= budget.reachMultiplier;
                }

                float actual = weapon.damage.AverageAt(level) * weapon.attackSpeed;

                Assert.AreEqual(1f, actual / allowed, budget.tolerance,
                    weapon.id + " is worth " + actual + " damage per second at level " + level
                    + ", against a budget of " + allowed + ".");
            }
        }

        private static FamilyBudget FamilyOf(DamageBudget budget, string id)
        {
            for (int i = 0; i < budget.families.Length; i++)
            {
                if (budget.families[i].id == id)
                {
                    return budget.families[i];
                }
            }

            Assert.Fail("There is no budget for the family " + id + ".");
            return null;
        }

        // --- The strings, which are the only place a word the player reads may live ---

        private static StringTable Strings()
        {
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(Folder + "itemStrings.json");
            Assert.IsNotNull(asset, "There is no " + Folder + "itemStrings.json.");

            List<string> problems = new List<string>();
            StringTable table = StringTable.From(JsonUtility.FromJson<StringTableData>(asset.text), problems);

            Assert.IsEmpty(problems, string.Join("\n", problems));
            return table;
        }

        /// <summary>
        /// Every id the player can end up reading has a word for it.
        ///
        /// A missing entry is invisible until an item drops with a blank where its name should be,
        /// which is exactly the kind of thing that reaches a build.
        /// </summary>
        [Test]
        public void EveryIdThePlayerReadsHasText()
        {
            StringTable strings = Strings();
            List<string> missing = new List<string>();

            ItemSlots slots = Slots();

            for (int i = 0; i < slots.slots.Length; i++)
            {
                Require(strings, "item.slot." + slots.slots[i].id + ".name", missing);
            }

            for (int i = 0; i < slots.classDefence.Length; i++)
            {
                Require(strings, "item.class." + slots.classDefence[i].id + ".name", missing);
            }

            ItemSubtypes subtypes = Subtypes();

            for (int i = 0; i < subtypes.weapons.Length; i++)
            {
                Require(strings, "item.subtype." + subtypes.weapons[i].id + ".name", missing);

                for (int b = 0; b < subtypes.weapons[i].bases.Length; b++)
                {
                    Require(strings, "item.base." + subtypes.weapons[i].bases[b].id + ".name", missing);
                }
            }

            for (int i = 0; i < subtypes.offHandDefensive.Length; i++)
            {
                Require(strings, "item.subtype." + subtypes.offHandDefensive[i].id + ".name", missing);

                for (int b = 0; b < subtypes.offHandDefensive[i].bases.Length; b++)
                {
                    Require(strings, "item.base." + subtypes.offHandDefensive[i].bases[b].id + ".name", missing);
                }
            }

            TechnologyStep[] technologies = Tiers().technology.steps;

            for (int i = 0; i < technologies.Length; i++)
            {
                Require(strings, "item.technology." + technologies[i].id + ".name", missing);
            }

            Assert.IsEmpty(missing, "These keys have no text:\n" + string.Join("\n", missing));
        }

        private static void Require(StringTable strings, string key, List<string> missing)
        {
            if (!strings.Has(key))
            {
                missing.Add(key);
            }
        }

        // --- The catalogue ---

        /// <summary>
        /// The database resolves every file. It is the same guard the stage database has, and it
        /// exists because an empty reference would otherwise surface as content that simply is not
        /// there.
        /// </summary>
        [Test]
        public void TheDatabaseResolvesEveryFile()
        {
            string[] found = AssetDatabase.FindAssets("t:ItemDatabase");

            Assert.AreEqual(1, found.Length,
                "Expected exactly one ItemDatabase in the project, found " + found.Length + ".");

            ItemDatabase database = AssetDatabase.LoadAssetAtPath<ItemDatabase>(
                AssetDatabase.GUIDToAssetPath(found[0]));

            Assert.IsNotNull(database.LoadSlots(), "The slots entry does not resolve.");
            Assert.IsNotNull(database.LoadSubtypes(), "The subtypes entry does not resolve.");
            Assert.IsNotNull(database.LoadModifiers(), "The modifiers entry does not resolve.");
            Assert.IsNotNull(database.LoadTiers(), "The tiers entry does not resolve.");
            Assert.IsNotNull(database.LoadUniqueRules(), "The uniques entry does not resolve.");
            Assert.IsNotNull(database.Strings, "The strings entry is empty.");
        }
    }
}
