using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using HerOClock.Items;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Attribute = HerOClock.Characters.Attribute;

namespace HerOClock.Tests
{
    /// <summary>
    /// The weapon: what it replaces on whoever holds it, and what it does not.
    ///
    /// The numbers are repeated here from `Assets/Items/subtypes.json` and from the worked examples
    /// in `items.md`, rather than read out of the files. That duplication is the whole point — two
    /// independent statements of the same value, so a disagreement shows up instead of one of them
    /// quietly winning.
    /// </summary>
    public class WeaponTests
    {
        // --- subtypes.json, read once and written down here ---
        //
        // blade:   1 hand,  reach 1-1, speed 1.3,  damage 6-8 at level 1, +0.6 / +0.8 per level.
        // claw:    1 hand,  reach 1-1, speed 1.7,  damage 5-6 at level 1.
        // ram:     1 hand,  reach 1-1, speed 0.8,  damage 7-15 at level 1.
        // cannon:  2 hands, reach 3-8, speed 0.45, damage 14-35 at level 1.
        // emitter: 1 hand,  reach 1-4, speed 1.2,  one handed **ranged** family.

        private static ItemRules Rules()
        {
            TextAsset slots = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Items/slots.json");
            TextAsset subtypes = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Items/subtypes.json");

            Assert.IsNotNull(slots, "There is no Assets/Items/slots.json.");
            Assert.IsNotNull(subtypes, "There is no Assets/Items/subtypes.json.");

            return new ItemRules(
                JsonUtility.FromJson<ItemSlots>(slots.text),
                JsonUtility.FromJson<ItemSubtypes>(subtypes.text));
        }

        private static Item Weapon(
            string slot, string subtype, string itemClass, int level, params ItemModifierRoll[] modifiers)
        {
            return new Item(
                slot + "-" + subtype, slot, itemClass, subtype, "conventional", level,
                new List<ItemModifierRoll>(modifiers));
        }

        // --- What a subtype is worth ---

        /// <summary>
        /// The four numbers of a subtype reach the rules untouched. Everything below depends on
        /// this one being right, so it is asserted on its own rather than through a character.
        /// </summary>
        [Test]
        public void ASubtypeHandsOverItsFourNumbers()
        {
            ItemWeapon blade = Rules().WeaponOf(Weapon(ItemRules.MainHandSlot, "blade", "light", 1));

            Assert.IsTrue(blade.Exists);
            Assert.AreEqual(1, blade.Hands);
            Assert.AreEqual(1, blade.MinRange);
            Assert.AreEqual(1, blade.MaxRange);
            Assert.AreEqual(1.3f, blade.AttackSpeed, 0.0001f);
            Assert.AreEqual(6f, blade.MinDamage, 0.0001f);
            Assert.AreEqual(8f, blade.MaxDamage, 0.0001f);
        }

        /// <summary>
        /// items.md: the range grows with the **item's** level and never with the holder's. A level
        /// 11 Blade is 6 + 0.6 x 10 and 8 + 0.8 x 10.
        /// </summary>
        [Test]
        public void AWeaponRangeGrowsWithTheItemLevel()
        {
            ItemWeapon blade = Rules().WeaponOf(Weapon(ItemRules.MainHandSlot, "blade", "light", 11));

            Assert.AreEqual(12f, blade.MinDamage, 0.0001f);
            Assert.AreEqual(16f, blade.MaxDamage, 0.0001f);
        }

        /// <summary>
        /// items.md: only the two ranged families draw a bolt. A melee weapon reaching two or three
        /// cells is still somebody swinging from further away.
        /// </summary>
        [TestCase("cannon", true)]
        [TestCase("emitter", true)]
        [TestCase("blade", false)]
        [TestCase("lance", false)]
        public void OnlyARangedFamilyDrawsABolt(string subtype, bool ranged)
        {
            ItemWeapon weapon = Rules().WeaponOf(Weapon(ItemRules.MainHandSlot, subtype, "heavy", 1));

            Assert.AreEqual(ranged, weapon.Ranged);
        }

        /// <summary>Nothing that is not a weapon is one, including the defensive off hands.</summary>
        [TestCase("chassis", "")]
        [TestCase("offHand", "bulwark")]
        [TestCase("controller", "")]
        public void WhatIsNotAWeaponIsNotAWeapon(string slot, string subtype)
        {
            Assert.IsFalse(Rules().WeaponOf(Weapon(slot, subtype, "heavy", 1)).Exists);
        }

        // --- The two hands ---

