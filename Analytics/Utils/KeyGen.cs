using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Analytics.Utils
{
    public static class KeyGen
    {
        public static string Generate()
        {
            var key = new byte[32];
            using (var generator = RandomNumberGenerator.Create()) // "Using" keyword in this context takes an IDisposable and automatically disposes of it
                generator.GetBytes(key);
            string apiKey = Convert.ToBase64String(key);
            return apiKey;
        }
    }
}
