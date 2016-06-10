namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class ValidationResult
    {
        List<string> validationErrors = new List<string>();

        public ReadOnlyCollection<string> Errors => validationErrors.AsReadOnly();

        public bool LengthIsValid { get; private set; } = true;

        public bool CharactersAreValid { get; private set; } = true;

        public bool IsValid => CharactersAreValid && LengthIsValid;

        public void AddErrorForInvalidCharacter(string error)
        {
            validationErrors.Add(error);
            CharactersAreValid = false;
        }

        public void AddErrorForInvalidLength(string error)
        {
            validationErrors.Add(error);
            LengthIsValid = false;
        }
    }
}