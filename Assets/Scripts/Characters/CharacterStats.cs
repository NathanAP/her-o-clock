using System;
using HerOClock.Abilities;
using HerOClock.Progression;
using UnityEngine;
using UnityEngine.Serialization;

namespace HerOClock.Characters
{
    /// <summary>
    /// Turns primary attributes into secondary ones, following attributes.md.
    /// Every percentage is returned on a 0 to 100 scale, matching the spec.
    ///
    /// The serialized fields are the **base** values of the sheet. What the rest of the game
    /// reads are the totals, which today are the base plus the points earned by levelling.
    /// Nothing outside this class should ever read a base value directly: when items, skill
    /// trees and buffs arrive, they become new sources inside <see cref="TotalOf"/> and no
    /// other file has to change.
    /// </summary>
    [Serializable]
    public class CharacterStats
    {
        [Header("Primary attributes (base values of the sheet)")]
        [Min(0)] public int BasePower;
        [Min(0)] public int BaseAgility;
        [Min(0)] public int BaseSpecialty;
        [Min(0)] public int BaseConstitution;

        [Header("Damage")]
        [Tooltip("Lowest damage of one basic attack before POW raises it. The character's own fists, "
            + "replaced by a weapon once items exist.")]
        [Min(0f)] public float BaseDamageMin = 1f;

        [Tooltip("How much the lowest damage gains per level. Heroes leave it at zero and grow through "
            + "items; minions and villains use it, since nothing else makes them hit harder.")]
        [Min(0f)] public float BaseDamageMinPerLevel;

        [Tooltip("Highest damage of one basic attack before POW raises it. Every blow rolls between "
            + "this and the minimum, so the gap is how unpredictable this character hits.")]
        [Min(0f)] public float BaseDamageMax = 1f;

        [Tooltip("How much the highest damage gains per level. Kept in proportion to the minimum's "
            + "growth, so the spread means the same thing at level 100 as it does at level 1.")]
        [Min(0f)] public float BaseDamageMaxPerLevel;

        [Header("Equipment")]
        public EquipmentClass Equipment = EquipmentClass.Light;

        [Header("Defence (base values of the sheet)")]
        [Tooltip("Physical armour at the character's starting level.")]
        [FormerlySerializedAs("PhysicalArmor")]
        [Min(0)] public int BasePhysicalArmor;

        [Tooltip("Physical armour gained on every level. Setting it equal to the base keeps mitigation "
            + "against a same level attacker constant for the whole game.")]
        [Min(0)] public int PhysicalArmorPerLevel;

        [Tooltip("Evasion points at the character's starting level. Same curve and same constant as "
            + "physical armour, so the same rule applies: set the growth equal to the base to keep "
            + "the chance constant for the whole game.")]
        [Min(0)] public int BaseEvasion;

        [Min(0)] public int EvasionPerLevel;

        [Tooltip("Resistance points against fire at the starting level. Same curve as physical armour.")]
        [FormerlySerializedAs("FireResistance")]
        [Min(0)] public int BaseFireResistance;

        [Min(0)] public int FireResistancePerLevel;

        [Tooltip("Resistance points against water at the starting level. Same curve as physical armour.")]
        [FormerlySerializedAs("WaterResistance")]
        [Min(0)] public int BaseWaterResistance;

        [Min(0)] public int WaterResistancePerLevel;

        [Tooltip("Resistance points against electricity at the starting level. Same curve as physical armour.")]
        [FormerlySerializedAs("ElectricResistance")]
        [Min(0)] public int BaseElectricResistance;

        [Min(0)] public int ElectricResistancePerLevel;

        [Tooltip("Physical damage reflected back at the attacker, from 0 to 100. A share of the damage, "
            + "so it does not decay with level and needs no growth of its own.")]
        [FormerlySerializedAs("ThornsPercent")]
        [Range(0f, 100f)] public float BaseThornsPercent;

        [Header("Offence")]
        [Tooltip("Health recovered when dealing physical damage, from 0 to 100.")]
        [Range(0f, 100f)] public float LifeStealPercent;

        [Tooltip("Health recovered per second before POW. Zero on every sheet today: regeneration is "
            + "meant to arrive with items and skills. POW multiplies whatever ends up here.")]
        [Min(0f)] public float BaseHealthRegen;

