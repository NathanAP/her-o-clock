using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Progression;
using HerOClock.Setup;
using HerOClock.Stages;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace HerOClock.Tests
{
    /// <summary>
    /// Balance, in the two ways it can be checked.
    ///
    /// The assertions state the few things that must stay true no matter what: the first stage is
    /// beatable with a fresh team, the second one is not. They are ranges and intentions, not
    /// exact values, and when one fails the question to ask is "did I want that to change?".
    ///
    /// The snapshot is the other half. It writes the current tables to
    /// <c>.claude/balance/snapshot.md</c>, which is committed alongside whatever changed them.
    /// The assertions catch the catastrophic move; the diff of that file shows the whole ripple,
    /// including everything nobody thought to assert.
    /// </summary>
    public class BalanceTests
    {
        /// <summary>Fixed so a rerun with no changes produces the same file.</summary>
        private const int Seed = 20260813;

        /// <summary>Enemies defeated per minute of open game, as stated in progress.md.</summary>
        private const double EnemiesPerMinute = 11.0;

        // --- What must stay true ---

        [Test]
        public void TheFirstStageIsBeatableByAFreshTeam()
        {
            StageSimulation.Outcome outcome = Play(0, HeroLevelFromFormation());

            Assert.IsFalse(outcome.TimedOut, "The first stage did not resolve at all.");
            Assert.IsTrue(outcome.Cleared,
                "A brand new team can no longer clear the first stage, so the game has no opening.");
        }

        /// <summary>
        /// The second stage is deliberately above the team's level. It is the wall that gives
        /// farming a reason to exist, and it stops being one the moment it can be walked through.
        /// </summary>
        [Test]
        public void TheSecondStageIsAWallForAFreshTeam()
        {
            StageSimulation.Outcome outcome = Play(1, HeroLevelFromFormation());

            Assert.IsFalse(outcome.Cleared,
                "The second stage stopped being a wall, so nothing in the game asks the player to farm.");
        }

        /// <summary>
        /// The wall has to be climbable. If enough levels do not open it, the stage is not a wall,
        /// it is a dead end.
        /// </summary>
        [Test]
        public void TheSecondStageOpensUpWithEnoughLevels()
        {
            const int comfortable = 60;

            Assert.IsTrue(Play(1, comfortable).Cleared,
                "Not even a level " + comfortable + " team can clear the second stage, so it is a dead end rather than a wall.");
        }

        /// <summary>
        /// A stage that takes a few minutes is a session. One that takes an hour is a wall wearing
        /// a disguise, and one that takes seconds is not worth loading.
        /// </summary>
        [Test]
        public void TheFirstStageTakesAReasonableAmountOfTime()
        {
            float seconds = Play(0, HeroLevelFromFormation()).Seconds;

            Assert.Greater(seconds, 10f, "The first stage is over before the player notices it started.");
            Assert.Less(seconds, 600f, "The first stage drags on for more than ten minutes of fighting.");
        }

        // --- The snapshot ---

        /// <summary>
        /// Writes the current balance tables to <c>.claude/balance/snapshot.md</c>.
        ///
        /// This is a generator rather than a check, and it is a test only because that is the
        /// cheapest way to run it with the real assets loaded. Nothing here can fail on its own.
        /// </summary>
        [Test]
        public void WriteTheBalanceSnapshot()
        {
            StringBuilder page = new StringBuilder();

            page.AppendLine("# Snapshot de balanceamento");
            page.AppendLine();
            page.AppendLine("Gerado por `BalanceTests.WriteTheBalanceSnapshot`. **Não edite à mão.**");
            page.AppendLine();
            page.AppendLine("Este arquivo é o retrato dos números de saída do jogo, conforme");
            page.AppendLine("\"## Onde cada número mora\" no `CLAUDE.md`. Ele entra no commit junto com a");
            page.AppendLine("alteração que o mudou, e é o diff dele que responde o que aquela alteração fez");
            page.AppendLine("com o resto do jogo.");
            page.AppendLine();

            AppendPacing(page);
            AppendSheets(page);
            AppendStages(page);

            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".claude", "balance"));
            Directory.CreateDirectory(folder);

            string path = Path.Combine(folder, "snapshot.md");
            File.WriteAllText(path, page.ToString());

            Debug.Log("Balance snapshot written to " + path);
        }

        private static void AppendPacing(StringBuilder page)
        {
            page.AppendLine("## Ritmo de progressão");
            page.AppendLine();
            page.AppendLine("Farmando inimigos do próprio nível, a " + EnemiesPerMinute.ToString(CultureInfo.InvariantCulture)
                + " inimigos por minuto de jogo aberto.");
            page.AppendLine();
            page.AppendLine("| Nível | Inimigos para o próximo | Horas acumuladas |");
            page.AppendLine("|---|---|---|");

            int[] marks = { 5, 10, 25, 50, 75, 90, 100 };

            for (int i = 0; i < marks.Length; i++)
            {
                int level = marks[i];
                double enemies = level < ExperienceTable.MaxLevel
                    ? (double)ExperienceTable.XpToNextLevel(level) / ExperienceTable.XpFromMinion(level)
                    : 0;

                page.AppendLine("| " + level
                    + " | " + Number(enemies, 0)
                    + " | " + Number(HoursToReach(level), 1) + " |");
            }

            page.AppendLine();
        }

        private static void AppendSheets(StringBuilder page)
        {
            page.AppendLine("## Fichas");
            page.AppendLine();
            page.AppendLine("Valores no nível inicial da ficha, sem itens.");
            page.AppendLine();
            page.AppendLine("| Ficha | Vida | Dano fis. | Atq/s | Casas/s | Evasão | Mit. vs nv1 | Mit. vs nv12 | Mit. vs nv50 |");
            page.AppendLine("|---|---|---|---|---|---|---|---|---|");

            CharacterDatabase database = Load<CharacterDatabase>();

            for (int i = 0; i < database.Characters.Count; i++)
            {
                CharacterDefinition definition = database.Characters[i];
                CharacterStats stats = definition.Stats.Clone();
                stats.ApplyInstance(definition.Level, definition.Growth, 1f);

                page.AppendLine("| " + definition.DisplayName
                    + " | " + stats.MaxHealth
                    + " | " + stats.PhysicalDamage
                    + " | " + Number(stats.AttacksPerSecond, 2)
                    + " | " + Number(stats.CellsPerSecond, 2)
                    + " | " + Number(stats.EvasionChance, 1) + "%"
                    + " | " + Number(stats.PhysicalMitigationAgainst(1), 1) + "%"
                    + " | " + Number(stats.PhysicalMitigationAgainst(12), 1) + "%"
                    + " | " + Number(stats.PhysicalMitigationAgainst(50), 1) + "%"
                    + " |");
            }

            page.AppendLine();
        }

        private static void AppendStages(StringBuilder page)
        {
            page.AppendLine("## Fases");
            page.AppendLine();
            page.AppendLine("A formação de heróis é a do asset. O tempo é só de combate, sem as transições.");
            page.AppendLine("\"Nível mínimo\" é o menor nível de herói testado que limpa a fase.");
            page.AppendLine();
            page.AppendLine("| Fase | Nível dos inimigos | Nível mínimo | Tempo | Experiência | Dinheiro |");
            page.AppendLine("|---|---|---|---|---|---|");

            StageDatabase stages = Load<StageDatabase>();
            int[] candidates = { 1, 5, 10, 15, 20, 30, 45, 60 };

            for (int index = 0; index < stages.Stages.Count; index++)
            {
                StageData stage = stages.Load(index);

                string minimum = "acima de " + candidates[candidates.Length - 1];
                StageSimulation.Outcome best = new StageSimulation.Outcome();

                for (int c = 0; c < candidates.Length; c++)
                {
                    StageSimulation.Outcome outcome = Play(index, candidates[c]);

                    if (outcome.Cleared)
                    {
                        minimum = candidates[c].ToString(CultureInfo.InvariantCulture);
                        best = outcome;
                        break;
                    }
                }

                page.AppendLine("| " + stage.name
                    + " | " + stage.enemyLevel
                    + " | " + minimum
                    + " | " + (best.Cleared ? Number(best.Seconds, 1) + " s" : "-")
                    + " | " + (best.Cleared ? best.ExperienceAwarded.ToString(CultureInfo.InvariantCulture) : "-")
                    + " | " + (best.Cleared ? best.MoneyAwarded.ToString(CultureInfo.InvariantCulture) : "-")
                    + " |");
            }

            page.AppendLine();
        }

        // --- Plumbing ---

        private static StageSimulation.Outcome Play(int stageIndex, int heroLevel)
        {
            StageDatabase stages = Load<StageDatabase>();
            CharacterDatabase characters = Load<CharacterDatabase>();
            BattleGridConfig config = Load<BattleGridConfig>();

            return StageSimulation.Run(
                config,
                Formation(),
                heroLevel,
                stages.Load(stageIndex),
                characters.BuildIndex(),
                Seed);
        }

        /// <summary>
        /// The player's formation. Picked by team rather than by being the only one, because the
        /// project still carries the retired enemy formation from before stages existed.
        /// </summary>
        private static BattleFormation HeroFormation()
        {
            string[] found = AssetDatabase.FindAssets("t:BattleFormation");

            for (int i = 0; i < found.Length; i++)
            {
                BattleFormation formation = AssetDatabase.LoadAssetAtPath<BattleFormation>(
                    AssetDatabase.GUIDToAssetPath(found[i]));

                if (formation != null && formation.Team == Team.Heroes)
                {
                    return formation;
                }
            }

            Assert.Fail("No BattleFormation with Team set to Heroes was found.");
            return null;
        }

        private static List<StageSimulation.Placement> Formation()
        {
            BattleFormation formation = HeroFormation();
            List<StageSimulation.Placement> placements = new List<StageSimulation.Placement>();

            for (int i = 0; i < formation.Placements.Count; i++)
            {
                BattleFormation.Placement placement = formation.Placements[i];

                if (placement.Character == null)
                {
                    continue;
                }

                placements.Add(new StageSimulation.Placement
                {
                    Sheet = placement.Character,
                    Column = placement.Column,
                    Row = placement.Row
                });
            }

            Assert.IsNotEmpty(placements, "The hero formation is empty, so nothing can be simulated.");
            return placements;
        }

        /// <summary>The starting level the sheets in the formation actually declare.</summary>
        private static int HeroLevelFromFormation()
        {
            List<StageSimulation.Placement> formation = Formation();
            int level = formation[0].Sheet.Level;

            for (int i = 1; i < formation.Count; i++)
            {
                level = Mathf.Min(level, formation[i].Sheet.Level);
            }

            return level;
        }

        private static double HoursToReach(int level)
        {
            double minutes = 0;

            for (int current = 1; current < level; current++)
            {
                double enemies = (double)ExperienceTable.XpToNextLevel(current) / ExperienceTable.XpFromMinion(current);
                minutes += enemies / EnemiesPerMinute;
            }

            return minutes / 60.0;
        }

        private static string Number(double value, int decimals)
        {
            return value.ToString("F" + decimals, CultureInfo.InvariantCulture);
        }

        private static T Load<T>() where T : ScriptableObject
        {
            string[] found = AssetDatabase.FindAssets("t:" + typeof(T).Name);

            Assert.AreEqual(1, found.Length,
                "Expected exactly one " + typeof(T).Name + " in the project, found " + found.Length + ".");

            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(found[0]));
        }
    }
}
