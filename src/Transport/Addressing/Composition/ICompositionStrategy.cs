namespace NServiceBus.AzureServiceBus.Addressing
{
    public interface ICompositionStrategy
    {
        string GetFullPath(string entityname);
    }
}
