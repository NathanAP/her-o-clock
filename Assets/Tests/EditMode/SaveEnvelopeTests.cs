using HerOClock.Persistence;
using NUnit.Framework;

namespace HerOClock.Tests
{
    /// <summary>
    /// The signed envelope of a save file, against the rules in save.md.
    ///
    /// This is pure text work, which is why it can be checked by handing it strings. The one
    /// property that matters most is the last test here: the signature covers the text as written,
    /// so a file holding fields this version has never heard of still verifies. Without that, no
    /// save would survive the version that adds a field.
    /// </summary>
    public class SaveEnvelopeTests
    {
        private const string Payload = "{\n    \"version\": 1,\n    \"money\": 1500\n}";

        [Test]
        public void APayloadComesBackExactlyAsItWentIn()
        {
            string file = SaveEnvelope.Wrap(Payload);

            string payload;
            string signature;
            string problem;

            Assert.IsTrue(SaveEnvelope.TryUnwrap(file, out payload, out signature, out problem), problem);
            Assert.AreEqual(Payload.Trim(), payload);
        }

        [Test]
        public void AFreshlyWrittenFileMatchesItsOwnSignature()
        {
            string file = SaveEnvelope.Wrap(Payload);

            string payload;
            string signature;
            string problem;

            SaveEnvelope.TryUnwrap(file, out payload, out signature, out problem);

            Assert.IsTrue(SaveSignature.Matches(payload, signature),
                "A file the game just wrote does not verify, so every save would be marked as edited.");
        }

        [Test]
        public void TheSignatureIsHexadecimalOfTheExpectedLength()
        {
            string signature = SaveSignature.Of(Payload);

            Assert.AreEqual(SaveSignature.Length, signature.Length);
            Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(signature, "^[0-9a-f]+$"),
                "Expected lowercase hexadecimal, got '" + signature + "'.");
        }

        // --- What tampering looks like ---

        /// <summary>
        /// The case the whole mechanism exists for: somebody opens the file and changes a number.
        /// </summary>
        [Test]
        public void EditingANumberBreaksTheSignature()
        {
            string file = SaveEnvelope.Wrap(Payload).Replace("1500", "999999");

            string payload;
            string signature;
            string problem;

            Assert.IsTrue(SaveEnvelope.TryUnwrap(file, out payload, out signature, out problem), problem);
            Assert.IsFalse(SaveSignature.Matches(payload, signature),
                "An edited amount of money verified, so the signature is checking nothing.");
        }

        [Test]
        public void ADifferentPayloadGivesADifferentSignature()
        {
            Assert.AreNotEqual(SaveSignature.Of(Payload), SaveSignature.Of(Payload + " "));
        }

        [Test]
        public void AnEmptySignatureNeverMatches()
        {
            Assert.IsFalse(SaveSignature.Matches(Payload, ""));
            Assert.IsFalse(SaveSignature.Matches(Payload, null));
        }

        // --- What is not an envelope at all ---

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("not json")]
        [TestCase("{\"payload\": {} }")]
        [TestCase("{\"signature\": \"abc\"}")]
        [TestCase("{\"signature\": \"abc\", \"payload\": 7}")]
        public void TextThatIsNotAnEnvelopeIsRefused(string text)
        {
            string payload;
            string signature;
            string problem;

            Assert.IsFalse(SaveEnvelope.TryUnwrap(text, out payload, out signature, out problem));
            Assert.IsNotNull(problem, "A refusal has to say why, or a broken save is undiagnosable.");
        }

        // --- Surviving a future version ---

        /// <summary>
        /// A save written by a later version holds fields this one knows nothing about. The
        /// signature covers the text, so it still verifies, and that is what makes it possible to
        /// add a field to the format without invalidating every save that already exists.
        ///
        /// Signing the result of serialising the object again would drop the unknown fields and
        /// break this for good.
        /// </summary>
        [Test]
        public void APayloadWithUnknownFieldsStillVerifies()
        {
            string fromTheFuture = "{\n    \"version\": 1,\n    \"money\": 1500,\n"
                + "    \"somethingAddedLater\": { \"deep\": [1, 2, 3] }\n}";

            string file = SaveEnvelope.Wrap(fromTheFuture);

            string payload;
            string signature;
            string problem;

            Assert.IsTrue(SaveEnvelope.TryUnwrap(file, out payload, out signature, out problem), problem);
            Assert.AreEqual(fromTheFuture, payload, "The unknown field did not survive the round trip.");
            Assert.IsTrue(SaveSignature.Matches(payload, signature));
        }
    }
}
