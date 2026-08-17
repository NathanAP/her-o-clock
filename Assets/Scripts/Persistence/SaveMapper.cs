using System.Collections.Generic;
using HerOClock.Characters;
using HerOClock.Progression;
using UnityEngine;

namespace HerOClock.Persistence
{
    /// <summary>
    /// Moves state between the running game and the save format.
    ///
    /// It is the only place that knows both sides, which keeps the payload classes free of game
    /// logic and the game classes free of the file format.
    /// </summary>
    public static class SaveMapper
    {
        /// <summary>Reads the current state of the game into a payload ready to be written.</summary>
        public static SavePayload Capture(
            IReadOnlyList<Character> heroes,
            PlayerWallet wallet,
            ActivityLog activity,
            string stageId,
            string integrity)
        {
            SavePayload payload = new SavePayload();

            payload.integrity = string.IsNullOrEmpty(integrity) ? SavePayload.IntegrityOk : integrity;
            payload.money = wallet != null ? wallet.Money : 0L;

            payload.stage = new StageSave();
            payload.stage.id = stageId;

            payload.heroes = CaptureHeroes(heroes);
            payload.activity = CaptureActivity(activity);

            return payload;
        }

        private static HeroSave[] CaptureHeroes(IReadOnlyList<Character> heroes)
        {
            if (heroes == null)
            {
                return new HeroSave[0];
            }

            HeroSave[] saved = new HeroSave[heroes.Count];

            for (int i = 0; i < heroes.Count; i++)
            {
                Character hero = heroes[i];
                HeroSave entry = new HeroSave();

                entry.id = hero.Definition.Id;
                entry.level = hero.Progress.Level;
                entry.currentXp = hero.Progress.CurrentXp;
                entry.skillPoints = hero.Progress.SkillPoints;

                entry.automaticAttributes = hero.Attributes.IsAutomatic;
                entry.automaticPoints = hero.Attributes.AutomaticPoints;
                entry.unspentPoints = hero.Attributes.Unspent;

                entry.manualPoints = new int[4];
                for (int a = 0; a < 4; a++)
                {
                    entry.manualPoints[a] = hero.Attributes.ManualOn((Attribute)a);
                }

                saved[i] = entry;
            }

            return saved;
        }

        private static ActivitySave CaptureActivity(ActivityLog activity)
        {
            ActivitySave saved = new ActivitySave();

            if (activity == null)
            {
                return saved;
            }

            IReadOnlyList<ActivityBucket> buckets = activity.Buckets;
            saved.buckets = new ActivityBucketSave[buckets.Count];

            for (int i = 0; i < buckets.Count; i++)
            {
                ActivityBucket bucket = buckets[i];

                saved.buckets[i] = new ActivityBucketSave
                {
                    seconds = bucket.Seconds,
                    experience = bucket.Experience,
                    money = bucket.Money,
                    enemiesDefeated = bucket.EnemiesDefeated,
                    damageDealt = bucket.DamageDealt,
                    damageTaken = bucket.DamageTaken,
                    healing = bucket.Healing
                };
            }

            return saved;
        }

        /// <summary>
        /// Puts a payload back into heroes that have already been created.
        ///
        /// Matching is by the sheet's id, because a save cannot hold an asset reference, and **each
        /// saved entry is used once**: the second hero of a given sheet takes the second entry with
        /// that id. Taking the first match instead is wrong in a way that is quiet and expensive —
        /// a party holding two of the same sheet would give both heroes the same progress and lose
        /// one of them on every load. The formation the game ships with today holds two of each.
        ///
        /// When the save and the content disagree nothing is thrown away: a saved hero whose sheet
        /// no longer exists is reported and skipped, and a hero the save never heard of starts where
        /// its sheet says. Both happen while the game is being built, and neither is the player's
        /// fault.
        ///
        /// The level goes in before the attribute points, since it is the level that decides how
        /// many points the character is owed.
        /// </summary>
        public static void ApplyHeroes(SavePayload payload, IReadOnlyList<Character> heroes)
        {
            if (payload == null || payload.heroes == null || heroes == null)
            {
                return;
            }

            Dictionary<string, Queue<HeroSave>> waiting = ById(payload.heroes);

            for (int i = 0; i < heroes.Count; i++)
            {
                Character hero = heroes[i];
                string id = hero.Definition.Id;

                Queue<HeroSave> queue;

                if (!waiting.TryGetValue(id, out queue) || queue.Count == 0)
                {
                    Debug.Log("Save: the hero '" + id + "' is not in the save, so it starts at the"
                        + " level its sheet declares.");
                    continue;
                }

                Restore(hero, queue.Dequeue());
            }

            foreach (KeyValuePair<string, Queue<HeroSave>> left in waiting)
            {
                if (left.Value.Count > 0)
                {
                    Debug.LogWarning("Save: " + left.Value.Count + " saved hero(es) with the id '"
                        + left.Key + "' have nobody in the party to go to, so their progress was left"
                        + " aside.");
                }
            }
        }

        /// <summary>The saved heroes queued by id, keeping the order they were written in.</summary>
        private static Dictionary<string, Queue<HeroSave>> ById(HeroSave[] saved)
        {
            Dictionary<string, Queue<HeroSave>> waiting = new Dictionary<string, Queue<HeroSave>>();

            for (int i = 0; i < saved.Length; i++)
            {
                HeroSave entry = saved[i];

                if (entry == null || string.IsNullOrEmpty(entry.id))
                {
                    continue;
                }

                Queue<HeroSave> queue;

                if (!waiting.TryGetValue(entry.id, out queue))
                {
                    queue = new Queue<HeroSave>();
                    waiting.Add(entry.id, queue);
                }

                queue.Enqueue(entry);
            }

            return waiting;
        }

        private static void Restore(Character hero, HeroSave entry)
        {
            hero.Progress.Restore(entry.level, entry.currentXp, entry.skillPoints);

            hero.Attributes.Restore(
                entry.automaticAttributes,
                entry.automaticPoints,
                entry.unspentPoints,
                entry.manualPoints);

            // Tops up anything the save is short of, which is what reconciles a file whose points
            // do not add up to the level it claims.
            hero.Attributes.GrantFor(hero.Progress.Level);
        }

        /// <summary>Puts the buckets of the last hour back, exactly as they were.</summary>
        public static void ApplyActivity(SavePayload payload, ActivityLog activity)
        {
            if (payload == null || activity == null)
            {
                return;
            }

            ActivityBucketSave[] saved = payload.activity != null && payload.activity.buckets != null
                ? payload.activity.buckets
                : new ActivityBucketSave[0];

            List<ActivityBucket> buckets = new List<ActivityBucket>(saved.Length);

            for (int i = 0; i < saved.Length; i++)
            {
                if (saved[i] == null)
                {
                    continue;
                }

                buckets.Add(new ActivityBucket
                {
                    Seconds = saved[i].seconds,
                    Experience = saved[i].experience,
                    Money = saved[i].money,
                    EnemiesDefeated = saved[i].enemiesDefeated,
                    DamageDealt = saved[i].damageDealt,
                    DamageTaken = saved[i].damageTaken,
                    Healing = saved[i].healing
                });
            }

            activity.Restore(buckets);
        }

    }
}
