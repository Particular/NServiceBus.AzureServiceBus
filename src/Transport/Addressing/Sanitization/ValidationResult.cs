namespace NServiceBus.Transport.AzureServiceBus
{
    /// <summary>
    /// Validation result to provide information about validation performed by the configured <see cref="ISanitizationStrategy"/>.
    /// </summary>
    public class ValidationResult
    {
        /// <summary></summary>
        public bool LengthIsValid { get; private set; } = true;

        /// <summary></summary>
        public bool CharactersAreValid { get; private set; } = true;

        /// <summary></summary>
        public string CharactersError { get; set; }

        /// <summary></summary>
        public string LengthError { get; set; }

        /// <summary></summary>
        public bool IsValid => CharactersAreValid && LengthIsValid;

        /// <summary></summary>
        public void AddErrorForInvalidCharacters(string error)
        {
            CharactersError = error;
            CharactersAreValid = false;
        }

        /// <summary></summary>
        public void AddErrorForInvalidLenth(string error)
        {
            LengthError = error;
            LengthIsValid = false;
        }

        /// <summary></summary>
        public static ValidationResult Empty = new ValidationResult();
    }
}