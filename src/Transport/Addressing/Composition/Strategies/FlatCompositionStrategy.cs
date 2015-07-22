namespace NServiceBus.AzureServiceBus.Addressing
{
    public class FlatCompositionStrategy : ICompositionStrategy
    {
        public string GetEntityPath(string entityname, EntityType entityType)
        {
            return entityname;
        }
    }
}