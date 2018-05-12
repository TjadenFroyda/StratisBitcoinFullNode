using NBitcoin;
using Stratis.Bitcoin.Features.Wallet;
using System.Collections.Generic;

// TODO: Add/improve comments
// TODO: Check coding style guide
namespace Stratis.Bitcoin.Features.SecureMessaging.Interfaces
{
	public interface ISecureMessaging
	{
		/// <summary>
		/// Gets the shared secret.
		/// </summary>
		/// <returns>The shared secret.</returns>
		string GetSharedSecret();
        
        /// <summary>
        /// Gets the shared private key.
        /// </summary>
        /// <returns>The shared private key.</returns>
		BitcoinSecret GetSharedPrivateKey();

		/// <summary>
        /// Gets the pub key.
        /// </summary>
        /// <returns>The pub key.</returns>
        PubKey GetSharedPubKey();

        /// <summary>
        /// Gets the destination script pub key.
        /// </summary>
        /// <returns>The destination script pub key.</returns>
		Script GetDestScriptPubKey();

		/// <summary>
		/// Encrypt the specified plaintextMessage.
		/// </summary>
		/// <returns>The encrypt.</returns>
		/// <param name="plaintextMessage">Plaintext message.</param>
		string EncryptMessage(string plaintextMessage);

		/// <summary>
		/// Decrypt the specified hexcipher.
		/// </summary>
		/// <returns>The decrypt.</returns>
		/// <param name="hexcipher">Hexcipher.</param>
		string DecryptMessage(string hexcipher);        

		/// <summary>
		/// Prepares the OPR eturn message list.
		/// </summary>
		/// <returns>The OPR eturn message list.</returns>
		/// <param name="hexEncodedEncryptedMessage">Hex encoded encrypted message.</param>
		List<string> prepareOPReturnMessageList(string hexEncodedEncryptedMessage);

        /// <summary>
        /// Transactions the builder.
        /// </summary>
        /// <returns>The builder.</returns>
        /// <param name="sendingWalletName">Sending wallet name.</param>
        /// <param name="sendingAccountName">Sending account name.</param>
        /// <param name="destination">Destination.</param>
        /// <param name="sendingPassword">Sending password.</param>
        /// <param name="messageList">Message list.</param>
		List<TransactionBuildContext> TransactionBuilder(
			string sendingWalletName,
			string sendingAccountName,
			Script destination,
			string sendingPassword,
			List<string> messageList);        
	}
}
