namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    static class SHA1DeterministicNameBuilder
    {
        public static string Build(string input)
        {
            //use SHA1 hash to get a 20-byte hash of the string
            using (var provider = new SHA1CryptoServiceProvider())
            {
                var inputBytes = Encoding.Default.GetBytes(input);
                var hashBytes = provider.ComputeHash(inputBytes);

                var hashBuilder = new StringBuilder(string.Join("", hashBytes.Select(x => x.ToString("x2"))));
                foreach (var delimeterIndex in new[] { 5, 11, 17, 23, 29, 35, 41 })
                {
                    hashBuilder.Insert(delimeterIndex, "-");
                }
                return hashBuilder.ToString();
            }
        }
    }
}