// TODO: Add/improve comments
// TODO: Check coding style guide
namespace Stratis.Bitcoin.Features.SecureMessaging.Interfaces
{
    public interface ISymmetricEncryption
    {
        string Encrypt(string plaintext);
        string Decrypt(string hexCipher);
    }
}
