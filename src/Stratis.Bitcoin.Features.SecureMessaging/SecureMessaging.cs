using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.DataEncoders;
using Stratis.Bitcoin.Features.SecureMessaging.Interfaces;
using Stratis.Bitcoin.Utilities;
using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Text;

// TODO: Add Logging
// TODO: Add/improve comments
// TODO: Safety checks
namespace Stratis.Bitcoin.Features.SecureMessaging
{
    /// <summary>
    /// Provides the ability to send secure messages through OP_RETURN messages
    /// </summary>
    /// 
    public class SecureMessaging : ISecureMessaging
    {
        private readonly Key myPrivKey;
        private readonly PubKey extPubKey;
        private readonly Key sharedSecretMasterPrivateKey;
        private readonly Network network;
        private readonly ISymmetricEncryption symmetricEncryption;
        
        public SecureMessaging(Key privkey, PubKey pubkey, Network net)
        {
            Guard.NotNull(privkey, nameof(privkey));
            Guard.NotNull(pubkey, nameof(pubkey));
            this.myPrivKey = privkey;
            this.extPubKey = pubkey;
            this.network = net;
            this.sharedSecretMasterPrivateKey = this.SetSharedSecretMasterPrivateKey();
            this.symmetricEncryption = new AES(this.sharedSecretMasterPrivateKey.ToHex(this.network));
        }

        /// <summary>
        /// Sets the shared secret master private key.
        /// </summary>
        /// <returns>The shared secret master private key.</returns>
        private Key SetSharedSecretMasterPrivateKey()
        {
            PubKey pubKey = new PubKey(this.extPubKey.ToHex(this.network));
            Key key = new Key(Encoders.Hex.DecodeData(this.myPrivKey.ToHex(this.network)));
            return new Key(Hashes.SHA256(pubKey.GetSharedPubkey(key).ToBytes()));
        }   

        /// <summary>
        /// Gets the shared private key.
        /// </summary>
        /// <returns>The shared private key.</returns>
        public Key GetSharedSecretMasterPrivateKey()
        {
            return this.sharedSecretMasterPrivateKey;
        }

        public BitcoinPubKeyAddress GetSharedAddress()
        {
            return this.sharedSecretMasterPrivateKey.PubKey.GetAddress(this.network);
        }
 
        /// <summary>
        /// Gets the destination script pub key.
        /// </summary>
        /// <returns>The destination script pub key.</returns>
        public Script GetDestScriptPubKey() 
        {
            return this.GetSharedAddress().ScriptPubKey;
        }

        /// <summary>
        /// Encrypt message using ECDH
        /// </summary>
        /// Implementation modified from CryptoLibrary at https://stephenhaunts.com/2013/03/04/cryptography-in-net-advanced-encryption-standard-aes/
        /// and https://msdn.microsoft.com/en-us/library/system.security.cryptography.aesmanaged(v=vs.110).aspx
        /// <param name="messageToEncrypt">Plaintext string representation of message to encrypt</param>
        /// <returns>Hex encoded, AES encrypted message</returns>
        public string EncryptMessage(string messageToEncrypt)
        {
            if (string.IsNullOrEmpty(messageToEncrypt))
            {
                throw new ArgumentNullException(nameof(messageToEncrypt));
            }
            return this.symmetricEncryption.Encrypt(messageToEncrypt);
        }

        /// <summary>
        /// Decrypt message using ECDH
        /// </summary>
        /// Implementation modified from CryptoLibrary at https://stephenhaunts.com/2013/03/04/cryptography-in-net-advanced-encryption-standard-aes/
        /// and https://msdn.microsoft.com/en-us/library/system.security.cryptography.aesmanaged(v=vs.110).aspx
        /// <param name="messageToDecrypt">Hex encoded, AES encrypted message</param>
        public string DecryptMessage(string messageToDecrypt)
        {
            if (string.IsNullOrEmpty(messageToDecrypt))
            {
                throw new ArgumentNullException(nameof(messageToDecrypt));
            }
            return this.symmetricEncryption.Decrypt(messageToDecrypt);
        }

        /// <summary>
        /// Builds the OPReturn message list.
        /// </summary>
        /// <returns>The OPReturn message list.</returns>
        /// <param name="plaintextMessage">Plaintext message.</param>
        private List<string> buildOPReturnMessageList(string plaintextMessage) 
        {
            string compressedString;
            using (var outputStream = new MemoryStream())
            {
                using (GZipStream gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    gZipStream.Write(Encoding.UTF8.GetBytes(plaintextMessage), 0, Encoding.UTF8.GetBytes(plaintextMessage).Length);
                }
                byte[] outputBytes = outputStream.ToArray();
                compressedString = Convert.ToBase64String(outputBytes);
            }
            string encryptedMessage = this.EncryptMessage(compressedString);
            List<string> messageList = prepareOPReturnMessageList(encryptedMessage);
            return messageList;
        }

        /// <summary>
        /// Prepares the OP_Return message list.
        /// </summary>
        /// <returns>The OP_Return message list.</returns>
        /// <param name="hexEncodedEncryptedMessage">Hex encoded encrypted message.</param>
        public List<string> prepareOPReturnMessageList(string hexEncodedEncryptedMessage)
        {
            // Stratis examples have shown 40 as max limit, but is 83 in NBitcoin/StandardScriptTemplate.cs
            // Sticking with 40 for now
            List<string> chunks = (List<string>)ChunksUpto(hexEncodedEncryptedMessage, 40);
            return chunks;
        }

        /// <summary>
        /// Divides a string into an array of strings of a certain size.
        /// </summary>
        /// <returns>Chunks of bytes up to the parameter size</returns>
        /// <param name="str">String.</param>
        /// <param name="maxChunkSize">Max chunk size.</param>
        private static IEnumerable<string> ChunksUpto(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
            {
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
            }
        }
    }
}
