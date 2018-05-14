using Moq;
using NBitcoin;
using Stratis.Bitcoin.Configuration;
using Stratis.Bitcoin.Features.Api;
using Stratis.Bitcoin.Features.Wallet.Interfaces;
using Stratis.Bitcoin.Features.Wallet;
using Stratis.Bitcoin.Tests.Common.Logging;
using Stratis.Bitcoin.Utilities;
using Xunit;
using System;
using Stratis.Bitcoin.Features.SecureMessaging.Controllers;
using Stratis.Bitcoin.Features.SecureMessaging.Models;

// TODO: Add Logging
// TODO: Add/improve Comments
// TODO: Check coding style guide
// TODO: Safety checks
// TODO: Add tests for valid and invalid model requests for each API endpoint. 
namespace Stratis.Bitcoin.Features.SecureMessaging.Tests
{
    public class SecureMessagingControllerTests : LogsTestBase
    {
        private TestPerson Alice;
        private TestPerson Bob;
        private Network network;
        private Wallet.Wallet AliceWallet;
        private Wallet.Wallet BobWallet;

        public SecureMessagingControllerTests()
        {
            // TODO: Set up two wallets for Alice and Bob. Will generate a secret key for each 
            // (maybe same ones as SecureMessageTests) for reproducibility. 
            this.network = Network.Main;
            this.Alice = new TestPerson(
                pub: "04bc888f2739cc9c9a5d595bf4da54a1fb6854c269f8e8ab0d3e94f71ba37d75a84b196ce0801eb13b94a181c4c34ed15f2ec1c5fd1899d8953a546b8c164d18c6",
                priv: "6215a59058ba689889a3aa0e32d0f686ddb7a3ffae003376f4d0744eb7b61b19",
                net: this.network
            );
            this.Bob = new TestPerson(
                pub: "049788818055d962297edbeb50431b8c44f545714bf0ce5471159cecd86c78efe2742778ad242ac325fb48217351f70782db8bf50f633b59f2bdbdcd1a08f1f7aa",
                priv: "d57cc0624f024149c300c0897ac917c58142be3d6346797651c85ca8dbae05f8",
                net: this.network
            );

        }
 
        /// <summary>
        /// Overloaded recover wallet method in WalletManager.cs. This checks to make sure it works as intended.
        /// </summary>
        [Fact]
        public void TestWalletRecoverFromSeed()
        {
            // Set up
            DataFolder dataFolder = CreateDataFolder(this);
            var chain = new ConcurrentChain(this.network);
            var nonce = RandomUtils.GetUInt32();
            var block = new Block();
            block.AddTransaction(new Transaction());
            block.UpdateMerkleRoot();
            block.Header.HashPrevBlock = chain.Genesis.HashBlock;
            block.Header.Nonce = nonce;
            chain.SetTip(block.Header);
            var walletManager = new WalletManager(
                this.LoggerFactory.Object, 
                this.network, 
                chain, 
                NodeSettings.Default(), 
                new Mock<WalletSettings>().Object,
                dataFolder, 
                new Mock<IWalletFeePolicy>().Object, 
                new Mock<IAsyncLoopFactory>().Object, 
                new NodeLifetime(), 
                DateTimeProvider.Default
            );
            string passphrase = "This is an awesome passphrase";
 
            // Act
            Wallet.Wallet AliceWallet = walletManager.RecoverWallet(
                new ExtKey(this.Alice.GetPrivateKeyHex()),
                "AliceWallet",
                DateTime.Now,
                passphrase
            );
 
            ExtKey expectedExtKey = new ExtKey(this.Alice.GetPrivateKeyHex());
            string expectedEncryptedSeed = expectedExtKey.PrivateKey.GetEncryptedBitcoinSecret(passphrase, this.network).ToWif();
 
            // Assert
            Assert.Equal("AliceWallet", AliceWallet.Name);
            Assert.Equal(this.network, AliceWallet.Network);
            Assert.Equal(expectedEncryptedSeed, AliceWallet.EncryptedSeed);           
        }

