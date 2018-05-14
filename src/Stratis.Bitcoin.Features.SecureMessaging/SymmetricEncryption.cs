using NBitcoin.DataEncoders;
using Stratis.Bitcoin.Features.SecureMessaging.Interfaces;
using Stratis.Bitcoin.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

// TODO: Add Logging
// TODO: Add/improve Comments
// TODO: Check coding style guide
// TODO: Safety checks
namespace Stratis.Bitcoin.Features.SecureMessaging
{
    /// <summary>
    /// Implementaton of AES Symmetric Algorithm based on MSDN example. 
    /// </summary>
    public class AES : ISymmetricEncryption
    {
        private readonly int saltSize = 32;
        private string sharedSecret;
 
        public AES(string sharedSecret)
        {
            Guard.NotNull(sharedSecret, nameof(sharedSecret));
            this.sharedSecret = sharedSecret;
        }

        /// <summary>
        /// Encrypt the specified plainText.
        /// </summary>
        /// <returns>The encrypt.</returns>
        /// <param name="plainText">Plain text.</param>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                throw new ArgumentNullException(nameof(plainText));
            }
            Rfc2898DeriveBytes keyDerivationFunction = new Rfc2898DeriveBytes(this.sharedSecret, this.saltSize);
            byte[] saltBytes = keyDerivationFunction.Salt;
            byte[] keyBytes = keyDerivationFunction.GetBytes(32);
            byte[] ivBytes = keyDerivationFunction.GetBytes(16);
            byte[] encrypted = EncryptStringToBytes_Aes(plainText, ref saltBytes, keyBytes, ivBytes);
            return Encoders.Hex.EncodeData(encrypted);
        }

        /// <summary>
        /// Decrypt the specified cipherText.
        /// </summary>
        /// <returns>The decrypt.</returns>
        /// <param name="cipherText">Cipher text.</param>
        public string Decrypt(string cipherText) 
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                throw new ArgumentNullException(nameof(cipherText));
            }
            byte[] cipherTextBytes = Encoders.Hex.DecodeData(cipherText);
            byte[] saltBytes = cipherTextBytes.Take(this.saltSize).ToArray();
            byte[] unsaltedCipherTextBytes = cipherTextBytes.Skip(this.saltSize).Take(cipherTextBytes.Length - this.saltSize).ToArray();
            Rfc2898DeriveBytes keyDerivationFunction = new Rfc2898DeriveBytes(this.sharedSecret, saltBytes);
            byte[] keyBytes = keyDerivationFunction.GetBytes(32);
            byte[] ivBytes = keyDerivationFunction.GetBytes(16);
            try
            {
                return DecryptStringFromBytes_Aes(unsaltedCipherTextBytes, keyBytes, ivBytes);
            }
            catch (Exception e)
            {
                throw new System.Security.Cryptography.CryptographicException("Invalid decryption key", e);
            }
        }
 
        /// <summary>
        /// Encrypts the string to bytes aes.
        /// </summary>
        /// <returns>The string to bytes aes.</returns>
        /// <param name="plainText">Plain text.</param>
        /// <param name="SaltBytes">Salt bytes.</param>
        /// <param name="Key">Key.</param>
        /// <param name="IV">Iv.</param>
        /// Adapted from https://msdn.microsoft.com/en-us/library/system.security.cryptography.aesmanaged(v=vs.110).aspx#Examples
        private byte[] EncryptStringToBytes_Aes(string plainText, ref byte[] SaltBytes, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
            {
                throw new ArgumentNullException(nameof(plainText));
            }
            if (SaltBytes == null || SaltBytes.Length <= 0)
            {
                throw new ArgumentNullException(nameof(Key));
            }
            if (Key == null || Key.Length <= 0)
            {
                throw new ArgumentNullException(nameof(Key));
            }
            if (IV == null || IV.Length <= 0)
            {
                throw new ArgumentNullException(nameof(IV));
            }
            byte[] cipherBytes;
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.KeySize = 256;                
                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(Key, IV);
                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    cipherBytes = msEncrypt.ToArray();
                }
            }
            Array.Resize(ref SaltBytes, SaltBytes.Length + cipherBytes.Length);
            Array.Copy(cipherBytes, 0, SaltBytes, this.saltSize, cipherBytes.Length);
            return SaltBytes;
        }
 
        /// <summary>
        /// Decrypts the string from bytes aes.
        /// </summary>
        /// <returns>The string from bytes aes.</returns>
        /// <param name="cipherTextBytes">Cipher text bytes.</param>
        /// <param name="Key">Key.</param>
        /// <param name="IV">Iv.</param>
        /// Adapted from https://msdn.microsoft.com/en-us/library/system.security.cryptography.aesmanaged(v=vs.110).aspx#Examples
        private string DecryptStringFromBytes_Aes(byte[] cipherTextBytes, byte[] Key, byte[] IV)
        {
            if (cipherTextBytes == null || cipherTextBytes.Length <= 0)
            {
                throw new ArgumentNullException(nameof(cipherTextBytes));
            }
            if (Key == null || Key.Length <= 0)
            {
                throw new ArgumentNullException(nameof(Key));
            }
            if (IV == null || IV.Length <= 0)
            {
                throw new ArgumentNullException(nameof(IV));
            }
            string plaintext = null;
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(cipherTextBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }
            return plaintext;
        }
    }
}