        // --- Per instance state, never part of the sheet ---

        [NonSerialized] private int level = 1;
        [NonSerialized] private float multiplier = 1f;
        [NonSerialized] private int[] levelPoints = new int[4];

        /// <summary>
        /// The buffs and debuffs currently on this instance, or null when there are none.
        ///
        /// This is the seam the class was written around, now actually carrying something. It
        /// reaches the derived values as well as the four primaries, because content buffs attack
        /// speed and cooldown reduction long before it buffs an attribute.
        /// </summary>
        [NonSerialized] private StatModifiers modifiers;

        /// <summary>
        /// What this character is actually wearing, mixed into the three classes attributes.md
        /// knows about, or null while nothing has been equipped.
        ///
        /// Null is not a missing value, it is the answer: a character wearing nothing uses the
        /// class written on its sheet, which is every minion, villain and NPC, and a hero before
        /// it has items. Falling back in <see cref="Composition"/> rather than filling this in
        /// keeps that rule in one place, and keeps the sheet's own stats correct even though
        /// nobody calls <see cref="ApplyInstance(int, int[], float)"/> on them.
        /// </summary>
        [NonSerialized] private EquipmentComposition equipment;

        /// <summary>
        /// What the character's **active** equipment adds up to, or null while nothing is worn.
        ///
        /// Inactive pieces are not in here at all. An item that stopped meeting its requirement is
        /// disregarded whole, so it contributes no attribute, no defence and no slice of class.
        /// </summary>
        [NonSerialized] private EquipmentTotals equipmentTotals;

        /// <summary>
        /// What the character's hands are holding, or null while it fights bare.
        ///
        /// Null is the answer and not a gap, exactly like <see cref="equipment"/>: a character with
        /// no weapon uses the three values on its own sheet — the punch, the speed 1 and the reach
        /// it declares. Every fallback below reads this being null, so "unarmed" is written once.
        /// </summary>
        [NonSerialized] private WeaponLoadout weapons;

        /// <summary>
        /// Base attack speed of a character holding nothing, in attacks per second.
        ///
        /// This is the `1` a weapon replaces. What it never replaces is the AGI term multiplying
        /// it: a weapon swaps the base, never the result.
        /// </summary>
        public const float BaseAttacksPerSecond = 1f;

        /// <summary>Base movement speed of every character, in cells per second.</summary>
        public const float BaseCellsPerSecond = 2f;

        /// <summary>
        /// A copy of these stats, so a character instance can be levelled up and buffed
        /// without touching the shared definition asset.
        ///
        /// The level points array is rebuilt on purpose. A shallow copy would hand both
        /// characters the same array, and levelling one would silently level the other.
        /// </summary>
        public CharacterStats Clone()
        {
            CharacterStats copy = (CharacterStats)MemberwiseClone();
            copy.levelPoints = new int[4];
            Array.Copy(levelPoints, copy.levelPoints, 4);

            // Never inherited. A copy belongs to another character, and sharing the buff list
            // would let one character's slow land on somebody else.
            copy.modifiers = null;

            // Same reasoning: equipment belongs to whoever is wearing it. A copy starts from its
            // own sheet class and with nothing on until somebody hands it a set of its own.
            copy.equipment = null;
            copy.equipmentTotals = null;
            copy.weapons = null;

            return copy;
        }

        /// <summary>Hands this instance the buff list it should read. Called once, by the character.</summary>
        public void UseModifiers(StatModifiers value)
        {
            modifiers = value;
        }

        /// <summary>
        /// Hands this instance the equipment it is wearing, already mixed into slices.
        ///
        /// Called once, when the stage builds the combatant, for the same reason the attribute
        /// points are copied once: what fights is a photograph, and a player re-equipping a hero
        /// mid stage must not reach the board until the next one.
        /// </summary>
        public void UseEquipment(
            EquipmentComposition composition, EquipmentTotals totals, WeaponLoadout loadout = null)
        {
            equipment = composition;
            equipmentTotals = totals;
            weapons = loadout;
        }

