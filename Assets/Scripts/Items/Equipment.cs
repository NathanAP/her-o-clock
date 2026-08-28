using System.Collections.Generic;
using HerOClock.Characters;

namespace HerOClock.Items
{
    /// <summary>
    /// What a hero is wearing, one item per slot, and the rule that decides which of them count.
    ///
    /// It belongs to the **record** and never to the combatant. A player re-equipping a hero in the
    /// middle of a stage changes this, and what is fighting does not notice, because the combatant
    /// took its photograph when the stage began.
    /// </summary>
    public class Equipment
    {
        private readonly Dictionary<string, Item> bySlot = new Dictionary<string, Item>();

        public IEnumerable<Item> Worn
        {
            get { return bySlot.Values; }
        }

        public int Count
        {
            get { return bySlot.Count; }
        }

        public Item In(string slotId)
        {
            Item item;
            return bySlot.TryGetValue(slotId, out item) ? item : null;
        }

        /// <summary>Puts an item on, replacing whatever was in that slot.</summary>
        public void Put(Item item)
        {
            if (item == null || string.IsNullOrEmpty(item.SlotId))
            {
                return;
            }

            bySlot[item.SlotId] = item;
        }

        public void Clear(string slotId)
        {
            bySlot.Remove(slotId);
        }

        /// <summary>
        /// Works out which worn items are active, and what they add up to.
        ///
        /// ## How activation works, and why it grows from nothing
        ///
        /// Start with **nothing** active. Then repeatedly switch on every worn item whose
        /// requirement is met by the character's own attributes plus the items that are **already
        /// on**, until a pass switches nothing new on.
        ///
        /// The rule answers one question: *could these have been put on one at a time, in some
        /// order?* That is exactly what the player does, and it is why two items that only satisfy
        /// each other both stay off — there is no first one to put on. Sorting that out is the
        /// player's job, which is the same deal Path of Exile offers.
        ///
        /// Growing from nothing rather than shrinking from everything matters. Shrinking would
        /// leave a mutually supporting pair switched on, a state the player could never have
        /// reached by equipping.
        ///
        /// The result does not depend on the order the slots are visited: switching an item on only
        /// ever adds attributes, so anything that becomes eligible stays eligible.
        /// </summary>
        public EquipmentResolution Resolve(ItemRules rules, int[] attributesWithoutEquipment)
        {
            EquipmentResolution resolution = new EquipmentResolution();

            if (rules == null || bySlot.Count == 0)
            {
                return resolution;
            }

            List<Item> pending = new List<Item>(bySlot.Values);

            // Sorted so a run is reproducible from a save, since a dictionary makes no promise
            // about order and the balance snapshot has to come out the same every time.
            pending.Sort((left, right) => string.CompareOrdinal(left.SlotId, right.SlotId));

            while (true)
            {
                int[] attributes = Attributes(attributesWithoutEquipment, resolution.Totals);
                bool switchedSomethingOn = false;

                for (int i = pending.Count - 1; i >= 0; i--)
                {
                    Item item = pending[i];

                    if (!MeetsRequirement(rules, item, attributes))
                    {
                        continue;
                    }

                    resolution.Active.Add(item);
                    ItemContribution.AddTo(resolution.Totals, item, rules.DefenceOf(item));
                    rules.AddClassSlicesOf(item, resolution.Classes);

                    pending.RemoveAt(i);
                    switchedSomethingOn = true;
                }

                if (!switchedSomethingOn)
                {
                    break;
                }
            }

            resolution.Inactive.AddRange(pending);
            return resolution;
        }

        private static bool MeetsRequirement(ItemRules rules, Item item, int[] attributes)
        {
            Dictionary<Attribute, int> demanded = rules.RequirementOf(item);

            foreach (KeyValuePair<Attribute, int> pair in demanded)
            {
                if (attributes[(int)pair.Key] < pair.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private static int[] Attributes(int[] without, EquipmentTotals totals)
        {
            int[] result = new int[4];

            for (int i = 0; i < 4; i++)
            {
                result[i] = (without != null && i < without.Length ? without[i] : 0)
                    + totals.Of((Attribute)i);
            }

            return result;
        }
    }

    /// <summary>
    /// The answer to "what is this hero actually wearing that counts".
    ///
    /// The inactive list is not waste: it is what the interface paints with a red border, and what
    /// tells a player their build stopped meeting its own requirements.
    /// </summary>
    public class EquipmentResolution
    {
        public List<Item> Active = new List<Item>();

        public List<Item> Inactive = new List<Item>();

        public List<ItemClass> Classes = new List<ItemClass>();

        public EquipmentTotals Totals = new EquipmentTotals();
    }
}
