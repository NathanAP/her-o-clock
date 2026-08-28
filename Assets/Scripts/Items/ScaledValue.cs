using System;

namespace HerOClock.Items
{
    /// <summary>
    /// A value that grows with a level: <c>base + perLevel x (level - 1)</c>.
    ///
    /// It is the shape the whole project already uses for armour, for the damage range on a
    /// sheet and for every number an item declares, and it exists for one reason, written in
    /// attributes.md: against a constant that rises with the level, a value standing still is
    /// worth less every level.
    ///
    /// The field is named <c>base</c> in the files because that is the word the specs use, and
    /// <c>base</c> is a C# keyword, hence the escape.
    /// </summary>
    [Serializable]
    public class ScaledValue
    {
        public float @base;
        public float perLevel;

        public float At(int level)
        {
            int steps = level < 1 ? 0 : level - 1;
            return @base + perLevel * steps;
        }
    }

    /// <summary>A pair of <see cref="ScaledValue"/>, which is how every damage range is written.</summary>
    [Serializable]
    public class ScaledRange
    {
        public ScaledValue min;
        public ScaledValue max;

        public float AverageAt(int level)
        {
            return (min.At(level) + max.At(level)) * 0.5f;
        }
    }
}