        /// <summary>
        /// items.md: equipping a two handed weapon takes off what was in **both** hands.
        /// </summary>
        [Test]
        public void ATwoHandedWeaponEmptiesBothHands()
        {
            ItemRules rules = Rules();
            Equipment worn = new Equipment();

            worn.Put(Weapon(ItemRules.MainHandSlot, "blade", "light", 1), rules);
            worn.Put(Weapon(ItemRules.OffHandSlot, "claw", "light", 1), rules);

            worn.Put(Weapon(ItemRules.MainHandSlot, "cannon", "heavy", 1), rules);

            Assert.AreEqual("cannon", worn.In(ItemRules.MainHandSlot).SubtypeId);
            Assert.IsNull(worn.In(ItemRules.OffHandSlot), "The off hand kept an item under a two hander.");
        }

        /// <summary>
        /// And from the other side: putting anything in the off hand takes the two hander off. An
        /// invariant that only holds when approached from one direction is not an invariant.
        /// </summary>
        [Test]
        public void FillingTheOffHandTakesOffATwoHandedWeapon()
        {
            ItemRules rules = Rules();
            Equipment worn = new Equipment();

            worn.Put(Weapon(ItemRules.MainHandSlot, "cannon", "heavy", 1), rules);
            worn.Put(Weapon(ItemRules.OffHandSlot, "bulwark", "heavy", 1), rules);

            Assert.IsNull(worn.In(ItemRules.MainHandSlot), "The two hander survived the off hand being filled.");
            Assert.AreEqual("bulwark", worn.In(ItemRules.OffHandSlot).SubtypeId);
        }

        /// <summary>
        /// The rule is asked again when the fight is built, because that is the last place it can be
        /// enforced on a set that arrived from a save rather than through equipping.
        ///
        /// The off hand lands in the inactive list rather than vanishing: that list is what the
        /// interface paints red, so the player is told instead of the piece silently doing nothing.
        /// </summary>
        [Test]
        public void ResolutionRefusesAnOffHandHeldUnderATwoHander()
        {
            ItemRules rules = Rules();
            Equipment worn = new Equipment();

            // Put without the rules, which is how a set loaded from disk arrives.
            worn.Put(Weapon(ItemRules.MainHandSlot, "cannon", "heavy", 1));
            worn.Put(Weapon(ItemRules.OffHandSlot, "bulwark", "heavy", 1));

            EquipmentResolution resolved = worn.Resolve(rules, new[] { 1000, 1000, 1000, 1000 });

            Assert.AreEqual(1, resolved.Active.Count, "Both hands counted under a two handed weapon.");
            Assert.AreEqual("cannon", resolved.Active[0].SubtypeId);
            Assert.AreEqual(1, resolved.Inactive.Count);
            Assert.AreEqual("bulwark", resolved.Inactive[0].SubtypeId);
        }

        // --- What the character ends up with ---

        /// <summary>
        /// A hero holding the given pieces.
        ///
        /// The AGI handed in has to clear the requirement of whatever is being worn, or the test
        /// would be measuring an inactive item and passing for the wrong reason. A level 1 piece
        /// asks for 2 points, so 50 is comfortable, and POW is left at zero everywhere the exact
        /// damage is asserted — POW multiplies it, and 0 is the only value that leaves the weapon's
        /// own range readable.
        ///
        /// The pieces are light for the same reason: a light Ram is a legal item on purpose, since
        /// `items.md` calls the natural class a tendency and never a lock.
        /// </summary>
        private static CharacterStats Armed(
            TestBattle battle, ItemRules rules, EquipmentClass sheetClass, int agility, params Item[] items)
        {
            CharacterDefinition sheet = battle.Sheet(
                "armed", CharacterKind.Hero, agility: agility, equipment: sheetClass, baseDamage: 10);

            HeroRecord record = battle.Record(sheet);

            for (int i = 0; i < items.Length; i++)
            {
                record.Equipment.Put(items[i], rules);
            }

            return battle.SpawnHero(record, Team.Heroes, 0, 0, 1f, rules).Stats;
        }

        /// <summary>
        /// items.md, worked example: a light hero with 20 AGI holding a Claw attacks
        /// `1.7 x (1 + 20 x 0.01)` times a second.
        ///
        /// **The weapon replaces the base, never the result** — AGI still multiplies, which is what
        /// keeps the light class worth building for somebody holding a weapon.
        /// </summary>
        [Test]
        public void AWeaponReplacesTheBaseSpeedAndAgilityStillMultipliesIt()
        {
            using (TestBattle battle = new TestBattle())
            {
                ItemRules rules = Rules();

                CharacterStats bare = Armed(battle, rules, EquipmentClass.Light, 20);
                Assert.AreEqual(1.2f, bare.AttacksPerSecond, 0.001f, "1 x (1 + 20 x 0.01).");

                // A light weapon, so the class mix stays light and the AGI rate stays 1%.
                CharacterStats armed = Armed(battle, rules, EquipmentClass.Light, 20,
                    Weapon(ItemRules.MainHandSlot, "claw", "light", 1));

                Assert.AreEqual(2.04f, armed.AttacksPerSecond, 0.001f, "1.7 x (1 + 20 x 0.01).");
            }
        }

