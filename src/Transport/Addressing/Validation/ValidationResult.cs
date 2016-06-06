namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class ValidationResult
    {
        List<string> validationErrors = new List<string>();

        public void AddError(string error)
        {
            validationErrors.Add(error);
        }

        public bool IsValid => !validationErrors.Any();

        public ReadOnlyCollection<string> Errors => validationErrors.AsReadOnly();
    }
}