        /// <summary>
        /// How many hands take turns swinging: two while a second weapon is held, one otherwise.
        ///
        /// Whoever counts the blows uses this to know which hand is next. It is never zero, so a
        /// caller never has to check whether there is a weapon at all.
        /// </summary>
        public int HandCount
        {
            get { return weapons != null ? weapons.HandCount : 1; }
        }

        /// <summary>What the active equipment gives, or an empty bag when there is none.</summary>
        private EquipmentTotals Equipped
        {
            get { return equipmentTotals ?? Empty; }
        }

        /// <summary>
        /// Shared and never written to, so every character with nothing on reads the same zeroes
        /// instead of allocating a bag each time a stat is asked for.
        /// </summary>
        private static readonly EquipmentTotals Empty = new EquipmentTotals();

        /// <summary>
        /// The class mix every per class table below reads, falling back to the sheet's own class
        /// while nothing is worn.
        /// </summary>
        private EquipmentComposition Composition
        {
            get { return equipment ?? EquipmentComposition.Of(Equipment); }
        }

        /// <summary>Runs a computed value through the buffs touching that stat, if there are any.</summary>
        private float Modified(ModifiableStat stat, float value)
        {
            return modifiers == null ? value : modifiers.Apply(stat, value);
        }

        /// <summary>
        /// Sets everything that belongs to this instance rather than to the sheet, with every
        /// level point spent by the given distribution.
        ///
        /// The points are computed straight from the level instead of being accumulated one level
        /// at a time. That keeps the result free of rounding drift and lets a level 40 minion be
        /// created without walking through 39 level ups.
        /// </summary>
        public void ApplyInstance(int level, AttributeGrowth growth, float multiplier)
        {
            ApplyLevel(level, multiplier);

            AttributeGrowth.Distribute(LevelProgress.PointsAtLevel(this.level), growth, levelPoints);
        }

        /// <summary>
        /// The same, but taking a split that was already decided elsewhere — a hero's build,
        /// which mixes points placed by hand with points spent automatically.
        ///
        /// It takes the four numbers and not the <see cref="AttributeAllocation"/> they came from,
        /// on purpose. A combatant copies its build once, when the stage builds it, and holding
        /// the live object instead would let a rebuild reach into a fight already running.
        /// </summary>
        public void ApplyInstance(int level, int[] points, float multiplier)
        {
            ApplyLevel(level, multiplier);

            for (int i = 0; i < 4; i++)
            {
                levelPoints[i] = points != null && i < points.Length ? points[i] : 0;
            }
        }

        private void ApplyLevel(int level, float multiplier)
        {
            this.level = Mathf.Max(1, level);
            this.multiplier = Mathf.Max(0f, multiplier);
        }

        /// <summary>
        /// The value of an attribute counting every source, which today means the sheet plus
        /// the points earned by levelling, scaled by the stage multiplier.
        /// </summary>
        public int TotalOf(Attribute attribute)
        {
            float total = BaseOf(attribute) + levelPoints[(int)attribute];

            if (!Mathf.Approximately(multiplier, 1f))
            {
                total *= multiplier;
            }

            // Equipment lands **after** the stage's fine tuning and before the buffs. The
            // multiplier exists for a stage to adjust a *sheet*, and equipment is not sheet, it is
            // what the player built: letting a stage amplify it would make a hard stage punish a
            // well equipped hero more than a bare one. The general reason behind that choice is
            // worth keeping in mind elsewhere too — anything multiplied has a way of running away
            // from whoever wrote the multiplier.
            total += Equipped.Of(attribute);

            // Buffs come last, on top of everything the sheet and the level produced. A debuff can
            // take an attribute down to 0 and no further, as buffs-and-debuffs.md states.
            total = Modified(StatOf(attribute), total);

            return Mathf.Max(0, Mathf.RoundToInt(total));
        }

        private static ModifiableStat StatOf(Attribute attribute)
        {
            switch (attribute)
            {
                case Attribute.Power: return ModifiableStat.Power;
                case Attribute.Agility: return ModifiableStat.Agility;
                case Attribute.Specialty: return ModifiableStat.Specialty;
                default: return ModifiableStat.Constitution;
            }
        }

        public int Power { get { return TotalOf(Attribute.Power); } }
        public int Agility { get { return TotalOf(Attribute.Agility); } }
        public int Specialty { get { return TotalOf(Attribute.Specialty); } }
        public int Constitution { get { return TotalOf(Attribute.Constitution); } }

