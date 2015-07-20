namespace NServiceBus.AzureServiceBus.Addressing
{
    public interface ICompositionStrategy
    {
        string GetEntityPath(string entityname);
    }
}
