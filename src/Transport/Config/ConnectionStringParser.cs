namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    class ConnectionStringParser
    {
        public string ParseNamespaceFrom(string inputQueue)
        {
            return inputQueue.Contains("@") ? inputQueue.Substring(inputQueue.IndexOf("@", System.StringComparison.Ordinal) + 1) : string.Empty;
        }

        public string ParseQueueNameFrom(string inputQueue)
        {
            return inputQueue.Contains("@") ? inputQueue.Substring(0, inputQueue.IndexOf("@", System.StringComparison.Ordinal)) : inputQueue;
        }

        public int ParseIndexFrom(string id)
        {
            var idArray = id.Split('.');
            int index;
            if (!int.TryParse((idArray[idArray.Length - 1]), out index))
            {
                idArray = id.Split('_');
                index = int.Parse((idArray[idArray.Length - 1]));
            }
            return index;
        }
    }
}