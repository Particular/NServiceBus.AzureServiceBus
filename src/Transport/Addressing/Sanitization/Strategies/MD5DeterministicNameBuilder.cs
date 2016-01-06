namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    class MD5DeterministicNameBuilder
    {
        public string Build(string input)
        {
            //use MD5 hash to get a 16-byte hash of the string
            using (var provider = new MD5CryptoServiceProvider())
            {
                var inputBytes = Encoding.Default.GetBytes(input);
                var hashBytes = provider.ComputeHash(inputBytes);

                return new Guid(hashBytes).ToString();
            }
        }
    }
}