using System;
using UnityEngine;

namespace HerOClock.Characters
{
    /// <summary>
    /// Turns primary attributes into secondary ones, following attributes.md.
    /// Every percentage is returned on a 0 to 100 scale, matching the spec.
    /// </summary>
    [Serializable]
    public class CharacterStats
    {
        [Header("Primary attributes")]
        [Min(0)] public int Power;
        [Min(0)] public int Agility;
        [Min(0)] public int Specialty;
        [Min(0)] public int Constitution;

        [Header("Equipment")]
        public EquipmentClass Equipment = EquipmentClass.Light;

        [Header("Defence")]
        [Tooltip("Physical armour granted by equipment. Items do not exist yet, so it is filled by hand.")]
        [Min(0)] public int PhysicalArmor;

        [Tooltip("Resistance points against fire. Uses the same curve as physical armour.")]
        [Min(0)] public int FireResistance;

        [Tooltip("Resistance points against water. Uses the same curve as physical armour.")]
        [Min(0)] public int WaterResistance;

        [Tooltip("Resistance points against electricity. Uses the same curve as physical armour.")]
        [Min(0)] public int ElectricResistance;

        [Tooltip("Physical damage reflected back at the attacker, from 0 to 100.")]
        [Range(0f, 100f)] public float ThornsPercent;

        [Header("Offence")]
        [Tooltip("Health recovered when dealing physical damage, from 0 to 100.")]
        [Range(0f, 100f)] public float LifeStealPercent;

        /// <summary>
        /// A copy of these stats, so a character instance can be levelled up and buffed
        /// without touching the shared definition asset.
        ///
        /// Every field here is a value type, so a shallow copy is a complete copy.
        /// </summary>
        public CharacterStats Clone()
        {
            return (CharacterStats)MemberwiseClone();
        }

        /// <summary>Base attack speed of every character, in attacks per second.</summary>
        public const float BaseAttacksPerSecond = 1f;

        /// <summary>Base movement speed of every character, in cells per second.</summary>
        public const float BaseCellsPerSecond = 2f;

        // --- Linear secondary attributes ---

        public int MaxHealth
        {
            get { return Power * 5 + Constitution * 10; }
        }

        public int PhysicalDamage
        {
            get { return Power; }
        }

        public int ElementalDamage
        {
            get { return Specialty; }
        }

        public float AttacksPerSecond
        {
            get { return BaseAttacksPerSecond * (1f + Agility * AttackSpeedPerAgility); }
        }

        public float CellsPerSecond
        {
            get { return BaseCellsPerSecond * (1f + Agility * MoveSpeedPerAgility); }
        }

        // --- Secondary attributes with diminishing returns ---

        /// <summary>Evasion chance, from 0 to 100. Never reaches 100.</summary>
        public float EvasionChance
        {
            get { return DiminishingReturns(100f, Agility, EvasionConstant); }
        }

        /// <summary>Cooldown reduction, from 0 to 60. Never reaches 60.</summary>
        public float CooldownReduction
        {
            get { return DiminishingReturns(60f, Specialty, CooldownConstant); }
        }

        /// <summary>
        /// Physical mitigation, from 0 to 75. The constant grows with the attacker's level,
        /// so the same armour is worth less against stronger enemies.
        /// </summary>
        public float PhysicalMitigationAgainst(int attackerLevel)
        {
            return DiminishingReturns(75f, PhysicalArmor, 50f * Mathf.Max(1, attackerLevel));
        }

        /// <summary>
        /// The diminishing returns curve from the spec: Cap x Points / (Points + Constant).
        /// The cap is never reached, only approached, so no manual clamp is needed anywhere.
        /// </summary>
        public static float DiminishingReturns(float cap, float points, float constant)
        {
            if (points <= 0f || constant <= 0f)
            {
                return 0f;
            }

            return cap * points / (points + constant);
        }

        // --- Per equipment class tables ---

        private float AttackSpeedPerAgility
        {
            get
            {
                switch (Equipment)
                {
                    case EquipmentClass.Light: return 0.01f;
                    case EquipmentClass.Magic: return 0.005f;
                    default: return 0.002f;
                }
            }
        }

        private float MoveSpeedPerAgility
        {
            get
            {
                switch (Equipment)
                {
                    case EquipmentClass.Light: return 0.01f;
                    case EquipmentClass.Magic: return 0.0075f;
                    default: return 0.005f;
                }
            }
        }

        private float EvasionConstant
        {
            get
            {
                switch (Equipment)
                {
                    case EquipmentClass.Light: return 100f;
                    case EquipmentClass.Magic: return 200f;
                    default: return 500f;
                }
            }
        }

        private float CooldownConstant
        {
            get
            {
                switch (Equipment)
                {
                    case EquipmentClass.Light: return 60f;
                    case EquipmentClass.Magic: return 40f;
                    default: return 300f;
                }
            }
        }
    }
}