        /// <summary>
        /// The reach comes from the weapon and not from the sheet, which is the bug this version
        /// exists to kill: the sheet is a shared asset, so two heroes built from the same file were
        /// forced to reach the same distance whatever they were holding.
        /// </summary>
        [Test]
        public void TwoHeroesFromOneSheetReachDifferentDistances()
        {
            using (TestBattle battle = new TestBattle())
            {
                ItemRules rules = Rules();

                CharacterDefinition sheet = battle.Sheet(
                    "shared", CharacterKind.Hero, power: 200, agility: 200, minRange: 1, maxRange: 1);

                HeroRecord melee = battle.Record(sheet);
                melee.Equipment.Put(Weapon(ItemRules.MainHandSlot, "blade", "light", 1), rules);

                HeroRecord gunner = battle.Record(sheet);
                gunner.Equipment.Put(Weapon(ItemRules.MainHandSlot, "cannon", "heavy", 1), rules);

                Character withBlade = battle.SpawnHero(melee, Team.Heroes, 0, 0, 1f, rules);
                Character withCannon = battle.SpawnHero(gunner, Team.Heroes, 1, 0, 1f, rules);

                Assert.AreEqual(1, withBlade.MinRange);
                Assert.AreEqual(1, withBlade.MaxRange);
                Assert.AreEqual(AutoAttackType.Melee, withBlade.AutoAttack);

                Assert.AreEqual(3, withCannon.MinRange, "The Cannon cannot hit anything adjacent.");
                Assert.AreEqual(8, withCannon.MaxRange, "The Cannon covers the whole board on purpose.");
                Assert.AreEqual(AutoAttackType.Ranged, withCannon.AutoAttack);
            }
        }

        /// <summary>
        /// A weapon whose requirement is not met is disregarded whole, and the hero falls back to
        /// the punch on its sheet. That is why the punch still exists there now that weapons do.
        /// </summary>
        [Test]
        public void AnInactiveWeaponFallsBackToThePunch()
        {
            using (TestBattle battle = new TestBattle())
            {
                ItemRules rules = Rules();

                // slots.json asks 1.5 of the class attribute per item level, so a level 60 heavy
                // weapon demands 90 POW, and this hero has none of it.
                CharacterStats stats = Armed(battle, rules, EquipmentClass.Light, 0,
                    Weapon(ItemRules.MainHandSlot, "ram", "heavy", 60));

                Assert.AreEqual(1f, stats.AttacksPerSecond, 0.001f, "The sheet's own speed of 1.");
                Assert.AreEqual(10f, stats.PhysicalDamageMin, 0.001f, "The sheet's own punch.");
                Assert.AreEqual(1, stats.HandCount);
            }
        }

        /// <summary>
        /// items.md: an empty main hand is no weapon at all, even with one in the off hand. Speed
        /// and reach come from the main hand, so there is nowhere to take them from.
        /// </summary>
        [Test]
        public void AWeaponAloneInTheOffHandArmsNobody()
        {
            using (TestBattle battle = new TestBattle())
            {
                ItemRules rules = Rules();

                CharacterStats stats = Armed(battle, rules, EquipmentClass.Light, 50,
                    Weapon(ItemRules.OffHandSlot, "claw", "light", 1));

                // The Claw is active — it is worn, it meets its requirement and it counts towards
                // the class mix. It still arms nobody, which is the whole assertion.
                Assert.AreEqual(1.5f, stats.AttacksPerSecond, 0.001f, "1 x (1 + 50 x 0.01).");
                Assert.AreEqual(10f, stats.PhysicalDamageMin, 0.001f, "The sheet's own punch.");
                Assert.AreEqual(1, stats.HandCount);
            }
        }

        // --- Two weapons ---

