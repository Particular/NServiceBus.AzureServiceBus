namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Validation
{
    using AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_validation_result
    {
        [Test]
        public void Should_be_valid_by_default()
        {
            var validationResult = new ValidationResult();

            Assert.IsTrue(validationResult.IsValid);
            Assert.IsTrue(validationResult.LengthIsValid);
            Assert.IsTrue(validationResult.CharactersAreValid);
        }

        [Test]
        public void Should_be_invalid_for_invalid_characters_error_message()
        {
            var validationResult = new ValidationResult();
            validationResult.AddErrorForInvalidCharacter("invalid chars");

            Assert.IsFalse(validationResult.IsValid);
            Assert.IsTrue(validationResult.LengthIsValid);
            Assert.IsFalse(validationResult.CharactersAreValid);
        }

        [Test]
        public void Should_be_invalid_for_invalid_length_error_message()
        {
            var validationResult = new ValidationResult();
            validationResult.AddErrorForInvalidLength("invalid length");

            Assert.IsFalse(validationResult.IsValid);
            Assert.IsFalse(validationResult.LengthIsValid);
            Assert.IsTrue(validationResult.CharactersAreValid);
        }

        [Test]
        public void Should_be_invalid_for_invalid_length_and_character_error_messages()
        {
            var validationResult = new ValidationResult();
            validationResult.AddErrorForInvalidCharacter("invalid chars");
            validationResult.AddErrorForInvalidLength("invalid length");

            Assert.IsFalse(validationResult.IsValid);
            Assert.IsFalse(validationResult.LengthIsValid);
            Assert.IsFalse(validationResult.CharactersAreValid);
        }

    }
}