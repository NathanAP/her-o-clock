using System;
using System.Collections.Generic;
using HerOClock.Characters;
using UnityEngine;

namespace HerOClock.Setup
{
    /// <summary>
    /// Who starts the battle on which cell.
    ///
    /// For heroes this is the formation chosen by the player. For minions and villains it is
    /// the stage's predefined group. A formation only defines where the battle starts, it does
    /// not restrict movement afterwards.
    /// </summary>
    [CreateAssetMenu(fileName = "BattleFormation", menuName = "Her-o-clock/Battle Formation")]
    public class BattleFormation : ScriptableObject
    {
        [Serializable]
        public struct Placement
        {
            public CharacterDefinition Character;

            [Min(1)] public int Column;
            [Min(1)] public int Row;
        }

        [Tooltip("Side of the fight. Heroes on one side, minions and villains on the other.")]
        public Team Team = Team.Heroes;

        public List<Placement> Placements = new List<Placement>();
    }
}
