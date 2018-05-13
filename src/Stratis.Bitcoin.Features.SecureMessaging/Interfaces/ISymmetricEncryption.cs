// TODO: Add/improve comments
// TODO: Check coding style guide
namespace Stratis.Bitcoin.Features.SecureMessaging.Interfaces
{
    public interface ISymmetricEncryption
    {
        /// <summary>
        /// Encrypt the specified plaintext.
        /// </summary>
        /// <returns>The encrypt.</returns>
        /// <param name="plaintext">Plaintext.</param>
        string Encrypt(string plaintext);

        /// <summary>
        /// Decrypt the specified hexCipher.
        /// </summary>
        /// <returns>The decrypt.</returns>
        /// <param name="hexCipher">Hex cipher.</param>
        string Decrypt(string hexCipher);        
    }
}