        /// <summary>
        /// items.md: the blows alternate, and each one uses the range of the hand that threw it.
        /// The speed and the reach stay the main hand's — two weapons do not attack at two speeds.
        /// </summary>
        [Test]
        public void TwoWeaponsGiveTwoHandsAndOneSpeed()
        {
            using (TestBattle battle = new TestBattle())
            {
                ItemRules rules = Rules();

                CharacterStats stats = Armed(battle, rules, EquipmentClass.Light, 50,
                    Weapon(ItemRules.MainHandSlot, "blade", "light", 1),
                    Weapon(ItemRules.OffHandSlot, "claw", "light", 1));

                Assert.AreEqual(2, stats.HandCount);

                Assert.AreEqual(6f, stats.PhysicalDamageMinOf(0), 0.001f, "The Blade is 6-8.");
                Assert.AreEqual(8f, stats.PhysicalDamageMaxOf(0), 0.001f);
                Assert.AreEqual(5f, stats.PhysicalDamageMinOf(1), 0.001f, "The Claw is 5-6.");
                Assert.AreEqual(6f, stats.PhysicalDamageMaxOf(1), 0.001f);

                Assert.AreEqual(1.95f, stats.AttacksPerSecond, 0.001f,
                    "1.3 x (1 + 50 x 0.01) — the Blade's speed and not the Claw's.");
            }
        }

        /// <summary>
        /// The blows really do take turns: primary, secondary, primary, secondary.
        ///
        /// The two ranges are chosen not to overlap, so which hand swung is readable from the
        /// damage alone whatever the roll landed on.
        /// </summary>
        [Test]
        public void TheBlowsAlternateBetweenTheHands()
        {
            using (TestBattle battle = new TestBattle())
            {
                ItemRules rules = Rules();

                // A Ram is 7-15 and a Claw is 5-6, so the two bands never touch.
                CharacterStats stats = Armed(battle, rules, EquipmentClass.Light, 50,
                    Weapon(ItemRules.MainHandSlot, "ram", "light", 1),
                    Weapon(ItemRules.OffHandSlot, "claw", "light", 1));

                for (int blow = 0; blow < 6; blow++)
                {
                    int hand = blow % 2;
                    int rolled = stats.RollPhysicalDamage(0.5f, hand);

                    if (hand == 0)
                    {
                        Assert.GreaterOrEqual(rolled, 7, "Blow " + blow + " should have come from the Ram.");
                    }
                    else
                    {
                        Assert.LessOrEqual(rolled, 6, "Blow " + blow + " should have come from the Claw.");
                    }
                }
            }
        }

        /// <summary>
        /// The average is taken across the hands, because they alternate one for one: over any
        /// stretch of a fight half the blows come from each.
        /// </summary>
        [Test]
        public void TheAverageDamageCountsBothHands()
        {
            using (TestBattle battle = new TestBattle())
            {
                ItemRules rules = Rules();

                CharacterStats stats = Armed(battle, rules, EquipmentClass.Light, 50,
                    Weapon(ItemRules.MainHandSlot, "ram", "light", 1),
                    Weapon(ItemRules.OffHandSlot, "claw", "light", 1));

                // The Ram averages 11 and the Claw 5.5.
                Assert.AreEqual(8.25f, stats.AveragePhysicalDamage, 0.001f);
            }
        }

        // --- The local damage modifier ---

        /// <summary>
        /// items.md: `weaponDamage` rolls a **range**, and the two ends land on the two ends of the
        /// weapon. A single number added to both would never widen anything.
        /// </summary>
        [Test]
        public void WeaponDamageWidensTheRangeOfItsOwnWeapon()
        {
            ItemWeapon blade = Rules().WeaponOf(Weapon(
                ItemRules.MainHandSlot, "blade", "light", 1,
                new ItemModifierRoll("weaponDamage", "software", 1, 2f, 5f)));

            Assert.AreEqual(8f, blade.MinDamage, 0.0001f, "6 + 2.");
            Assert.AreEqual(13f, blade.MaxDamage, 0.0001f, "8 + 5.");
        }

        /// <summary>
        /// And it stays on its own weapon. A character wide version would make an off hand carrying
        /// it strictly better than any alternative, instead of being a choice.
        /// </summary>
        [Test]
        public void WeaponDamageOnTheOffHandLeavesTheMainHandAlone()
        {
            using (TestBattle battle = new TestBattle())
            {
                ItemRules rules = Rules();

                CharacterStats stats = Armed(battle, rules, EquipmentClass.Light, 50,
                    Weapon(ItemRules.MainHandSlot, "blade", "light", 1),
                    Weapon(ItemRules.OffHandSlot, "claw", "light", 1,
                        new ItemModifierRoll("weaponDamage", "software", 1, 10f, 10f)));

                Assert.AreEqual(6f, stats.PhysicalDamageMinOf(0), 0.001f, "The Blade is untouched.");
                Assert.AreEqual(8f, stats.PhysicalDamageMaxOf(0), 0.001f);
                Assert.AreEqual(15f, stats.PhysicalDamageMinOf(1), 0.001f, "The Claw is 5 + 10.");
                Assert.AreEqual(16f, stats.PhysicalDamageMaxOf(1), 0.001f);
            }
        }

