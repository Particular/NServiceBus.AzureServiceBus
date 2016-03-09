namespace NServiceBus.AzureServiceBus.Addressing
{
    public class FlatComposition : ICompositionStrategy
    {
        public string GetEntityPath(string entityname, EntityType entityType)
        {
            return entityname;
        }
    }
}