        private int BaseOf(Attribute attribute)
        {
            switch (attribute)
            {
                case Attribute.Power: return BasePower;
                case Attribute.Agility: return BaseAgility;
                case Attribute.Specialty: return BaseSpecialty;
                default: return BaseConstitution;
            }
        }

        // --- Defence ---

        /// <summary>
        /// A defensive value counting every source, the same way <see cref="TotalOf"/> does for the
        /// primary attributes. Nothing outside this class reads the base fields.
        ///
        /// Defence has to grow with the level because the mitigation curve's constant grows with
        /// the attacker's level. Left flat, the same armour is worth less every level, and a
        /// character with no source of new armour simply rots: the villain used to fall from 56%
        /// mitigation at level 1 to 4% at level 50 without anything being done to it.
        ///
        /// Growth equal to the base is the shape that holds mitigation still, because both the
        /// armour and the curve's constant then scale with the level and cancel out.
        /// </summary>
        /// <summary>
        /// A sheet value that grows with the level and then answers to the stage's fine tuning.
        ///
        /// Used by every defence and by the damage base, because they share the same problem: a
        /// number that never moves is worth less every level, since what it is measured against
        /// keeps rising.
        /// </summary>
        private int DefenceOf(int baseValue, int perLevel)
        {
            return Mathf.Max(0, Mathf.RoundToInt(ScaledOf(baseValue, perLevel)));
        }

        /// <summary>
        /// The same growth without the rounding, for the values that are not whole numbers.
        ///
        /// The damage range needs it: a minion can sit between 1.5 and 2.5, and rounding each end
        /// before the roll would collapse the range into a single number.
        /// </summary>
        private float ScaledOf(float baseValue, float perLevel)
        {
            float total = baseValue + perLevel * (level - 1);

            if (!Mathf.Approximately(multiplier, 1f))
            {
                total *= multiplier;
            }

            return Mathf.Max(0f, total);
        }

        public int PhysicalArmor
        {
            get
            {
                float armor = DefenceOf(BasePhysicalArmor, PhysicalArmorPerLevel) + Equipped.Armour;
                return Mathf.Max(0, Mathf.RoundToInt(Modified(ModifiableStat.PhysicalArmor, armor)));
            }
        }

        public int FireResistance
        {
            get { return DefenceOf(BaseFireResistance, FireResistancePerLevel) + Equipped.FireResistance; }
        }

        public int WaterResistance
        {
            get { return DefenceOf(BaseWaterResistance, WaterResistancePerLevel) + Equipped.WaterResistance; }
        }

        public int ElectricResistance
        {
            get { return DefenceOf(BaseElectricResistance, ElectricResistancePerLevel) + Equipped.ElectricResistance; }
        }

        /// <summary>
        /// Physical damage sent back at the attacker, counting the sheet and the equipment.
        ///
        /// Nothing outside this class reads the sheet's own field, for the same reason nothing
        /// reads `BasePower`: the moment a second source exists, reading the base is reading a
        /// number the game does not play with.
        /// </summary>
        public float ThornsPercent
        {
            get { return BaseThornsPercent + Equipped.ThornsPercent; }
        }

        /// <summary>
        /// Share of the target's defence this character ignores when it attacks, from 0 to 100.
        ///
        /// Equipment only, with no field on the sheet. A minion has no items, so it has none of
        /// this — which is the honest answer rather than a zero pretending to be a choice. The day
        /// a villain is written that pierces armour, the sheet gains the field and this gains a
        /// term, exactly as <see cref="ThornsPercent"/> already reads.
        /// </summary>
        public float ResistanceIgnoredPercent
        {
            get { return Equipped.ResistanceIgnoredPercent; }
        }

        /// <summary>
        /// Extra ability damage for one element, as a percentage.
        ///
        /// Four properties and not one taking a damage type, because `DamageType` lives in
        /// `Combat`, which already depends on this namespace. Whoever knows about damage types
        /// does the switch, the same way the mitigation lookup already does.
        ///
        /// **It never touches the basic attack.** What raises a basic attack is the weapon's own
        /// base damage, and keeping the two families apart is what makes a weapon build and an
        /// ability build look for different items.
        /// </summary>
        public float FireAbilityDamagePercent
        {
            get { return Equipped.FireAbilityDamagePercent; }
        }

