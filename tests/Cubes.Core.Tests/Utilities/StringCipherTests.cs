using Cubes.Core.Utilities;
using Xunit;

namespace Cubes.Core.Tests.Utilities
{
    public class StringCipherTests
    {
        [Fact]
        public void Encrypt_Decrypt_Works()
        {
            string password = "P@ssw0rd";
            string plainText = "This is a text phrase";
            string ciphered = StringCipher.Encrypt(plainText, password);

            string decrypted = StringCipher.Decrypt(ciphered, password);

            Assert.Equal(decrypted, plainText);
        }
    }
}
