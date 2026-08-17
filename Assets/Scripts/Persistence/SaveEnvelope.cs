namespace HerOClock.Persistence
{
    /// <summary>
    /// The two halves of a save file: the signature and the payload text it covers.
    ///
    /// The file looks like this, and staying readable is a deliberate choice described in save.md:
    ///
    /// <code>
    /// {
    ///     "signature": "3f8a...",
    ///     "payload": { ... }
    /// }
    /// </code>
    ///
    /// The payload is taken out of the file **verbatim**, as the stretch of text between
    /// <c>"payload":</c> and the closing brace of the envelope. Nothing is parsed and rebuilt on
    /// the way, so a field this version has never heard of travels through untouched and is
    /// covered by the signature just like the rest.
    ///
    /// Pure string work on purpose: it needs no file, no engine and no clock, so the whole format
    /// can be checked by handing it text.
    /// </summary>
    public static class SaveEnvelope
    {
        private const string SignatureKey = "\"signature\"";
        private const string PayloadKey = "\"payload\"";

        /// <summary>Wraps a payload text in a signed envelope, ready to be written to disk.</summary>
        public static string Wrap(string payloadText)
        {
            string body = payloadText == null ? "{}" : payloadText.Trim();

            return "{\n    " + SignatureKey + ": \"" + SaveSignature.Of(body) + "\",\n    "
                + PayloadKey + ": " + body + "\n}\n";
        }

        /// <summary>
        /// Splits a file back into its two halves.
        ///
        /// Returns false only when the text is not an envelope at all. A signature that does not
        /// match is **not** handled here: that decision belongs to whoever is loading, because
        /// save.md requires a tampered save to load anyway and be marked.
        /// </summary>
        public static bool TryUnwrap(string fileText, out string payloadText, out string signature, out string problem)
        {
            payloadText = null;
            signature = null;

            if (string.IsNullOrWhiteSpace(fileText))
            {
                problem = "the file is empty.";
                return false;
            }

            if (!TryReadSignature(fileText, out signature))
            {
                problem = "no '" + SignatureKey + "' field was found.";
                return false;
            }

            int keyAt = fileText.IndexOf(PayloadKey, System.StringComparison.Ordinal);

            if (keyAt < 0)
            {
                problem = "no '" + PayloadKey + "' field was found.";
                return false;
            }

            int colon = fileText.IndexOf(':', keyAt + PayloadKey.Length);
            int end = fileText.LastIndexOf('}');

            if (colon < 0 || end <= colon)
            {
                problem = "the '" + PayloadKey + "' field has no value.";
                return false;
            }

            // Everything from the value to the envelope's own closing brace. The payload ends with
            // its own brace, so the last one in the file is the envelope's.
            payloadText = fileText.Substring(colon + 1, end - colon - 1).Trim();

            if (!payloadText.StartsWith("{") || !payloadText.EndsWith("}"))
            {
                problem = "the payload is not a JSON object.";
                payloadText = null;
                return false;
            }

            problem = null;
            return true;
        }

        /// <summary>Reads the hexadecimal signature. It never contains an escape, so no unescaping.</summary>
        private static bool TryReadSignature(string fileText, out string signature)
        {
            signature = null;

            int keyAt = fileText.IndexOf(SignatureKey, System.StringComparison.Ordinal);

            if (keyAt < 0)
            {
                return false;
            }

            int colon = fileText.IndexOf(':', keyAt + SignatureKey.Length);

            if (colon < 0)
            {
                return false;
            }

            int open = fileText.IndexOf('"', colon + 1);

            if (open < 0)
            {
                return false;
            }

            int close = fileText.IndexOf('"', open + 1);

            if (close < 0)
            {
                return false;
            }

            signature = fileText.Substring(open + 1, close - open - 1);
            return true;
        }
    }
}