        /// <inheritdoc cref="FireAbilityDamagePercent"/>
        public float WaterAbilityDamagePercent
        {
            get { return Equipped.WaterAbilityDamagePercent; }
        }

        /// <inheritdoc cref="FireAbilityDamagePercent"/>
        public float ElectricAbilityDamagePercent
        {
            get { return Equipped.ElectricAbilityDamagePercent; }
        }

        /// <inheritdoc cref="FireAbilityDamagePercent"/>
        public float PhysicalAbilityDamagePercent
        {
            get { return Equipped.PhysicalAbilityDamagePercent; }
        }

        // --- Linear secondary attributes ---

        /// <summary>
        /// attributes.md: `CON x 10 + vida vinda de outras fontes`.
        ///
        /// CON stays the only **attribute** that grants health. What equipment does is add, never
        /// convert, so a point of CON is worth the same ten whatever the hero is wearing.
        /// </summary>
        public int MaxHealth
        {
            get { return Constitution * HealthPerConstitution + Equipped.Life; }
        }

        /// <summary>Health granted by each point of CON, from attributes.md.</summary>
        public const int HealthPerConstitution = 10;

        /// <summary>
        /// How much of the damage base a single attribute point adds, as a fraction.
        ///
        /// Ten points are one percent. It is deliberately small: the base comes from content — the
        /// weapon for a basic attack, the rank for an ability — and the attribute multiplies it.
        /// A point that added flat damage would make the weapon irrelevant, since a character can
        /// reach 500 points and no weapon is worth five hundred of anything.
        /// </summary>
        public const float DamageSharePerPoint = 0.001f;

        /// <summary>
        /// Health recovered per second.
        ///
        /// POW does not grant regeneration, it multiplies it, which is what "0.5% of regeneration
        /// speed per point" in attributes.md means. With nothing granting a base, this is zero for
        /// everyone today, exactly like thorns and life steal. It becomes real when items arrive.
        ///
        /// It still scales with the level, because POW does.
        /// </summary>
        public float HealthPerSecond
        {
            get { return BaseHealthRegen * (1f + Constitution * HealthRegenPerConstitution); }
        }

        /// <summary>Share of regeneration speed granted by each point of CON.</summary>
        public const float HealthRegenPerConstitution = 0.005f;

        /// <summary>
        /// Lowest and highest damage of one basic attack: the range this character swings with,
        /// both ends raised by POW.
        ///
        /// The range is the character's own until an item replaces it — the punch a robot throws
        /// with nothing equipped. POW never adds to it, it multiplies it, which is what keeps a
        /// weapon worth finding.
        ///
        /// Both ends grow with the level the same way armour does, and for the same reason: a
        /// value standing still is a character that stops mattering. A hero leaves the growth at
        /// zero because a weapon is what raises it; a minion has no weapon, so this is the only
        /// thing that makes it hit harder in a later act.
        ///
        /// They are floats and are not rounded here on purpose. A minion sitting between 1.5 and
        /// 2.5 would lose its whole range if each end were rounded before the roll.
        /// </summary>
        public float PhysicalDamageMin
        {
            get { return PhysicalDamageMinOf(0); }
        }

        /// <inheritdoc cref="PhysicalDamageMin"/>
        public float PhysicalDamageMax
        {
            get { return PhysicalDamageMaxOf(0); }
        }

        /// <summary>
        /// The same range, for one of the hands that take turns swinging.
        ///
        /// With two weapons held the two hands have different ranges, and each blow uses the range
        /// of the hand that threw it. Everything else about the character stays the same from one
        /// blow to the next: life steal, thorns and elemental damage belong to the hero, and only
        /// the damage belongs to the hand.
        ///
        /// A weapon's range is already scaled by the **item's** level when it arrives here, so it
        /// does not go through <see cref="ScaledOf"/> again — and it is not touched by the stage
        /// multiplier either, for the reason written on <see cref="TotalOf"/>: the multiplier exists
        /// to adjust a sheet, and a weapon is not sheet.
        /// </summary>
        public float PhysicalDamageMinOf(int hand)
        {
            return weapons != null
                ? weapons.MinDamageOf(hand) * PowerShare
                : SwingOf(BaseDamageMin, BaseDamageMinPerLevel);
        }

