using System.Collections.Generic;
using HerOClock.Characters;
using HerOClock.Items;
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
            IReadOnlyList<HeroRecord> heroes,
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
            payload.items = CaptureItems(heroes);
            payload.activity = CaptureActivity(activity);

            return payload;
        }

        private static HeroSave[] CaptureHeroes(IReadOnlyList<HeroRecord> heroes)
        {
            if (heroes == null)
            {
                return new HeroSave[0];
            }

            HeroSave[] saved = new HeroSave[heroes.Count];

            for (int i = 0; i < heroes.Count; i++)
            {
                HeroRecord hero = heroes[i];
                HeroSave entry = new HeroSave();

                entry.id = hero.Id;
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

                entry.equipment = CaptureSlots(hero);

                saved[i] = entry;
            }

            return saved;
        }

        /// <summary>Which item is in which slot, by id. The items themselves go in the flat list.</summary>
        private static EquippedSave[] CaptureSlots(HeroRecord hero)
        {
            List<EquippedSave> worn = new List<EquippedSave>();

            foreach (Item item in hero.Equipment.Worn)
            {
                worn.Add(new EquippedSave { slot = item.SlotId, itemId = item.Id });
            }

            // Sorted so two saves of the same state come out byte for byte the same. The dictionary
            // behind the equipment makes no promise about order, and the file is signed.
            worn.Sort((left, right) => string.CompareOrdinal(left.slot, right.slot));

            return worn.ToArray();
        }

        /// <summary>
        /// Every item the player owns, written once.
        ///
        /// Today that is only what is being worn. When the chest arrives its contents join the same
        /// list, and nothing about the shape has to change — which is exactly why the list is flat
        /// and the holders refer to it.
        /// </summary>
        private static ItemSave[] CaptureItems(IReadOnlyList<HeroRecord> heroes)
        {
            List<ItemSave> saved = new List<ItemSave>();

            if (heroes == null)
            {
                return saved.ToArray();
            }

            HashSet<string> written = new HashSet<string>();

            for (int i = 0; i < heroes.Count; i++)
            {
                foreach (Item item in heroes[i].Equipment.Worn)
                {
                    if (!written.Add(item.Id))
                    {
                        continue;
                    }

                    saved.Add(Write(item));
                }
            }

            saved.Sort((left, right) => string.CompareOrdinal(left.id, right.id));
            return saved.ToArray();
        }

        private static ItemSave Write(Item item)
        {
            ItemSave saved = new ItemSave
            {
                id = item.Id,
                slot = item.SlotId,
                itemClass = item.ClassId,
                subtype = item.SubtypeId,
                technology = item.TechnologyId,
                level = item.Level
            };

            saved.modifiers = new ItemModifierSave[item.Modifiers.Count];

            for (int i = 0; i < item.Modifiers.Count; i++)
            {
                ItemModifierRoll roll = item.Modifiers[i];

                saved.modifiers[i] = new ItemModifierSave
                {
                    id = roll.Id,
                    family = roll.Family,
                    tier = roll.Tier,
                    value = roll.Value,
                    valueMax = roll.ValueMax
                };
            }

            return saved;
        }

        /// <summary>
        /// Puts the items back on the heroes.
        ///
        /// Called after <see cref="ApplyHeroes"/>, and separately from it, because it needs the
        /// payload's whole item list and not one hero's slice of it.
        ///
        /// An id in a slot with no item behind it is skipped and reported. It means the file was
        /// edited or truncated, and inventing an item to fill the gap would be worse than the hero
        /// arriving with an empty slot.
        /// </summary>
        public static void ApplyItems(SavePayload payload, IReadOnlyList<HeroRecord> heroes)
        {
            if (payload == null || heroes == null)
            {
                return;
            }

            Dictionary<string, Item> byId = new Dictionary<string, Item>();

            if (payload.items != null)
            {
                for (int i = 0; i < payload.items.Length; i++)
                {
                    Item item = Read(payload.items[i]);

                    if (item != null)
                    {
                        byId[item.Id] = item;
                    }
                }
            }

            Dictionary<string, Queue<HeroSave>> waiting = ById(payload.heroes ?? new HeroSave[0]);

            for (int i = 0; i < heroes.Count; i++)
            {
                Queue<HeroSave> queue;

                if (!waiting.TryGetValue(heroes[i].Id, out queue) || queue.Count == 0)
                {
                    continue;
                }

                HeroSave saved = queue.Dequeue();

                if (saved.equipment == null)
                {
                    continue;
                }

                for (int e = 0; e < saved.equipment.Length; e++)
                {
                    EquippedSave slot = saved.equipment[e];
                    Item item;

                    if (!byId.TryGetValue(slot.itemId ?? string.Empty, out item))
                    {
                        Debug.LogWarning("Save: the hero '" + heroes[i].Id + "' had the item '"
                            + slot.itemId + "' in " + slot.slot + ", and that item is not in the file.");
                        continue;
                    }

                    heroes[i].Equipment.Put(item);
                }
            }
        }

        private static Item Read(ItemSave saved)
        {
            if (saved == null || string.IsNullOrEmpty(saved.id))
            {
                return null;
            }

            List<ItemModifierRoll> rolls = new List<ItemModifierRoll>();

            if (saved.modifiers != null)
            {
                for (int i = 0; i < saved.modifiers.Length; i++)
                {
                    ItemModifierSave roll = saved.modifiers[i];
                    rolls.Add(new ItemModifierRoll(
                        roll.id, roll.family, roll.tier, roll.value, roll.valueMax));
                }
            }

            return new Item(saved.id, saved.slot, saved.itemClass, saved.subtype,
                saved.technology, saved.level, rolls);
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
        /// Puts a payload back into the hero records.
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
        /// It runs before any combatant exists, and that is required rather than incidental. A
        /// combatant is a photograph of a record, so every record has to be finished before the
        /// first one is taken.
        ///
        /// The level goes in before the attribute points, since it is the level that decides how
        /// many points the character is owed.
        /// </summary>
        public static void ApplyHeroes(SavePayload payload, IReadOnlyList<HeroRecord> heroes)
        {
            if (payload == null || payload.heroes == null || heroes == null)
            {
                return;
            }

            Dictionary<string, Queue<HeroSave>> waiting = ById(payload.heroes);

            for (int i = 0; i < heroes.Count; i++)
            {
                HeroRecord hero = heroes[i];
                string id = hero.Id;

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
                        + left.Key + "' have no record to go to, so their progress was left aside.");
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

        private static void Restore(HeroRecord hero, HeroSave entry)
        {
            hero.Restore(entry.level, entry.currentXp, entry.skillPoints);

            hero.Attributes.Restore(
                entry.automaticAttributes,
                entry.automaticPoints,
                entry.unspentPoints,
                entry.manualPoints);

            // Tops up anything the save is short of, which is what reconciles a file whose points
            // do not add up to the level it claims.
            hero.Attributes.GrantFor(hero.Progress.Level);

            // There is no health here, and there is nothing to leave out either: a record has
            // none. Health belongs to a combatant, and a combatant is built after this runs,
            // already whole, at the maximum the restored level and points produce.
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


        /// <summary>Writes the team, the bench and the positions that have been unlocked.</summary>
        public static RosterSave ToSave(Roster roster, IReadOnlyList<string> clearedStages)
        {
            RosterSave saved = new RosterSave();

            if (roster == null)
            {
                return saved;
            }

            saved.slots = roster.Slots;
            saved.owned = new List<string>(roster.Owned).ToArray();
            saved.team = new List<string>(roster.Team).ToArray();
            saved.clearedStages = clearedStages == null
                ? new string[0]
                : new List<string>(clearedStages).ToArray();

            return saved;
        }

        /// <summary>
        /// Puts the roster back, rebuilding it when the file predates it.
        ///
        /// A save written before the roster existed carries zero slots, and everything it knows
        /// about the team is the list of heroes it saved. Treating those as the team, with one
        /// position each, lands exactly where that player left off.
        /// </summary>
        public static void ApplyRoster(SavePayload payload, Roster roster, List<string> clearedStages)
        {
            if (payload == null || roster == null)
            {
                return;
            }

            RosterSave saved = payload.roster ?? new RosterSave();

            if (saved.slots <= 0)
            {
                List<string> fromHeroes = new List<string>();

                for (int i = 0; payload.heroes != null && i < payload.heroes.Length; i++)
                {
                    if (payload.heroes[i] != null && !string.IsNullOrWhiteSpace(payload.heroes[i].id)
                        && !fromHeroes.Contains(payload.heroes[i].id))
                    {
                        fromHeroes.Add(payload.heroes[i].id);
                    }
                }

                roster.Restore(fromHeroes.Count, fromHeroes, fromHeroes);
            }
            else
            {
                roster.Restore(saved.slots, saved.owned, saved.team);
            }

            if (clearedStages != null)
            {
                clearedStages.Clear();

                for (int i = 0; saved.clearedStages != null && i < saved.clearedStages.Length; i++)
                {
                    clearedStages.Add(saved.clearedStages[i]);
                }
            }
        }
    }
}
