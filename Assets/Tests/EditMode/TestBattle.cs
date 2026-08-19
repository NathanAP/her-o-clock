using System.Collections.Generic;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Combat;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// Builds battles in memory, with no scene and nothing rendered.
    ///
    /// Everything created here belongs to the test that asked for it, and is thrown away with
    /// <see cref="Dispose"/>. It uses DestroyImmediate because outside Play Mode the deferred
    /// Destroy never runs.
    ///
    /// The whole point of the fixed step is that this is possible: a fight advances by being
    /// told to, so a test can run one in milliseconds and assert what came out.
    /// </summary>
    public class TestBattle : System.IDisposable
    {
        private readonly List<Object> owned = new List<Object>();

        public BattleGridConfig Config { get; private set; }
        public BattleGrid Grid { get; private set; }

        public TestBattle(int columns = 6, int rows = 8)
        {
            Config = ScriptableObject.CreateInstance<BattleGridConfig>();
            Config.Columns = columns;
            Config.Rows = rows;
            Config.HeroRows = rows / 2;
            Config.CellSize = 1f;
            owned.Add(Config);

            Grid = new BattleGrid(Config);
        }

        /// <summary>
        /// A character sheet built in code, so a test can state exactly the numbers it needs
        /// instead of depending on whatever the real assets happen to hold today.
        /// </summary>
        public CharacterDefinition Sheet(
            string id,
            CharacterKind kind,
            int power = 0,
            int agility = 0,
            int specialty = 0,
            int constitution = 10,
            EquipmentClass equipment = EquipmentClass.Light,
            int minRange = 1,
            int maxRange = 1,
            int physicalArmor = 0,
            int baseDamage = 10)
        {
            CharacterDefinition definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            definition.Id = id;
            definition.Kind = kind;
            definition.Level = 1;
            definition.MaxLevel = 100;
            definition.MinRange = minRange;
            definition.MaxRange = maxRange;
            definition.Growth = new AttributeGrowth { Power = 25, Agility = 25, Specialty = 25, Constitution = 25 };

            definition.Stats = new CharacterStats
            {
                BasePower = power,
                BaseAgility = agility,
                BaseSpecialty = specialty,
                BaseConstitution = constitution,
                Equipment = equipment,
                BasePhysicalArmor = physicalArmor,

                // Damage comes from a base the content provides, and POW only multiplies it. A test
                // character with no base would punch for nothing however much POW it was given, so
                // the helper hands out a workable weapon unless a test asks for another.
                BaseDamage = baseDamage
            };

            owned.Add(definition);
            return definition;
        }

        public Character Spawn(CharacterDefinition definition, Team team, int column, int row, int level = 1, float multiplier = 1f)
        {
            GameObject instance = new GameObject(definition.Id);
            Character character = instance.AddComponent<Character>();
            character.Initialize(definition, team, new GridPosition(column, row), Grid, level, multiplier);

            owned.Add(instance);
            return character;
        }

        /// <summary>A director already running the given fight.</summary>
        public BattleDirector Direct(BattleRandom random, params Character[] characters)
        {
            GameObject host = new GameObject("Director");
            BattleDirector director = host.AddComponent<BattleDirector>();
            owned.Add(host);

            director.Begin(Grid, characters, random);
            return director;
        }

        /// <summary>
        /// Advances a battle by whole simulation steps until it ends or the limit is reached.
        /// Returns how many steps were actually taken.
        /// </summary>
        public static int RunUntilOver(BattleDirector director, float seconds)
        {
            int limit = Mathf.RoundToInt(seconds / BattleDirector.FixedStep);

            for (int step = 0; step < limit; step++)
            {
                if (!director.IsRunning)
                {
                    return step;
                }

                director.Tick(BattleDirector.FixedStep);
            }

            return limit;
        }

        public void Dispose()
        {
            for (int i = 0; i < owned.Count; i++)
            {
                if (owned[i] != null)
                {
                    Object.DestroyImmediate(owned[i]);
                }
            }

            owned.Clear();
        }
    }
}
