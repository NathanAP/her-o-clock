using System.Collections.Generic;
using HerOClock.Abilities;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.Items;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// The ability damage percentages an item carries, per element.
    ///
    /// The numbers come from "### Dano de habilidade" in attributes.md, which carries the formula
    /// and the worked example: a fire ability of base 100 with `spe: 1.0`, on a character with 200
    /// SPE and 25% of fire ability damage, lands `100 x (1 + 0.2 + 0.25)` = 145.
    /// </summary>
    public class AbilityDamagePercentTests
    {
        private static ItemRules Rules()
        {
            TextAsset slots = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Items/slots.json");
            Assert.IsNotNull(slots, "There is no Assets/Items/slots.json.");

            return new ItemRules(JsonUtility.FromJson<ItemSlots>(slots.text));
        }

        /// <summary>
        /// A utility piece carrying one modifier. The controller and the firmware have no base
        /// defence at all, so nothing but the modifier reaches the character.
        /// </summary>
        private static Item Piece(string slot, string modifier, float value)
        {
            return new Item(
                slot, slot, "special", string.Empty, "conventional", 1,
                new List<ItemModifierRoll> { new ItemModifierRoll(modifier, "software", 1, value) });
        }

        /// <summary>
        /// A fire ability of the shape the spec's example describes, with the damage landing on
        /// whoever the ability targeted.
        /// </summary>
        private static AbilityDefinition FireBolt(float baseDamage, float specialtyWeight)
        {
            return TestAbility.Instant("fire-bolt").With(new AbilityEffect
            {
                Type = EffectType.DealDamage,
                Target = EffectTarget.EachTarget,
                DamageType = DamageType.Fire,
                Base = RankedValue.Constant(baseDamage),
                Scaling = new AbilityScaling { Specialty = specialtyWeight },
                Falloff = RankedValue.Constant(0f)
            });
        }

        /// <summary>
        /// Casts one ability at one target and returns the damage that landed.
        ///
        /// The target has no armour, no resistance and no evasion, so what comes out the far end is
        /// the base damage the resolver worked out — which is the only thing these tests are about.
        /// </summary>
        private static int Cast(TestBattle battle, Character user, Character target, AbilityDefinition ability)
        {
            int dealt = 0;

            AbilityResolver.Apply(
                user, ability, 0, new List<Character> { target }, battle.Grid, new BattleRandom(1),
                (u, t, result) => dealt += result.Damage);

            return dealt;
        }

        /// <summary>
        /// Builds a caster holding the given pieces, on a cell of its own.
        ///
        /// The column is explicit because a test comparing two casters would otherwise put both on
        /// the same cell, and the second one would claim a square the first is standing on.
        /// </summary>
        private static Character Caster(
            TestBattle battle, ItemRules rules, int column, int specialty, params Item[] pieces)
        {
            CharacterDefinition sheet = battle.Sheet(
                "caster", CharacterKind.Hero, specialty: specialty, constitution: 200);

            HeroRecord record = battle.Record(sheet);

            for (int i = 0; i < pieces.Length; i++)
            {
                record.Equipment.Put(pieces[i], rules);
            }

            return battle.SpawnHero(record, Team.Heroes, column, 0, 1f, rules);
        }

        // --- The example ---

        /// <summary>
        /// attributes.md, worked example: `1 + 200 x 0.001 + 25 / 100` is 1.45, so a base of 100
        /// lands 145.
        /// </summary>
        [Test]
        public void TheWorkedExampleOfTheSpec()
        {
            using (TestBattle battle = new TestBattle())
            {
                ItemRules rules = Rules();

                Character caster = Caster(battle, rules, 0, 200,
                    Piece("controller", "fireAbilityDamage", 10f),
                    Piece("firmware", "fireAbilityDamage", 15f));

                Character dummy = battle.Spawn(
                    battle.Sheet("dummy", CharacterKind.Minion, constitution: 200), Team.Enemies, 0, 4);

                Assert.AreEqual(200, caster.Stats.TotalOf(Attribute.Specialty));
                Assert.AreEqual(25f, caster.Stats.FireAbilityDamagePercent, 0.001f, "10 and 15 add to 25.");

                Assert.AreEqual(145, Cast(battle, caster, dummy, FireBolt(100f, 1f)));
            }
        }

        /// <summary>
        /// The percentage joins the **same sum** as the attribute share instead of multiplying it.
        ///
        /// That is what makes 1% of ability damage worth exactly ten attribute points, so the two
        /// are one currency the player can trade between. Multiplying would have given
        /// `100 x 1.2 x 1.25` = 150, and each point would be worth more the more items were on.
        /// </summary>
        [Test]
        public void ThePercentageAddsToTheAttributeShareRatherThanMultiplyingIt()
        {
            using (TestBattle battle = new TestBattle())
            {
                ItemRules rules = Rules();

                Character caster = Caster(battle, rules, 0, 200,
                    Piece("controller", "fireAbilityDamage", 25f));

                Character dummy = battle.Spawn(
                    battle.Sheet("dummy", CharacterKind.Minion, constitution: 200), Team.Enemies, 0, 4);

                int dealt = Cast(battle, caster, dummy, FireBolt(100f, 1f));

                Assert.AreEqual(145, dealt);
                Assert.AreNotEqual(150, dealt, "150 is the answer multiplying would have given.");
            }
        }

        /// <summary>
        /// Ten attribute points and one percent are the same thing, which is the property the
        /// whole ordering was chosen for.
        /// </summary>
        [Test]
        public void TenPointsOfAttributeAreWorthOnePercent()
        {
            using (TestBattle battle = new TestBattle())
            {
                ItemRules rules = Rules();

                Character byAttribute = Caster(battle, rules, 0, 250);
                Character byItem = Caster(battle, rules, 1, 200,
                    Piece("controller", "fireAbilityDamage", 5f));

                Character dummy = battle.Spawn(
                    battle.Sheet("dummy", CharacterKind.Minion, constitution: 200), Team.Enemies, 0, 4);

                Assert.AreEqual(
                    Cast(battle, byAttribute, dummy, FireBolt(100f, 1f)),
                    Cast(battle, byItem, dummy, FireBolt(100f, 1f)),
                    "50 extra SPE and 5% of ability damage have to be worth the same.");
            }
        }

        // --- One element at a time ---

        /// <summary>
        /// The percentages that apply are chosen by the effect's own element. A fire effect gets
        /// nothing from investment in electric, so only the attribute share is left.
        /// </summary>
        [Test]
        public void OnlyTheElementOfTheEffectCounts()
        {
            using (TestBattle battle = new TestBattle())
            {
                ItemRules rules = Rules();

                Character caster = Caster(battle, rules, 0, 200,
                    Piece("controller", "electricAbilityDamage", 25f));

                Character dummy = battle.Spawn(
                    battle.Sheet("dummy", CharacterKind.Minion, constitution: 200), Team.Enemies, 0, 4);

                Assert.AreEqual(120, Cast(battle, caster, dummy, FireBolt(100f, 1f)),
                    "Only the 200 SPE counts, so the multiplier is 1.2.");
            }
        }

        // --- The cost of a costly ability ---

        /// <summary>
        /// It applies to the damage an ability does to its own user.
        ///
        /// That is the point rather than an oversight: a hero stacking fire pays more for a fire
        /// ability that burns them, and the way out is fire resistance and health. The cost becomes
        /// a build decision instead of a flat tax.
        ///
        /// The effect carries no scaling at all, exactly as Gadrat's Warm Up does, so the
        /// percentage is the only thing raising it.
        /// </summary>
        [Test]
        public void ItAppliesToTheDamageAnAbilityDoesToItsOwnUser()
        {
            using (TestBattle battle = new TestBattle())
            {
                ItemRules rules = Rules();

                Character caster = Caster(battle, rules, 0, 200,
                    Piece("controller", "fireAbilityDamage", 25f));

                AbilityDefinition burn = TestAbility.Instant("warm-up").With(new AbilityEffect
                {
                    Type = EffectType.DealDamage,
                    Target = EffectTarget.Self,
                    DamageType = DamageType.Fire,
                    Base = RankedValue.Constant(24f),
                    Scaling = new AbilityScaling(),
                    Falloff = RankedValue.Constant(0f)
                });

                int before = caster.CurrentHealth;

                AbilityResolver.Apply(
                    caster, burn, 0, new List<Character>(), battle.Grid, new BattleRandom(1), null);

                Assert.AreEqual(30, before - caster.CurrentHealth, "24 x 1.25, with no scaling to help.");
            }
        }

        // --- What it must never touch ---

        /// <summary>
        /// It never reaches the basic attack, whatever the element.
        ///
        /// What raises a basic attack is the weapon's own base damage. Keeping the two families
        /// apart is what makes a weapon build and an ability build look for different items.
        /// </summary>
        [Test]
        public void ItNeverTouchesTheBasicAttack()
        {
            using (TestBattle battle = new TestBattle())
            {
                ItemRules rules = Rules();

                Character bare = Caster(battle, rules, 0, 200);
                Character loaded = Caster(battle, rules, 1, 200,
                    Piece("controller", "physicalAbilityDamage", 100f),
                    Piece("firmware", "fireAbilityDamage", 100f));

                Assert.AreEqual(100f, loaded.Stats.PhysicalAbilityDamagePercent, 0.001f,
                    "The modifier really is on the character.");

                Assert.AreEqual(bare.Stats.PhysicalDamageMin, loaded.Stats.PhysicalDamageMin, 0.001f);
                Assert.AreEqual(bare.Stats.PhysicalDamageMax, loaded.Stats.PhysicalDamageMax, 0.001f);
                Assert.AreEqual(bare.Stats.AttacksPerSecond, loaded.Stats.AttacksPerSecond, 0.001f);
            }
        }

        /// <summary>
        /// A character with nothing equipped gets nothing, which is every minion and villain.
        /// </summary>
        [Test]
        public void ACharacterWithNoItemsGetsNothing()
        {
            using (TestBattle battle = new TestBattle())
            {
                Character minion = battle.Spawn(
                    battle.Sheet("minion", CharacterKind.Minion), Team.Enemies, 0, 4);

                Assert.AreEqual(0f, minion.Stats.FireAbilityDamagePercent, 0.0001f);
                Assert.AreEqual(0f, minion.Stats.WaterAbilityDamagePercent, 0.0001f);
                Assert.AreEqual(0f, minion.Stats.ElectricAbilityDamagePercent, 0.0001f);
                Assert.AreEqual(0f, minion.Stats.PhysicalAbilityDamagePercent, 0.0001f);
            }
        }
    }
}