        /// <inheritdoc cref="PhysicalDamageMinOf"/>
        public float PhysicalDamageMaxOf(int hand)
        {
            return weapons != null
                ? weapons.MaxDamageOf(hand) * PowerShare
                : SwingOf(BaseDamageMax, BaseDamageMaxPerLevel);
        }

        /// <summary>
        /// The middle of the range.
        ///
        /// **Nothing in a fight reads this.** It is for the places that compare characters without
        /// swinging — the editor windows and the balance snapshot — where a single number is what
        /// makes two sheets comparable. A blow always comes from
        /// <see cref="RollPhysicalDamage"/>.
        /// </summary>
        public float AveragePhysicalDamage
        {
            get
            {
                float total = 0f;

                // Averaged across the hands rather than read off the first one, because the hands
                // alternate one for one: over any stretch of a fight, half the blows come from each.
                for (int hand = 0; hand < HandCount; hand++)
                {
                    total += (PhysicalDamageMinOf(hand) + PhysicalDamageMaxOf(hand)) * 0.5f;
                }

                return total / HandCount;
            }
        }

        private float SwingOf(float baseValue, float perLevel)
        {
            return ScaledOf(baseValue, perLevel) * PowerShare;
        }

        /// <summary>
        /// What POW multiplies a damage base by. Never an addition, for the reason written on
        /// <see cref="DamageSharePerPoint"/>.
        /// </summary>
        private float PowerShare
        {
            get { return 1f + Power * DamageSharePerPoint; }
        }

        /// <summary>
        /// The damage of one blow, drawn from anywhere in the range with equal chance.
        ///
        /// It takes a number from 0 to 1 rather than the battle's random source, and that is
        /// deliberate: <c>Characters</c> would otherwise have to reference <c>Combat</c>, which is
        /// the wrong direction — <c>Combat</c> already depends on this class. The caller draws,
        /// this class does the arithmetic.
        ///
        /// The rounding happens once, at the end, following "Arredondamento" in attributes.md.
        /// </summary>
        public int RollPhysicalDamage(float unitRoll)
        {
            return RollPhysicalDamage(unitRoll, 0);
        }

        /// <inheritdoc cref="RollPhysicalDamage(float)"/>
        public int RollPhysicalDamage(float unitRoll, int hand)
        {
            float min = PhysicalDamageMinOf(hand);
            float max = PhysicalDamageMaxOf(hand);

            // A sheet with the two ends equal never varies, and a sheet with them the wrong way
            // round is a content mistake that should not become negative damage.
            float swing = max > min ? min + Mathf.Clamp01(unitRoll) * (max - min) : min;

            return Mathf.Max(0, Mathf.RoundToInt(swing));
        }

        /// <summary>
        /// Attacks per second: the weapon's own speed, then AGI, then the buffs.
        ///
        /// The weapon replaces the base and never the result, which is what keeps AGI worth
        /// investing in for somebody holding one. Substituting the result instead would make the
        /// light class stop paying the moment a weapon is equipped.
        ///
        /// Only the main hand has a say. Two weapons held do not attack at two speeds; they take
        /// turns at the speed of the one in the main hand.
        /// </summary>
        public float AttacksPerSecond
        {
            get
            {
                float rate = WeaponAttackSpeed * (1f + Agility * AttackSpeedPerAgility);
                return Mathf.Max(0f, Modified(ModifiableStat.AttackSpeed, rate));
            }
        }

        /// <summary>The base a weapon replaces, or the bare character's own 1.</summary>
        private float WeaponAttackSpeed
        {
            get { return weapons != null ? weapons.AttackSpeed : BaseAttacksPerSecond; }
        }

        public float CellsPerSecond
        {
            get
            {
                float speed = BaseCellsPerSecond * (1f + Agility * MoveSpeedPerAgility);
                return Mathf.Max(0f, Modified(ModifiableStat.MovementSpeed, speed));
            }
        }

        // --- Secondary attributes with diminishing returns ---

