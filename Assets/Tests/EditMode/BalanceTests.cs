using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using HerOClock.Battle;
using HerOClock.Characters;
using HerOClock.Items;
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
        /// A fresh team cannot walk through the act. Somewhere in it there is a wall, and that is
        /// what gives farming a reason to exist.
        ///
        /// It asks about the **last** stage rather than a numbered one, because which stage is the
        /// wall is a balance decision that moves. That the act has one at all is not.
        /// </summary>
        [Test]
        public void TheActIsNotWalkableByAFreshTeam()
        {
            StageDatabase stages = Load<StageDatabase>();
            StageSimulation.Outcome outcome = Play(stages.Stages.Count - 1, 1);

            Assert.IsFalse(outcome.Cleared,
                "A brand new team clears the whole act, so nothing in the game asks it to farm.");
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
            AppendWeapons(page);
            AppendEquipment(page);
            AppendStages(page);
            AppendSweep(page);

            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".claude", "balance"));
            Directory.CreateDirectory(folder);

            string path = Path.Combine(folder, "snapshot.md");
            File.WriteAllText(path, page.ToString());

            Debug.Log("Balance snapshot written to " + path);
        }

        /// <summary>
        /// A player who never opened the attributes screen gets past a stage at the level it
        /// recommends.
        ///
        /// The sheet's own distribution is the baseline the content was written against, and it is
        /// the only build the game can promise anything about. The corners swept alongside it —
        /// every point in one attribute — are deliberately bad and are measured, not promised.
        /// </summary>
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void TheSheetsOwnBuildClearsAStageAtTheLevelItRecommends(int index)
        {
            StageData stage = Load<StageDatabase>().Load(index);
            int high = RecommendedLevels[index][1];

            List<StageSweep.BuildResult> perBuild = StageSweep.PerBuild(
                Load<BattleGridConfig>(), Formation(), high, stage,
                Load<CharacterDatabase>().BuildIndex(), StageSweep.BuildsAt(high));

            Assert.IsTrue(perBuild[0].ClearedAny,
                "At level " + high + ", which " + stage.id + " recommends, the sheet's own build "
                + "cleared none of its " + perBuild[0].Tried + " attempts.");
        }

        /// <summary>
        /// No stage depends on a single build.
        ///
        /// This is the project's own promise, from the flexibility `CLAUDE.md` asks for: a player
        /// who built something other than the obvious answer should still have a game. A stage only
        /// one build gets through is the failure that promise is about.
        /// </summary>
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void MoreThanOneBuildClearsEveryStage(int index)
        {
            StageData stage = Load<StageDatabase>().Load(index);
            int high = RecommendedLevels[index][1];

            if (StageSweep.BuildsAt(high) < 2)
            {
                Assert.Ignore("At level " + high + " no points have been spent yet, so there is only "
                    + "one build to have. Diversity is not a thing that exists here to measure.");
            }

            List<StageSweep.BuildResult> perBuild = StageSweep.PerBuild(
                Load<BattleGridConfig>(), Formation(), high, stage,
                Load<CharacterDatabase>().BuildIndex(), StageSweep.BuildsAt(high));

            int worked = 0;

            for (int i = 0; i < perBuild.Count; i++)
            {
                if (perBuild[i].ClearedAny)
                {
                    worked++;
                }
            }

            Assert.Greater(worked, 1,
                stage.id + " is cleared by " + worked + " of the " + perBuild.Count
                + " builds swept, so it asks for one answer instead of a level.");
        }

        /// <summary>
        /// Below the range, the level has to matter. A stage every party clears two levels early is
        /// a stage whose recommended level says nothing.
        ///
        /// Skipped where the range already starts at 1, since there is nothing below it.
        /// </summary>
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void NotEveryPartyClearsAStageBelowTheLevelItRecommends(int index)
        {
            StageData stage = Load<StageDatabase>().Load(index);
            int below = RecommendedLevels[index][0] - 1;

            if (below < 1)
            {
                Assert.Ignore("The stage recommends level 1, so there is no level below it.");
            }

            StageSweep.Result result = StageSweep.At(
                Load<BattleGridConfig>(), Formation(), below, stage,
                Load<CharacterDatabase>().BuildIndex(), StageSweep.BuildsAt(RecommendedLevels[index][1]));

            Assert.Less(result.ClearedFraction, 1f,
                "Every one of the " + result.Tried + " sampled parties cleared " + stage.id
                + " at level " + below + ", below what it recommends, so the level asks for nothing.");
        }

        /// <summary>
        /// The recommended level range of each stage, copied from `stages.md`.
        ///
        /// It is the spec's number repeated here on purpose, as "## Onde cada número mora" in
        /// CLAUDE.md asks: two independent statements of the same intent, so that a disagreement
        /// between them shows up instead of being invisible.
        /// </summary>
        private static readonly int[][] RecommendedLevels =
        {
            new[] { 1, 1 },
            new[] { 2, 3 },
            new[] { 4, 5 },
            new[] { 5, 6 }
        };

        /// <summary>
        /// How many of many different parties clear each stage, level by level.
        ///
        /// This replaces "the minimum level is 4", which was a sample of one wearing the clothes of
        /// a fact: one party, the sheet's own build, one seed. Two players with different builds
        /// have different afternoons on the same stage, and that spread is the thing worth writing
        /// down.
        /// </summary>
        private static void AppendSweep(StringBuilder page)
        {
            StageDatabase stages = Load<StageDatabase>();
            CharacterDatabase characters = Load<CharacterDatabase>();
            BattleGridConfig config = Load<BattleGridConfig>();

            page.AppendLine("## Quantas equipes limpam cada fase");
            page.AppendLine();
            page.AppendLine("Cada linha roda várias equipes, várias builds e várias sementes na mesma");
            page.AppendLine("fase e no mesmo nível, e conta quantas limparam.");
            page.AppendLine();
            page.AppendLine("**Isto é uma amostra, e nunca uma prova.** O espaço de equipes, builds,");
            page.AppendLine("ordens e, mais adiante, itens e árvores é grande demais para ser coberto.");
            page.AppendLine("O que a tabela mostra é a forma: no nível recomendado a maioria passa, e");
            page.AppendLine("alguns níveis abaixo a maioria não passa.");
            page.AppendLine();
            page.AppendLine("A amostra cresce junto com o espaço. No começo não há o que variar: um herói");
            page.AppendLine("no nível 1 não tem ponto nenhum para distribuir.");
            page.AppendLine();

            for (int index = 0; index < stages.Stages.Count; index++)
            {
                StageData stage = stages.Load(index);

                if (stage == null)
                {
                    continue;
                }

                int low = index < RecommendedLevels.Length ? RecommendedLevels[index][0] : 1;
                int high = index < RecommendedLevels.Length ? RecommendedLevels[index][1] : low;

                page.AppendLine("### " + stage.id + " (recomendado " + low + " a " + high + ")");
                page.AppendLine();
                page.AppendLine("| Nível | Limparam | Testadas | % |");
                page.AppendLine("|---|---|---|---|");

                int from = low - 2 < 1 ? 1 : low - 2;

                // Fixed for the whole table, so the rows can be read against each other.
                int builds = StageSweep.BuildsAt(high);

                for (int level = from; level <= high + 2; level++)
                {
                    StageSweep.Result result = StageSweep.At(
                        config, Formation(), level, stage, characters.BuildIndex(), builds);

                    page.AppendLine("| " + level + " | " + result.Cleared + " | " + result.Tried + " | "
                        + (result.ClearedFraction * 100f).ToString("0", CultureInfo.InvariantCulture) + "% |");
                }

                page.AppendLine();
                page.AppendLine("Por build, no nível " + high + ":");
                page.AppendLine();
                page.AppendLine("| Build | Limpou | Tentativas |");
                page.AppendLine("|---|---|---|");

                List<StageSweep.BuildResult> perBuild = StageSweep.PerBuild(
                    config, Formation(), high, stage, characters.BuildIndex(), builds);

                for (int b = 0; b < perBuild.Count; b++)
                {
                    page.AppendLine("| " + perBuild[b].Name + " | " + perBuild[b].Cleared
                        + " | " + perBuild[b].Tried + " |");
                }

                page.AppendLine();
            }
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
            page.AppendLine("| Ficha | Vida | Dano fis. | Atq/s | Casas/s | Evasão (pts) | Armadura | Regen/s |");
            page.AppendLine("|---|---|---|---|---|---|---|---|");

            CharacterDatabase database = Load<CharacterDatabase>();

            for (int i = 0; i < database.Characters.Count; i++)
            {
                CharacterDefinition definition = database.Characters[i];
                CharacterStats stats = AtLevel(definition, definition.Level);

                page.AppendLine("| " + definition.Id
                    + " | " + stats.MaxHealth
                    + " | " + Mathf.RoundToInt(stats.PhysicalDamageMin) + "-" + Mathf.RoundToInt(stats.PhysicalDamageMax)
                    + " | " + Number(stats.AttacksPerSecond, 2)
                    + " | " + Number(stats.CellsPerSecond, 2)
                    + " | " + stats.EvasionPoints
                    + " | " + stats.PhysicalArmor
                    + " | " + Number(stats.HealthPerSecond, 2)
                    + " |");
            }

            page.AppendLine();
            AppendMitigation(page, database);
        }

        /// <summary>
        /// Mitigation is reported against an attacker of the character's own level, because that
        /// is the number the growth rule exists to hold still. Reading it at a fixed level against
        /// varying attackers, which is what this table used to do, hides the whole property.
        /// </summary>
        private static void AppendMitigation(StringBuilder page, CharacterDatabase database)
        {
            int[] levels = { 1, 12, 30, 50, 100 };

            page.AppendLine("### Defesa contra um atacante do mesmo nível");
            page.AppendLine();
            page.AppendLine("Mitigação física e chance de evasão, que passam pela mesma curva contra a mesma");
            page.AppendLine("constante. Uma linha parada significa que aquela defesa acompanha a curva. Uma linha");
            page.AppendLine("que cai significa que a ficha perde aquela defesa conforme o jogo avança.");
            page.AppendLine();

            page.Append("| Ficha | Defesa |");
            for (int i = 0; i < levels.Length; i++)
            {
                page.Append(" Nível ").Append(levels[i]).Append(" |");
            }
            page.AppendLine();

            page.Append("|---|---|");
            for (int i = 0; i < levels.Length; i++)
            {
                page.Append("---|");
            }
            page.AppendLine();

            for (int i = 0; i < database.Characters.Count; i++)
            {
                CharacterDefinition definition = database.Characters[i];
                page.Append("| ").Append(definition.Id).Append(" | armadura |");

                for (int l = 0; l < levels.Length; l++)
                {
                    int level = Mathf.Min(levels[l], definition.MaxLevel);
                    CharacterStats stats = AtLevel(definition, level);

                    page.Append(' ').Append(Number(stats.PhysicalMitigationAgainst(level), 1)).Append("% |");
                }

                page.AppendLine();
                page.Append("| ").Append(definition.Id).Append(" | evasão |");

                for (int l = 0; l < levels.Length; l++)
                {
                    int level = Mathf.Min(levels[l], definition.MaxLevel);
                    CharacterStats stats = AtLevel(definition, level);

                    page.Append(' ').Append(Number(stats.EvasionChanceAgainst(level), 1)).Append("% |");
                }

                page.AppendLine();
            }

            page.AppendLine();
        }

        private static CharacterStats AtLevel(CharacterDefinition definition, int level)
        {
            CharacterStats stats = definition.Stats.Clone();
            stats.ApplyInstance(level, definition.Growth, 1f);
            return stats;
        }

        /// <summary>Highest hero level the search will consider before giving up on a stage.</summary>
        private const int HighestLevelSearched = 80;

        /// <summary>
        /// The lowest hero level that clears the stage, found by halving the range.
        ///
        /// Halving is safe here because a higher level is strictly better in every way that
        /// decides a fight: more health, more damage and more armour. Returns zero when even the
        /// highest level searched cannot do it.
        /// </summary>
        private static int MinimumLevelToClear(int stageIndex)
        {
            if (!Play(stageIndex, HighestLevelSearched).Cleared)
            {
                return 0;
            }

            int low = 1;
            int high = HighestLevelSearched;

            while (low < high)
            {
                int middle = (low + high) / 2;

                if (Play(stageIndex, middle).Cleared)
                {
                    high = middle;
                }
                else
                {
                    low = middle + 1;
                }
            }

            return low;
        }

        /// <summary>
        /// What a plain set of gear is worth, with no modifier on it.
        ///
        /// Nothing drops yet, so this is the only way to ask the question at all, and the kit is
        /// deliberately boring: base defence only. Anything else would mean inventing the generator
        /// that 0.11.5.0 will actually write, and measuring against a guess.
        ///
        /// The **active** column is the one to watch. A kit of the hero's own level demands 1.5
        /// times that level in one attribute, and a hero who spread its points cannot meet that on
        /// every piece — so the requirement starts biting on its own, without anybody tuning it.
        ///
        /// The kit carries a one handed weapon of its own class, so the offensive table below
        /// measures a weapon rather than a fist. One handed on purpose: a two hander would empty
        /// the off hand and move the defensive answer for a reason that has nothing to do with it.
        /// </summary>
        private static void AppendEquipment(StringBuilder page)
        {
            TextAsset file = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Items/slots.json");

            if (file == null)
            {
                return;
            }

            ItemRules rules = ItemRulesFromDisk();
            CharacterDefinition hero = SheetOf("tempo");

            if (hero == null)
            {
                return;
            }

            int[] levels = { 1, 12, 30, 50, 100 };
            string[] classes = { "light", "heavy", "special" };

            page.AppendLine("## O kit de referência");
            page.AppendLine();
            page.AppendLine("A ficha da Tempo vestindo um kit sem modificador nenhum, do mesmo nível que ela:");
            page.AppendLine("os quatro cascos e a mão secundária, só com a defesa base da classe.");
            page.AppendLine();
            page.AppendLine("A coluna **Ativos** é a que interessa. Um kit do próprio nível exige 1.5 vezes esse");
            page.AppendLine("nível num atributo, e uma heroína que espalhou os pontos não atende isso em tudo.");
            page.AppendLine();
            page.AppendLine("| Nível | Kit | Ativos | Armadura | Mitigação | Evasão (pts) | Evasão | Resist. |");
            page.AppendLine("|---|---|---|---|---|---|---|---|");

            for (int l = 0; l < levels.Length; l++)
            {
                int level = Mathf.Min(levels[l], hero.MaxLevel);

                page.AppendLine(Row(hero, level, null, rules, "sem kit", 0));

                for (int c = 0; c < classes.Length; c++)
                {
                    List<Item> kit = ReferenceKit.Of(classes[c], level);

                    Equipment worn = new Equipment();

                    for (int i = 0; i < kit.Count; i++)
                    {
                        worn.Put(kit[i], rules);
                    }

                    page.AppendLine(Row(hero, level, worn, rules, classes[c], kit.Count));
                }
            }

            page.AppendLine();

            AppendKitOffence(page, hero, rules);
        }

        /// <summary>
        /// What each weapon subtype is worth on its own: the range it swings for, how often, and
        /// the two multiplied together.
        ///
        /// This is the table that answers "is a Piledriver still a choice next to a Claw". That the
        /// derivation holds is asserted by `ItemContentTests.EveryWeaponMeetsItsFamilyBudget`; what
        /// is published here is the consequence, which is a number of output and belongs nowhere
        /// else.
        /// </summary>
        private static void AppendWeapons(StringBuilder page)
        {
            TextAsset file = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Items/subtypes.json");

            if (file == null)
            {
                return;
            }

            ItemSubtypes subtypes = JsonUtility.FromJson<ItemSubtypes>(file.text);

            if (subtypes == null || subtypes.weapons == null)
            {
                return;
            }

            page.AppendLine("## As armas");
            page.AppendLine();
            page.AppendLine("O que cada subtipo rende sozinho, antes de POW e de AGI. O dano por segundo é o");
            page.AppendLine("dano médio vezes a velocidade, que é o orçamento que a família paga.");
            page.AppendLine();
            page.AppendLine("| Subtipo | Mãos | Alcance | Atq/s | Dano nv. 1 | DPS nv. 1 | Dano nv. 100 | DPS nv. 100 |");
            page.AppendLine("|---|---|---|---|---|---|---|---|");

            for (int i = 0; i < subtypes.weapons.Length; i++)
            {
                WeaponSubtype weapon = subtypes.weapons[i];

                page.AppendLine("| " + weapon.id
                    + " | " + weapon.hands
                    + " | " + weapon.minRange + "-" + weapon.maxRange
                    + " | " + Number(weapon.attackSpeed, 2)
                    + " | " + Band(weapon, 1)
                    + " | " + Number(weapon.damage.AverageAt(1) * weapon.attackSpeed, 1)
                    + " | " + Band(weapon, 100)
                    + " | " + Number(weapon.damage.AverageAt(100) * weapon.attackSpeed, 1)
                    + " |");
            }

            page.AppendLine();
        }

        private static string Band(WeaponSubtype weapon, int level)
        {
            return Number(weapon.damage.min.At(level), 0) + "-" + Number(weapon.damage.max.At(level), 0);
        }

        /// <summary>
        /// What the same reference kit does to the hero on the offensive side: the blow it swings
        /// for, how often, and the two multiplied.
        ///
        /// Kept apart from the defensive table because they answer different questions and share
        /// only the kit. One table of thirteen columns would answer neither.
        /// </summary>
        private static void AppendKitOffence(StringBuilder page, CharacterDefinition hero, ItemRules rules)
        {
            int[] levels = { 1, 12, 30, 50, 100 };
            string[] classes = { "light", "heavy", "special" };

            page.AppendLine("A mesma Tempo, do lado ofensivo. Uma arma inativa devolve o soco da ficha, então");
            page.AppendLine("uma linha igual à do kit ausente quer dizer que a arma não passou no requerimento.");
            page.AppendLine();
            page.AppendLine("| Nível | Kit | Mãos | Dano | Atq/s | DPS |");
            page.AppendLine("|---|---|---|---|---|---|");

            for (int l = 0; l < levels.Length; l++)
            {
                int level = Mathf.Min(levels[l], hero.MaxLevel);

                page.AppendLine(OffenceRow(hero, level, null, rules, "sem kit"));

                for (int c = 0; c < classes.Length; c++)
                {
                    Equipment worn = new Equipment();
                    List<Item> kit = ReferenceKit.Of(classes[c], level);

                    for (int i = 0; i < kit.Count; i++)
                    {
                        worn.Put(kit[i], rules);
                    }

                    page.AppendLine(OffenceRow(hero, level, worn, rules, classes[c]));
                }
            }

            page.AppendLine();
        }

        private static string OffenceRow(
            CharacterDefinition hero, int level, Equipment worn, ItemRules rules, string label)
        {
            CharacterStats stats = Dressed(hero, level, worn, rules);

            return "| " + level
                + " | " + label
                + " | " + stats.HandCount
                + " | " + Number(stats.PhysicalDamageMin, 1) + "-" + Number(stats.PhysicalDamageMax, 1)
                + " | " + Number(stats.AttacksPerSecond, 2)
                + " | " + Number(stats.AveragePhysicalDamage * stats.AttacksPerSecond, 1)
                + " |";
        }

        /// <summary>The hero at a level, wearing what it is handed. Null means bare.</summary>
        private static CharacterStats Dressed(
            CharacterDefinition hero, int level, Equipment worn, ItemRules rules)
        {
            CharacterStats stats = AtLevel(hero, level);

            if (worn == null)
            {
                return stats;
            }

            int[] without =
            {
                stats.TotalOf(Attribute.Power),
                stats.TotalOf(Attribute.Agility),
                stats.TotalOf(Attribute.Specialty),
                stats.TotalOf(Attribute.Constitution)
            };

            EquipmentResolution resolved = worn.Resolve(rules, without);

            stats.UseEquipment(
                EquipmentComposition.Of(resolved.Classes, stats.Equipment),
                resolved.Totals,
                WeaponBuilder.Build(resolved.Active, rules));

            return stats;
        }

        /// <summary>The item tables as the game loads them, subtypes included.</summary>
        private static ItemRules ItemRulesFromDisk()
        {
            TextAsset slots = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Items/slots.json");
            TextAsset subtypes = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Items/subtypes.json");

            return new ItemRules(
                JsonUtility.FromJson<ItemSlots>(slots.text),
                subtypes != null ? JsonUtility.FromJson<ItemSubtypes>(subtypes.text) : null);
        }

        private static string Row(
            CharacterDefinition hero, int level, Equipment worn, ItemRules rules, string label, int pieces)
        {
            CharacterStats stats = AtLevel(hero, level);
            int active = 0;

            if (worn != null)
            {
                int[] without =
                {
                    stats.TotalOf(Attribute.Power),
                    stats.TotalOf(Attribute.Agility),
                    stats.TotalOf(Attribute.Specialty),
                    stats.TotalOf(Attribute.Constitution)
                };

                EquipmentResolution resolved = worn.Resolve(rules, without);
                active = resolved.Active.Count;

                stats.UseEquipment(
                    EquipmentComposition.Of(resolved.Classes, stats.Equipment), resolved.Totals);
            }

            return "| " + level
                + " | " + label
                + " | " + (worn == null ? "-" : active + "/" + pieces)
                + " | " + stats.PhysicalArmor
                + " | " + Number(stats.PhysicalMitigationAgainst(level), 1) + "%"
                + " | " + stats.EvasionPoints
                + " | " + Number(stats.EvasionChanceAgainst(level), 1) + "%"
                + " | " + stats.FireResistance
                + " |";
        }

        private static CharacterDefinition SheetOf(string id)
        {
            CharacterDatabase database = Load<CharacterDatabase>();

            for (int i = 0; i < database.Characters.Count; i++)
            {
                if (database.Characters[i] != null && database.Characters[i].Id == id)
                {
                    return database.Characters[i];
                }
            }

            return null;
        }

        private static void AppendStages(StringBuilder page)
        {
            page.AppendLine("## Fases");
            page.AppendLine();
            page.AppendLine("A formação de heróis é a do asset. O tempo é só de combate, sem as transições.");
            page.AppendLine("\"Nível mínimo\" é o menor nível de herói que limpa a fase.");
            page.AppendLine();
            page.AppendLine("| Fase | Nível dos inimigos | Nível mínimo | Tempo | Experiência | Dinheiro |");
            page.AppendLine("|---|---|---|---|---|---|");

            StageDatabase stages = Load<StageDatabase>();

            for (int index = 0; index < stages.Stages.Count; index++)
            {
                StageData stage = stages.Load(index);
                int minimum = MinimumLevelToClear(index);

                StageSimulation.Outcome outcome = minimum > 0
                    ? Play(index, minimum)
                    : new StageSimulation.Outcome();

                page.AppendLine("| " + stage.id
                    + " | " + stage.enemyLevel
                    + " | " + (minimum > 0 ? minimum.ToString(CultureInfo.InvariantCulture) : "acima de " + HighestLevelSearched)
                    + " | " + (outcome.Cleared ? Number(outcome.Seconds, 1) + " s" : "-")
                    + " | " + (outcome.Cleared ? outcome.ExperienceAwarded.ToString(CultureInfo.InvariantCulture) : "-")
                    + " | " + (outcome.Cleared ? outcome.MoneyAwarded.ToString(CultureInfo.InvariantCulture) : "-")
                    + " |");
            }

            page.AppendLine();
            AppendWalls(page, stages);
        }

        /// <summary>
        /// Where the player is forced to stop and farm.
        ///
        /// The table above answers "what level clears this stage". This one answers the question
        /// that actually matters: **with what level does the player arrive there**. A stage that
        /// needs level 15 is only a wall if you reach it at level 3, and how big a wall depends on
        /// how much the stage before it pays.
        ///
        /// The walk is the honest one: start at the level the sheets begin at, clear each stage
        /// once, carry the experience forward. When the next stage asks for more than you have,
        /// that is a wall, and the cost of it is measured in repeats of the stage before.
        /// </summary>
        private static void AppendWalls(StringBuilder page, StageDatabase stages)
        {
            page.AppendLine("### Paredes");
            page.AppendLine();
            page.AppendLine("Jogando as fases em ordem e limpando cada uma uma vez. \"Chega com\" é o nível");
            page.AppendLine("que o time tem ao encostar na fase; \"exige\" é o nível que ela pede. Quando o");
            page.AppendLine("exigido passa o de chegada, o jogador é obrigado a parar e farmar.");
            page.AppendLine();
            page.AppendLine("| Fase | Chega com | Exige | Parede | Custo |");
            page.AppendLine("|---|---|---|---|---|");

            int level = HeroLevelFromFormation();

            for (int index = 0; index < stages.Stages.Count; index++)
            {
                StageData stage = stages.Load(index);
                int required = MinimumLevelToClear(index);
                int arrival = level;

                string wall = "não";
                string cost = "-";

                if (required <= 0)
                {
                    wall = "sim";
                    cost = "não vencível até o nível " + HighestLevelSearched;
                }
                else if (arrival < required)
                {
                    wall = "sim";
                    cost = index == 0
                        ? "a primeira fase já é uma parede"
                        : CostOfFarming(index - 1, arrival, required);

                    // Past the wall, the player farmed their way to the level it asked for.
                    level = required;
                }

                page.AppendLine("| " + stage.id
                    + " | " + arrival
                    + " | " + (required > 0 ? required.ToString(CultureInfo.InvariantCulture) : "?")
                    + " | " + wall
                    + " | " + cost + " |");

                if (required <= 0)
                {
                    break;
                }

                level = Play(index, level).FirstHeroLevel;
            }

            page.AppendLine();
            page.AppendLine("O custo conta só o tempo de combate, e despreza a experiência parcial que sobra");
            page.AppendLine("de um nível para o outro. As transições entre ondas somam vários segundos por");
            page.AppendLine("repetição, então o tempo real de relógio é maior que o mostrado.");
            page.AppendLine();
            page.AppendLine("**As fases que existem hoje são casos de teste, não conteúdo.** A segunda tem");
            page.AppendLine("inimigos de nível 12 de propósito, para exercitar herói caindo em combate e avanço");
            page.AppendLine("bloqueado. A parede gigante entre as duas é o resultado esperado desse par, e não");
            page.AppendLine("um problema de balanceamento. Ver `game-objects/fases.md`.");
            page.AppendLine();
        }

        /// <summary>
        /// How many clears of the given stage it takes to go from the current level to the one
        /// the next stage asks for, and how long that is in fighting time.
        /// </summary>
        private static string CostOfFarming(int farmStageIndex, int fromLevel, int targetLevel)
        {
            StageSimulation.Outcome run = Play(farmStageIndex, fromLevel);

            if (!run.Cleared || run.ExperienceAwarded <= 0)
            {
                return "a fase anterior não paga nada";
            }

            long needed = ExperienceTable.TotalXpTo(targetLevel) - ExperienceTable.TotalXpTo(fromLevel);

            if (needed <= 0)
            {
                return "-";
            }

            int repeats = (int)((needed + run.ExperienceAwarded - 1) / run.ExperienceAwarded);
            double minutes = repeats * run.Seconds / 60.0;

            return repeats + "x a fase anterior, " + Number(minutes, 1) + " min de combate";
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
