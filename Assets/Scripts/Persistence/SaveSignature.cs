using System;
using System.Security.Cryptography;
using System.Text;

namespace HerOClock.Persistence
{
    /// <summary>
    /// Signs and verifies the text of a save.
    ///
    /// It works on the **text**, exactly as it sits in the file, and never on the result of
    /// serialising an object again. That is what keeps an old file verifiable: a save written by a
    /// future version may hold fields this one does not know about, and reserialising would drop
    /// them and break the signature forever.
    /// </summary>
    public static class SaveSignature
    {
        /// <summary>Length of a signature in characters, since SHA256 is 32 bytes in hexadecimal.</summary>
        public const int Length = 64;

        public static string Of(string payload)
        {
            byte[] hash;

            using (HMACSHA256 hmac = new HMACSHA256(SaveKey.Bytes()))
            {
                hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));
            }

            StringBuilder hex = new StringBuilder(hash.Length * 2);

            for (int i = 0; i < hash.Length; i++)
            {
                hex.Append(hash[i].ToString("x2"));
            }

            return hex.ToString();
        }

        public static bool Matches(string payload, string signature)
        {
            if (string.IsNullOrEmpty(signature))
            {
                return false;
            }

            return string.Equals(Of(payload), signature, StringComparison.OrdinalIgnoreCase);
        }
    }
}
