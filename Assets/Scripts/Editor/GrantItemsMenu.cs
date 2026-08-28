using System.Collections.Generic;
using HerOClock.Characters;
using HerOClock.Items;
using HerOClock.Setup;
using UnityEditor;
using UnityEngine;

namespace HerOClock.EditorTools
{
    /// <summary>
    /// Hands every hero in the running session a plain set of gear.
    ///
    /// ## Why this exists
    ///
    /// Nothing drops yet — that is 0.12.0.0 — and there is no inventory to move things around in
    /// either. So without this there is no way at all to watch a fight with equipment on, and the
    /// only evidence that any of it works would be the tests.
    ///
    /// It hands out <see cref="ReferenceKit"/>, the same boring set the balance snapshot measures,
    /// so what is seen on screen and what is written in the snapshot are the same thing.
    ///
    /// It lives in the editor assembly, so **it does not exist in a build**.
    ///
    /// ## Why it takes effect only on the next stage
    ///
    /// It gives the items to the **record**, and a combatant is a photograph taken when its stage
    /// began. That is not a limitation of the tool, it is the rule from `gameplay.md` — changing
    /// equipment mid stage never reaches the board — and the tool obeying it is the point.
    /// </summary>
    public static class GrantItemsMenu
    {
        private const string MenuPath = "Her-o-clock/Grant a reference kit";

        /// <summary>Only while the game runs, since records exist only in a session.</summary>
        [MenuItem(MenuPath, true)]
        private static bool CanGrant()
        {
            return Application.isPlaying;
        }

        [MenuItem(MenuPath)]
        private static void Grant()
        {
            BattleBootstrap bootstrap = Object.FindAnyObjectByType<BattleBootstrap>();

            if (bootstrap == null)
            {
                EditorUtility.DisplayDialog("No battle running",
                    "There is no BattleBootstrap in the scene, so there are no heroes to dress.", "OK");
                return;
            }

            IReadOnlyCollection<HeroRecord> records = bootstrap.LiveRecords;

            if (records == null || records.Count == 0)
            {
                EditorUtility.DisplayDialog("No heroes yet",
                    "No hero has been created in this session yet.", "OK");
                return;
            }

            string itemClass = ClassChosenBy(records);
            int dressed = 0;

            foreach (HeroRecord record in records)
            {
                List<Item> kit = ReferenceKit.Of(itemClass, record.Level);

                for (int i = 0; i < kit.Count; i++)
                {
                    record.Equipment.Put(kit[i]);
                }

                dressed++;
            }

            Debug.Log("GrantItemsMenu: dressed " + dressed + " hero(es) in a " + itemClass
                + " reference kit of their own level. It reaches the board when the next stage starts.");
        }

        /// <summary>
        /// The class of the kit, taken from the first hero's own sheet.
        ///
        /// A kit a hero cannot meet the requirement of would be handed out and immediately
        /// disregarded, which looks exactly like the tool being broken. Matching the sheet is the
        /// choice most likely to actually go on.
        /// </summary>
        private static string ClassChosenBy(IReadOnlyCollection<HeroRecord> records)
        {
            foreach (HeroRecord record in records)
            {
                switch (record.Definition.Stats.Equipment)
                {
                    case EquipmentClass.Heavy: return "heavy";
                    case EquipmentClass.Special: return "special";
                    default: return "light";
                }
            }

            return "light";
        }
    }
}
