namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using Serialization;
    using Settings;

    static class Guard
    {
        public static void AgainstNull(string argumentName, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        public static void AgainstNegativeAndZero(string argumentName, int value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void AgainstNegative(string argumentName, int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void AgainstNegative(string argumentName, TimeSpan value)
        {
            if (value < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(argumentName);
            }
        }

        public static void AgainstUnsetSerializerSetting(SettingsHolder settings)
        {
            if (!settings.TryGet<Tuple<SerializationDefinition, SettingsHolder>>(WellKnownConfigurationKeys.Core.MainSerializerSettingsKey, out var _))
            {
                throw new Exception("Use 'endpointConfiguration.UseSerialization<T>();' to select a serializer. If you are upgrading, install the `NServiceBus.Newtonsoft.Json` NuGet package and consult the upgrade guide for further information.");
            }
        }
    }
}