        /// <summary>
        /// Total evasion points, counting every source: the sheet's own value grown by level, and
        /// AGI converted at the rate the worn equipment class gives.
        ///
        /// **This is points and not a chance**, and that is the whole shape of the rule. Evasion
        /// goes through the same curve as physical armour and elemental resistance, against the
        /// same constant of 50 x the attacker's level, so how much of an attack it avoids depends
        /// on who is swinging. A character cannot know its own evasion chance on its own, and
        /// <see cref="Combat.DamageCalculator"/> is where the two sides meet.
        ///
        /// Before this, evasion was the one defence whose constant did not grow, so it was the
        /// only one that never rotted — a character that never improved it kept the same chance
        /// from level 1 to 100 while its armour turned to dust. Nobody decided that; it was the
        /// shape of the formula deciding for us.
        /// </summary>
        public int EvasionPoints
        {
            get
            {
                float fromSheet = DefenceOf(BaseEvasion, EvasionPerLevel);
                float fromAgility = Agility * EvasionPerAgility;

                return Mathf.Max(0, Mathf.RoundToInt(fromSheet + fromAgility + Equipped.Evasion));
            }
        }

        /// <summary>
        /// Cooldown reduction, from 0 to 60 before buffs. The curve never reaches 60, so nothing
        /// clamps it; a buff can push past that, the same way elemental resistance can pass 100%.
        ///
        /// It is not clamped here either. What protects the game is that an ability's cooldown is
        /// floored at zero where it is computed, so even an absurd value can only ever mean
        /// "ready immediately" rather than a negative wait.
        /// </summary>
        public float CooldownReduction
        {
            get
            {
                float reduction = (float)DiminishingReturns(60.0, Specialty, CooldownConstant);
                return Mathf.Max(0f, Modified(ModifiableStat.CooldownReduction, reduction));
            }
        }

        /// <summary>
        /// Physical mitigation, from 0 to 75. The constant grows with the attacker's level,
        /// so the same armour is worth less against stronger enemies.
        /// </summary>
        public float PhysicalMitigationAgainst(int attackerLevel)
        {
            return (float)DiminishingReturns(75.0, PhysicalArmor, 50.0 * Mathf.Max(1, attackerLevel));
        }

        /// <summary>
        /// Evasion chance, from 0 to 100, against an attacker of the given level. Same curve and
        /// same constant as the mitigation above.
        ///
        /// The fight itself never calls this: <see cref="Combat.DamageCalculator"/> works from
        /// <see cref="EvasionPoints"/> and the attacker it already has in hand. This exists for
        /// the places that want to show a number without an attacker — the editor windows and the
        /// balance snapshot — and they all pass the character's own level, because that is the
        /// comparison the defence growth exists to hold still.
        /// </summary>
        public float EvasionChanceAgainst(int attackerLevel)
        {
            return (float)DiminishingReturns(100.0, EvasionPoints, 50.0 * Mathf.Max(1, attackerLevel));
        }

        /// <summary>
        /// The diminishing returns curve from the spec: Cap x Points / (Points + Constant).
        /// The cap is never reached, only approached, so no manual clamp is needed anywhere.
        ///
        /// It computes in double so the damage calculation can use it without going back through
        /// float, where the runtime is free to round intermediates differently and change a
        /// result that lands on a half. This is the only place the curve is written down.
        /// </summary>
        public static double DiminishingReturns(double cap, double points, double constant)
        {
            if (points <= 0.0 || constant <= 0.0)
            {
                return 0.0;
            }

            return cap * points / (points + constant);
        }

        // --- Per equipment class tables ---
        //
        // These four are the whole reason EquipmentComposition exists. Each one is three numbers
        // from attributes.md — one per pure class — and the character's own value is those three
        // averaged by what it wears. A character in a single class lands exactly on that class's
        // number, which is why nothing moved when the mixing arrived.

        private float AttackSpeedPerAgility
        {
            get { return Composition.Blend(0.01f, 0.005f, 0.002f); }
        }

        private float MoveSpeedPerAgility
        {
            get { return Composition.Blend(0.01f, 0.0075f, 0.005f); }
        }

        private float EvasionPerAgility
        {
            get { return Composition.Blend(1f, 0.5f, 0.2f); }
        }

        private float CooldownConstant
        {
            get { return Composition.Blend(60f, 40f, 300f); }
        }
    }
}
