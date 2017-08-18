namespace NServiceBus.Transport.AzureServiceBus
{
    public class ValidationResult
    {
        public bool LengthIsValid { get; private set; } = true;

        public bool CharactersAreValid { get; private set; } = true;

        public string CharactersError { get; set; }

        public string LengthError { get; set; }

        public bool IsValid => CharactersAreValid && LengthIsValid;

        public void AddErrorForInvalidCharacters(string error)
        {
            CharactersError = error;
            CharactersAreValid = false;
        }

        public void AddErrorForInvalidLenth(string error)
        {
            LengthError = error;
            LengthIsValid = false;
        }

        public static ValidationResult Empty = new ValidationResult();
    }
}