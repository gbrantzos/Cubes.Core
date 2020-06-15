using System;
using System.Security.Cryptography;

namespace Cubes.Core.Security
{
    public static class PasswordGenerator
    {
        /// <summary>
        /// Generate random password. The length of the buffer should be multiple of 3 and it is used by the
        /// <see cref="RNGCryptoServiceProvider"/>.
        /// <para>
        /// To calculate the output length use the formula (length / 3) * 4.
        /// For example length 27 produces a string of 16 characters: (27 / 3) * 4.
        /// </para>
        /// </summary>
        /// <param name="length">The length of the <see cref="RNGCryptoServiceProvider"/> buffer</param>
        /// <returns></returns>
        public static string GeneratePassword(int length = 27)
        {
            // https://stackoverflow.com/a/24711536/3410871
            // Buffer length should be multiple of 3, i.e. (27 / 3) * 4 = 32

            using var cryptRNG = new RNGCryptoServiceProvider();
            byte[] tokenBuffer = new byte[length];
            cryptRNG.GetBytes(tokenBuffer);

            return Convert.ToBase64String(tokenBuffer);
        }
    }
}
