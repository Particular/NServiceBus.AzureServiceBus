﻿namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    class DeterministicGuidBuilder
    {
        public Guid Build(string input)
        {
            //use SHA1 hash to get a 16-byte hash of the string
            using (var provider = new SHA1CryptoServiceProvider())
            {
                var inputBytes = Encoding.Default.GetBytes(input);
                var hashBytes = provider.ComputeHash(inputBytes);
                //generate a guid from the hash:
                Array.Resize(ref hashBytes, 16);
                return new Guid(hashBytes);
            }

        }
    }
}