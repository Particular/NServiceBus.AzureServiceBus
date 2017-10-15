namespace NServiceBus.Transport.AzureServiceBus
{
    /// <summary>
    /// Validation result to provide information about validation performed by the configured <see cref="ISanitizationStrategy"/>.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>Return true for a valid entity path/name length.</summary>
        public bool LengthIsValid { get; private set; } = true;

        /// <summary>Return true for entity path/name containing valid characters only.</summary>
        public bool CharactersAreValid { get; private set; } = true;

        /// <summary>Invalid characters found in entity path/name.</summary>
        public string CharactersError { get; set; }

        /// <summary>Entity path/name length error message.</summary>
        public string LengthError { get; set; }

        /// <summary>Return true for an valid entity path/name passing characters and length checks.</summary>
        public bool IsValid => CharactersAreValid && LengthIsValid;

        /// <summary>Store invalid characters error and set the <seealso cref="CharactersAreValid"/> to false.</summary>
        public void AddErrorForInvalidCharacters(string error)
        {
            CharactersError = error;
            CharactersAreValid = false;
        }

        /// <summary>Store invalid length error and set the <seealso cref="LengthIsValid"/> to false.</summary>
        public void AddErrorForInvalidLength(string error)
        {
            LengthError = error;
            LengthIsValid = false;
        }

        /// <summary>Empty validation result.</summary>
        public static ValidationResult Empty = new ValidationResult();
    }
}