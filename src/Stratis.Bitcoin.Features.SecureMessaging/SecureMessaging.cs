using NBitcoin;
using NBitcoin.Crypto;
using Stratis.Bitcoin.Features.SecureMessaging.Interfaces;
using Stratis.Bitcoin.Features.Wallet;
using Stratis.Bitcoin.Utilities;
using System;
using System.Collections.Generic;

// TODO: Add Logging
// TODO: Add/improve comments
// TODO: Check coding style guide
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
		private readonly Key extPubKey;
		private readonly Key sharedSecret;
		private readonly ExtKey sharedPrivateKey;
		private readonly Network network;
		private readonly ISymmetricEncryption symmetricEncryption;
		private string blockexplorerurl = "https://chainz.cryptoid.info/explorer/tx.raw.dws?coin=strat&id=";
		private readonly Wallet.Wallet sharedWallet;

		public SecureMessaging(Key privkey, Key pubkey, Network net)
		{
			Guard.NotNull(privkey, nameof(privkey));
			Guard.NotNull(pubkey, nameof(pubkey));
			Guard.NotNull(net, nameof(net));
			this.myPrivKey = privkey;
			this.extPubKey = pubkey;
			this.network = net;
			this.sharedSecret = this.SetSharedSecret(pubkey, privkey);
			this.symmetricEncryption = new AES(this.sharedSecret.ToHex(this.network));
			this.sharedPrivateKey = this.SetSharedMasterPrivateKey();
			this.sharedWallet = this.generateSharedWallet();
		}
        
		/// <summary>
        /// Get Sharedsecret given the public key of the message sender and the private key of the receiver
        /// </summary>
        /// <returns>The shared secret key</returns>
		private Key SetSharedSecret(Key publickey, Key privatekey)
        {
			PubKey pk = new PubKey(publickey.ToHex(this.network));
			Key key = new Key(privatekey.ToBytes());
			return new Key(Hashes.SHA256(pk.GetSharedPubkey(key).ToBytes()));
        }

        private Wallet.Wallet generateSharedWallet()
		{
			// TODO: Generate shared wallet
		}

        /// <summary>
        /// Gets the shared secret.
        /// </summary>
        /// <returns>The shared secret.</returns>
        public Key GetSharedSecret()
		{
			return this.sharedSecret;
		}
        
        /// <summary>
        /// Gets the shared private key.
        /// </summary>
        /// <returns>The shared private key.</returns>
		public ExtKey GetSharedMasterPrivateKey()
		{
			return this.sharedPrivateKey;
		}
        
        /// <summary>
        /// Sets the shared private key.
        /// </summary>
        /// <returns>The shared private key.</returns>
        private ExtKey SetSharedMasterPrivateKey()
		{
			return new ExtKey(this.sharedSecret.ToBytes());
        }
        
        /// <summary>
        /// Gets the pub key.
        /// </summary>
        /// <returns>The pub key.</returns>
        public ExtPubKey GetSharedMasterPubKey()
		{
			return this.sharedPrivateKey.Neuter();
		}

        /// <summary>
        /// Gets the shared address.
        /// </summary>
        /// <returns>The shared address.</returns>
        public BitcoinPubKeyAddress GetSharedAddress()
		{
			return this.sharedPrivateKey.Neuter().PubKey.GetAddress(this.network);
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
			//TODO: Add handling for messages larger than 40 bytes
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
			//TODO: Add handling for messages larger than 40 bytes
			return this.symmetricEncryption.Decrypt(messageToDecrypt);
		}

        /// <summary>
        /// Builds the OPR eturn message list.
        /// </summary>
        /// <returns>The OPR eturn message list.</returns>
        /// <param name="plaintextMessage">Plaintext message.</param>
        private List<string> buildOPReturnMessageList(string plaintextMessage) 
		{
			string encryptedMessage = this.EncryptMessage(plaintextMessage);
            List<string> messageList = prepareOPReturnMessageList(encryptedMessage);
			return messageList;
		}

        /// <summary>
        /// Builds a list of transactions
        /// </summary>
        /// <returns>The builder.</returns>
        /// <param name="sendingWalletName">Sending wallet name.</param>
        /// <param name="sendingAccountName">Sending account name.</param>
        /// <param name="destination">Destination.</param>
        /// <param name="sendingPassword">Sending password.</param>
        /// <param name="messageList">Message list.</param>
		public List<TransactionBuildContext> TransactionBuilder(
			string sendingWalletName,
            string sendingAccountName,
            Script destination,
            string sendingPassword,
			List<string> messageList)
		{
            List<TransactionBuildContext> contextList = new List<TransactionBuildContext>();
			foreach(string message in messageList) {                       
				TransactionBuildContext context = new TransactionBuildContext(
                new WalletAccountReference(sendingWalletName, sendingAccountName),
                new List<Recipient> { new Recipient { Amount = "0.00000001", ScriptPubKey = destination } },
                sendingPassword, message)
                {
                    TransactionFee = null,
                    MinConfirmations = 0,
                    Shuffle = true
                };
                context.FeeType = 0;
				contextList.Add(context);
			}
			return contextList;
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
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
        }
	}
}
