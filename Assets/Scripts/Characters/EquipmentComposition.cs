using System.Collections.Generic;

namespace HerOClock.Characters
{
    /// <summary>
    /// How much of each of the three character classes a set of worn equipment adds up to, from
    /// "A classe do personagem" in attributes.md.
    ///
    /// ## What problem this solves
    ///
    /// Four formulas in attributes.md need **one** number that depends on the class being worn:
    /// the evasion constant, the cooldown constant, and the attack and movement speed gained per
    /// point of AGI. A character wears up to eight items, each with its own class out of six.
    /// Something has to turn eight into one, and this is it.
    ///
    /// Each worn item hands in one slice. A hybrid hands in half a slice to each of its two sides,
    /// which is what lets attributes.md keep knowing about three classes while an item can be one
    /// of six. The value of a formula is then the average of its three values, weighted by the
    /// slices.
    ///
    /// ## Why the constant is mixed and never the result
    ///
    /// Mixing the results would stop a character's evasion from being
    /// `100 x AGI / (AGI + constant)` for any single constant, and everything the spec hangs on
    /// that shape — including the evasion points equipment will grant — would have nowhere to go.
    /// Mixing the constant keeps one formula with one number in it.
    ///
    /// The side effect is that mixing classes pays slightly less than the average of the results,
    /// because the curve is convex. That is deliberate: specialising pays.
    ///
    /// Instances are immutable, so nothing can change one from under a character that is reading
    /// it mid stage.
    /// </summary>
    public class EquipmentComposition
    {
        private readonly float light;
        private readonly float special;
        private readonly float heavy;

        private EquipmentComposition(float light, float special, float heavy)
        {
            this.light = light;
            this.special = special;
            this.heavy = heavy;
        }

        /// <summary>
        /// The composition of a character wearing nothing, which is the class written on its
        /// sheet.
        ///
        /// This is what every minion, villain and NPC uses, since they never wear anything, and
        /// what a hero uses before it has items.
        /// </summary>
        public static EquipmentComposition Of(EquipmentClass sheetClass)
        {
            switch (sheetClass)
            {
                case EquipmentClass.Light: return new EquipmentComposition(1f, 0f, 0f);
                case EquipmentClass.Special: return new EquipmentComposition(0f, 1f, 0f);
                default: return new EquipmentComposition(0f, 0f, 1f);
            }
        }

        /// <summary>
        /// The composition of a character wearing the given classes, one entry per **active**
        /// piece of equipment.
        ///
        /// An empty slot is simply absent from the list, and so is a piece that is inactive for
        /// failing its requirement: attributes.md counts neither, because a character wearing two
        /// items is the mix of those two and not a mix of two items and six holes.
        ///
        /// With nothing in the list at all, the sheet's own class is the answer.
        /// </summary>
        public static EquipmentComposition Of(IReadOnlyList<ItemClass> worn, EquipmentClass sheetClass)
        {
            if (worn == null || worn.Count == 0)
            {
                return Of(sheetClass);
            }

            float light = 0f;
            float special = 0f;
            float heavy = 0f;

            for (int i = 0; i < worn.Count; i++)
            {
                switch (worn[i])
                {
                    case ItemClass.Light: light += 1f; break;
                    case ItemClass.Special: special += 1f; break;
                    case ItemClass.Heavy: heavy += 1f; break;
                    case ItemClass.Medium: light += 0.5f; heavy += 0.5f; break;
                    case ItemClass.LightSpecial: light += 0.5f; special += 0.5f; break;
                    default: special += 0.5f; heavy += 0.5f; break;
                }
            }

            return new EquipmentComposition(light, special, heavy);
        }

        /// <summary>Total slices handed in, which is the number of active pieces worn.</summary>
        public float Total
        {
            get { return light + special + heavy; }
        }

        /// <summary>How many slices went to one of the three classes.</summary>
        public float ShareOf(EquipmentClass characterClass)
        {
            switch (characterClass)
            {
                case EquipmentClass.Light: return light;
                case EquipmentClass.Special: return special;
                default: return heavy;
            }
        }

        /// <summary>
        /// The value of a formula for this character: the three class values averaged by the
        /// slices.
        ///
        /// The arithmetic runs in double for the same reason the damage calculation does. These
        /// results feed the diminishing returns curve, and a value landing either side of a
        /// rounding boundary would make the editor and a build disagree about a fight.
        /// </summary>
        public float Blend(float lightValue, float specialValue, float heavyValue)
        {
            double total = (double)light + special + heavy;

            if (total <= 0d)
            {
                // Unreachable through either factory: both always hand out at least one whole
                // slice. Kept so a future caller building one by hand fails loudly in a test
                // rather than dividing by zero in the middle of a battle.
                return lightValue;
            }

            double mixed = light * (double)lightValue
                + special * (double)specialValue
                + heavy * (double)heavyValue;

            return (float)(mixed / total);
        }
    }
}
