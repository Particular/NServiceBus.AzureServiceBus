namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    static class MD5DeterministicNameBuilder
    {
        public static string Build(string input)
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