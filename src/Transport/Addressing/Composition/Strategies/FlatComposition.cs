namespace NServiceBus
{
    using Transport.AzureServiceBus;

    public class FlatComposition : ICompositionStrategy
    {
        public string GetEntityPath(string entityname, EntityType entityType)
        {
            return entityname;
        }
    }
}