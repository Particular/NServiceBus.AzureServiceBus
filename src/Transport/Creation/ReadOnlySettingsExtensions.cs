namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;
    using Settings;

    static class ReadOnlySettingsExtensions
    {
        /// <summary>Apply conditional setting if condition exists</summary>
        internal static T GetConditional<T>(this ReadOnlySettings settings, string name, string key)
        {
            var condition = settings.Get<Func<string, bool>>(key + "Condition");
            return GetConditional<T>(settings, () => condition(name), key);
        }

        static T GetConditional<T>(this ReadOnlySettings settings, Func<bool> condition, string key)
        {
            if (condition())
            {
                return settings.GetOrDefault<T>(key);
            }

            return settings.GetDefault<T>(key);
        }

        static T GetDefault<T>(this ReadOnlySettings settings, string key)
        {
            object result;
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var defaults = (ConcurrentDictionary<string, object>)typeof(SettingsHolder).GetField("Defaults", bindingFlags).GetValue(settings);
            if (defaults.TryGetValue(key, out result))
            {
                return (T)result;
            }

            return default(T);
        }
    }
}