        // --- All the way through a real fight ---

        /// <summary>
        /// The end to end proof: the same hero, on the same board, against the same dummy, kills it
        /// far quicker holding a weapon than throwing its own punch.
        ///
        /// Everything above measures one seam at a time. This one runs the real director, the real
        /// attack timer and the real damage calculation, and is the only assertion here that would
        /// fail if the weapon were resolved perfectly and then never reached a blow.
        ///
        /// The sheet punch is deliberately feeble — 2 damage at one swing a second — so the gap is
        /// a matter of magnitudes and not of tuning. A level 20 Blade is 17.4-23.2.
        /// </summary>
        [Test]
        public void AWeaponReachesTheBlowsAndNotJustTheNumbers()
        {
            using (TestBattle battle = new TestBattle())
            {
                ItemRules rules = Rules();

                // A level 20 light weapon demands 30 AGI, and 40 clears it.
                CharacterDefinition sheet = battle.Sheet(
                    "puncher", CharacterKind.Hero, agility: 40, constitution: 100, baseDamage: 2);

                CharacterDefinition dummy = battle.Sheet(
                    "dummy", CharacterKind.Minion, constitution: 30, baseDamage: 0);

                int bare = StepsToFell(battle, sheet, dummy, rules, null);

                int armed = StepsToFell(battle, sheet, dummy, rules,
                    Weapon(ItemRules.MainHandSlot, "blade", "light", 20));

                Assert.Greater(bare, 0, "The bare hero never felled the dummy at all.");
                Assert.Greater(armed, 0, "The armed hero never felled the dummy at all.");

                Assert.Less(armed * 4, bare,
                    "The Blade is worth more than four times the punch on paper, and the fight does "
                    + "not agree: " + armed + " steps against " + bare + ".");
            }
        }

        /// <summary>
        /// How many simulation steps the hero needs to fell the dummy, or zero if it never does.
        ///
        /// Both fighters are placed one cell apart, so nobody walks and the count is the attack
        /// timer and the damage alone.
        /// </summary>
        private static int StepsToFell(
            TestBattle battle, CharacterDefinition sheet, CharacterDefinition dummySheet,
            ItemRules rules, Item weapon)
        {
            HeroRecord record = battle.Record(sheet);

            if (weapon != null)
            {
                record.Equipment.Put(weapon, rules);
            }

            Character hero = battle.SpawnHero(record, Team.Heroes, 0, 3, 1f, rules);
            Character dummy = battle.Spawn(dummySheet, Team.Enemies, 0, 4);

            BattleDirector director = battle.Direct(new BattleRandom(20260829), hero, dummy);

            int limit = Mathf.RoundToInt(300f / BattleDirector.FixedStep);
            int steps = 0;

            for (; steps < limit && dummy.IsAlive; steps++)
            {
                director.Tick(BattleDirector.FixedStep);
            }

            int taken = dummy.IsAlive ? 0 : steps;

            battle.Disband(hero);
            battle.Disband(dummy);

            return taken;
        }

        // --- POW still multiplies ---

        /// <summary>
        /// attributes.md: an attribute multiplies a damage base and never adds to it. That has to
        /// stay true of a weapon's base as much as of a sheet's punch, or a weapon would make POW
        /// stop paying.
        /// </summary>
        [Test]
        public void PowerStillMultipliesAWeaponRange()
        {
            using (TestBattle battle = new TestBattle())
            {
                ItemRules rules = Rules();

                // AGI only has to clear the Blade's requirement of 2; it changes no damage.
                CharacterDefinition sheet = battle.Sheet(
                    "strong", CharacterKind.Hero, power: 100, agility: 10);
                HeroRecord record = battle.Record(sheet);
                record.Equipment.Put(Weapon(ItemRules.MainHandSlot, "blade", "light", 1), rules);

                CharacterStats stats = battle.SpawnHero(record, Team.Heroes, 0, 0, 1f, rules).Stats;

                Assert.AreEqual(100, stats.TotalOf(Attribute.Power));
                Assert.AreEqual(6f * 1.1f, stats.PhysicalDamageMin, 0.001f, "6 x (1 + 100 x 0.001).");
                Assert.AreEqual(8f * 1.1f, stats.PhysicalDamageMax, 0.001f);
            }
        }
    }
}
