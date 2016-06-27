namespace NServiceBus.Transport.AzureServiceBus
{
    public interface ICompositionStrategy
    {
        string GetEntityPath(string entityname, EntityType entityType);
    }
}
