using System;
using NServiceBus;
using NServiceBus.Serialization;
using NServiceBus.Settings;
using NServiceBus.Transport.AzureServiceBus;

public static class SettingsHolderFactory
{
    public static SettingsHolder BuildWithSerializer()
    {
        var settings = new SettingsHolder();
        settings.Set(WellKnownConfigurationKeys.Core.MainSerializerSettingsKey, Tuple.Create<SerializationDefinition, SettingsHolder>(new XmlSerializer(), settings));
        return settings;
    }
}