        // Test retrieving a private key from Alice's wallet without using  
        // DumpPrivKey is a RPC call. Seeking to avoid RPC based calls as not all nodes 
        // will be running RPC and API is more modern approach.
        [Fact]
        public void TestWalletGenerateSecureMessagingPrivateKey()
        {
            // Set up
            DataFolder dataFolder = CreateDataFolder(this);
            var chain = new ConcurrentChain(this.network);
            var nonce = RandomUtils.GetUInt32();
            var block = new Block();
            block.AddTransaction(new Transaction());
            block.UpdateMerkleRoot();
            block.Header.HashPrevBlock = chain.Genesis.HashBlock;
            block.Header.Nonce = nonce;
            chain.SetTip(block.Header);
            WalletManager walletManager = new WalletManager(
                this.LoggerFactory.Object, this.network, 
                chain, 
                NodeSettings.Default(), 
                new Mock<WalletSettings>().Object,
                dataFolder, 
                new Mock<IWalletFeePolicy>().Object, 
                new Mock<IAsyncLoopFactory>().Object, 
                new NodeLifetime(), 
                DateTimeProvider.Default
            );
            string passphrase = "This is an awesome passphrase";
            Wallet.Wallet AliceWallet = walletManager.RecoverWallet(
                new ExtKey(this.Alice.GetPrivateKeyHex()),
                "AliceWallet",
                DateTime.Now,
                passphrase
            );
            SecureMessagingController secureMessagingController =  new SecureMessagingController(
                new Mock<FullNode>().Object,
                this.LoggerFactory.Object, 
                walletManager, 
                this.network,
                new Mock<IWalletTransactionHandler>().Object
            );
            SecureMessageKeyRequest myKeyRequest = new SecureMessageKeyRequest
            {
                WalletName = "AliceWallet",
                Passphrase = passphrase
            };
            // Act
            Key dumpkey = secureMessagingController.GetPrivateMessagingKey(myKeyRequest);

            // Secure message private key derivation algorithm. 
            ExtKey masterKey = new ExtKey(this.Alice.GetPrivateKeyHex());
            string expencseed = masterKey.PrivateKey.GetEncryptedBitcoinSecret(passphrase, this.network).ToWif();
            Key expdecseed = Key.Parse(expencseed, passphrase, this.network);
            Key expectedKey = new Key(expdecseed.ToBytes());

            // Assert
            Assert.Equal(expectedKey, dumpkey);
        }

        [Fact]
        public void TestSharedSecretGeneration()
        {
            // TODO: Test shared address generation using the Bob and Alice's wallet keys.
            // The last unit test generated a set of private keys derived from the wallet seed. 
            // This test will see if the shared secret can be successfully generated from these keys.
            // Should work based on testing with SECP256K1 keys but need to make sure that nothing funny 
            // happens when using the HD wallet keys. 
        }

        /// <summary>
        /// Tests the restful APIM essage encryption service.
        /// </summary>
        [Fact]
        public void TestRestfulAPIMessageEncryptionService()
        {
            // TODO: Test Restful API method for message encryption given a provided public key and message. 
            // Nothing gets written to the blockchain with this method, but the encrypted message could be 
            // utilized by third party applications built on top of the node. 
        }

        /// <summary>
        /// Tests the restful APIM essage decryption service.
        /// </summary>
        [Fact]
        public void TestRestfulAPIMessageDecryptionService()
        {
            // TODO: Test Restful API method to message decryption given a provided public key and cipher. 
            // Message could be obtained from any method and is not limited to OP_RETURN. As above, also 
            // potentially useful for a third party application built on top of the node. 
        }

        /// <summary>
        /// Tests the transaction builder.
        /// </summary>
        [Fact]
        public void TestTransactionBuilder()
        {
            // TODO: Set up a transaction and broadcast it. 
            // We want to make sure that the result is a standard message that will be accepted by validators. 
            // In this test we will write a transaction to the testnet blockchain to confirm that sending OP_RETURN
            // messages through the API works. 
            // In summary, message consists of an op_return message sent from either Alice or Bob to a receiving address
            // in a wallet generated by the shared secret. This message will be a short message.             
        }

        [Fact]
        public void TestLongMessageTransactionBuilder()
        {
            // TODO: Set up a long transaction and broadcast it. 
            // Same as above, but with a longer message sent by batching.            
        }
                      
        [Fact]
        public void TestAddSharedWallet()
        {
            // TODO: Test importing shared secret key into wallet. 
            // Will use existing HD wallet functionality to import the shared private key to create a new shared wallet 
            // between Alice and Bob. The wallet should only be used for messaging, with the receiving addresses 
            // potentially being used for new conversations. 
        }

        [Fact]
        public void TestGetSharedAddressHistory()
        {
            // TODO: Test retrieving the transaction history of the shared wallet
            // Method not implemented yet
            // Could use use chainz api to gather transactions from shared address. 
            // Alternatively, could use the wallet API to gather transactions by receiving address. 
            // Each receiving address could be a separate conversation. 
        }

        [Fact]
        public void TestConstructConversationFromSharedAddress()
        {
            // TODO: Uses the shared address to 1) gather op_return messages, 2) construct a 
            // Method not implemented yet
            // Builds on the last test to construct conversations from the gathered op return messages. 
            // If this works, it would be an interesting implemenation of secure messaging on the blockchain. 
            // Granted, there are cheaper secure messaging methods
        }      
    }
}
