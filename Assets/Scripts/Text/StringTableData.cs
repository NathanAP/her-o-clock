using System;

namespace HerOClock.Text
{
    /// <summary>
    /// A strings file, mirroring the shape of its JSON.
    ///
    /// Field names are lowercase because JsonUtility matches them against the JSON keys character
    /// for character. This is a wire format, not a normal C# type, exactly like StageData.
    ///
    /// The entries are an array of pairs rather than an object, because JsonUtility cannot read a
    /// dictionary. The cost is a little noise in the file; the benefit is not having to bring in
    /// another JSON library for one feature.
    /// </summary>
    [Serializable]
    public class StringTableData
    {
        /// <summary>Which language this file holds, as a plain tag such as "en".</summary>
        public string language;

        public StringEntry[] entries;
    }

    [Serializable]
    public struct StringEntry
    {
        public string key;
        public string value;
    }
}
