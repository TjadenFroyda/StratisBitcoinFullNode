using NBitcoin; 
using Stratis.Bitcoin.Features.SecureMessaging.Interfaces;
using Stratis.Bitcoin.Tests.Common.Logging;
using System;
using Xunit;

namespace Stratis.Bitcoin.Features.SecureMessaging.Tests
{
	/// <summary>
    /// Test person class to help with Unit Testing 
    /// </summary>
	public class TestPerson
    {
        public string privKey { get; private set; }
        public string pubKey { get; private set; }

        public TestPerson(string pub, string priv)
        {
            this.pubKey = pub;
            this.privKey = priv;
        }
    }

    /// <summary>
    /// Secure messaging tests.
    /// </summary>
	public class SecureMessagingTests : LogsTestBase
	{
		private TestPerson Alice;
		private TestPerson Bob;
		private TestPerson Chad;
		private ISecureMessaging BobSM;
		private ISecureMessaging AliceSM;
        
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Stratis.Bitcoin.Features.SecureMessaging.Tests.SecureMessagingTests"/> class.
        /// </summary>
		public SecureMessagingTests()
		{
			// KeyPairs generated from https://kjur.github.io/jsrsasign/sample/sample-ecdsa.html using SECP256K1
            this.Alice = new TestPerson(
                pub: "04bc888f2739cc9c9a5d595bf4da54a1fb6854c269f8e8ab0d3e94f71ba37d75a84b196ce0801eb13b94a181c4c34ed15f2ec1c5fd1899d8953a546b8c164d18c6",
                priv: "6215a59058ba689889a3aa0e32d0f686ddb7a3ffae003376f4d0744eb7b61b19"
            );

            this.Bob = new TestPerson(
                pub: "049788818055d962297edbeb50431b8c44f545714bf0ce5471159cecd86c78efe2742778ad242ac325fb48217351f70782db8bf50f633b59f2bdbdcd1a08f1f7aa",
                priv: "d57cc0624f024149c300c0897ac917c58142be3d6346797651c85ca8dbae05f8"
            );
			this.Chad = new TestPerson(
				pub: "040c9ec013d34f362445ec1f3b87feaf857cee2ae45d8506c62a53ecb23147219993ee8dec6e1eaf25edf3757871df344636b1defe2b31b06ea5858e2c529db4d2",
				priv: "3e4af283ebb2716f0212b7079466f3b75cc3d4189877ab7953e48c826eebdd4d"
			);
            
	    	// For these tests, Bob and Alice will be establishing a shared secret key using the ECDH method and AES symmetric encryption        
            // Chad is a bad actor and a shared secret will not be generated. 
			this.BobSM = new SecureMessaging(this.Bob.privKey, this.Alice.pubKey, Network.TestNet);
			this.AliceSM = new SecureMessaging(this.Alice.privKey, this.Bob.pubKey, Network.TestNet);
		}
        /// <summary>
        /// Tests the shared secret algo1.
        /// </summary>
		/// First test case from NBitcoin unit test "src/NBitcoin.Tests/util_tests.cs"
		[Fact]
		public void TestSharedSecretAlgo1()
		{
			// Set up
			TestPerson Alice1 = new TestPerson(
                pub: "04a5cf05bfe42daffaff4f1732f5868ed7c7919cba279fa7d940e6b02a8b059bde56be218077bcab1ad6b5f5dcb04c42534477fb8d21b6312b0063e08a8ae52b3e",
                priv: "1249b289c5959c71ae60e0a2a7d57dffbd5cb862aaf10442db205f6787791732"
            ); // Invalid key-pair, only pub used
            TestPerson Bob1 = new TestPerson(
                pub: "043f12235bcf2776c8489ed138d4c9b85a1e29f3f4ad2787b9c8588e960867afc9de1e5702caa787665f5d0a4b04015c8bd5f1541e3d170efc3668f6ac587d43bc",
                priv: "7bd0db101160c888e9643f10594185a36a8db91b5308aaa7aad4c03245c6bdc1"
            ); // Invalid key-pair, only priv used
			string expectedKey = "a461392f592ff4292bfce732d808a07f1bc3f49c9a66a40d50761ffb8b2325f6";

            // Act
			SecureMessaging Bob1SM = new SecureMessaging(Bob1.privKey, Alice1.pubKey, Network.TestNet);

            // Assert
			Assert.Equal(Bob1SM.GetSharedSecret(), expectedKey);
		}

        /// <summary>
        /// Tests the shared secret algo2.
        /// </summary>
		/// Second test case from NBitcoin unit test "src/NBitcoin.Tests/util_tests.cs"
        [Fact]
		public void TestSharedSecretAlgo2()
        {
			// Set up
            TestPerson Alice2 = new TestPerson(
				pub: "043f12235bcf2776c8489ed138d4c9b85a1e29f3f4ad2787b9c8588e960867afc9de1e5702caa787665f5d0a4b04015c8bd5f1541e3d170efc3668f6ac587d43bc",
                priv: "1249b289c5959c71ae60e0a2a7d57dffbd5cb862aaf10442db205f6787791732"
            ); // Invalid key-pair, only pub used
            TestPerson Bob2 = new TestPerson(
                pub: "043f12235bcf2776c8489ed138d4c9b85a1e29f3f4ad2787b9c8588e960867afc9de1e5702caa787665f5d0a4b04015c8bd5f1541e3d170efc3668f6ac587d43bc",
				priv: "1249b289c5959c71ae60e0a2a7d57dffbd5cb862aaf10442db205f6787791732"
            ); // Invalid key-pair, only priv used
			string expectedKey = "1d664ba11d3925cfcd938b2ef131213ba4ca986822944d0a7616b34027738e7c";

            // Act
			SecureMessaging Bob2SM = new SecureMessaging(Bob2.privKey, Alice2.pubKey, Network.TestNet);

            // Assert
            Assert.Equal(Bob2SM.GetSharedSecret(), expectedKey);
        }

        /// <summary>
        /// Tests the shared secret algo3.
        /// </summary>
		/// Third test case from NBitcoin unit test "src/NBitcoin.Tests/util_tests.cs"
		[Fact]
        public void TestSharedSecretAlgo3()
        {
			// Set up
            TestPerson Alice3 = new TestPerson(
				pub: "04769c29328998917d9f2f7c6ce46f2f12a6064e937dff722b4811e9c88b4e1d45387fea132321541e8dbdc92384aef1944d650aa889bfa836db078897e5299262",
                priv: "1249b289c5959c71ae60e0a2a7d57dffbd5cb862aaf10442db205f6787791732"
            ); // Invalid key-pair, only pub used
            TestPerson Bob3 = new TestPerson(
                pub: "043f12235bcf2776c8489ed138d4c9b85a1e29f3f4ad2787b9c8588e960867afc9de1e5702caa787665f5d0a4b04015c8bd5f1541e3d170efc3668f6ac587d43bc",
				priv: "41d0cbeeb3365b8c9e190f9898689997002f94006ad3bf1dcfbac28b6e4fb84d"
            ); // Invalid key-pair, only priv used
			string expectedKey = "7fcfa754a40ceaabee5cd3df1a99ee2e5d2c027fdcbd8e437d9be757ea58708f";

            // Act
			SecureMessaging Bob3SM = new SecureMessaging(Bob3.privKey, Alice3.pubKey, Network.TestNet);

            // Assert
            Assert.Equal(Bob3SM.GetSharedSecret(), expectedKey);
        }

        /// <summary>
        /// Tests the matching shared secrets.
        /// </summary>
		[Fact]
        public void TestMatchingSharedSecrets()
        {   
            // Set up performed during initialization of the test persons

            // Act and Assert
			Assert.Equal(this.BobSM.GetSharedSecret(), this.AliceSM.GetSharedSecret());
        }
        
        /// <summary>
        /// Tests the bob message to alice.
        /// </summary>
		[Fact]
        public void TestBobMessageToAlice()
		{
			// Set up
            string BobPlainTextMessage = "Have a great day Alice!";
            
            // Act
			string BobCipher = this.BobSM.EncryptMessage(BobPlainTextMessage);
			string AliceDecryptedPlainTextMessage = this.AliceSM.DecryptMessage(BobCipher);

            // Assert
			Assert.Equal(BobPlainTextMessage, AliceDecryptedPlainTextMessage);
		}

        /// <summary>
        /// Tests the alice message to bob.
        /// </summary>
		[Fact]
        public void TestAliceMessageToBob()
        {
			// Set Up
			string AlicePlainTextMessage = "Bugger off, Bob";
            
            // Act
			string AliceCipher = this.AliceSM.EncryptMessage(AlicePlainTextMessage);
			string BobDecryptedPlainTextMessage = this.BobSM.DecryptMessage(AliceCipher);

            // Assert
			Assert.Equal(AlicePlainTextMessage, BobDecryptedPlainTextMessage);
        }

        /// <summary>
        /// Tests the alice long message to bob.
        /// </summary>
		[Fact]
        public void TestAliceLongMessageToBob()
        {
			// Set up
			string AlicePlainTextMessage = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cras eget varius massa. Nulla non dictum turpis, quis viverra odio. Suspendisse commodo, mauris at convallis scelerisque, quam diam sodales sapien, sit amet dictum odio tortor ut est. Nullam vulputate venenatis turpis vitae porttitor. Integer quis turpis dapibus, viverra orci vel, molestie odio. Aliquam pulvinar dapibus nisl, non vehicula est efficitur feugiat. Nunc egestas mi sit amet nibh pretium mollis. Vestibulum dui dui, elementum non tempor non, auctor nec ante. Quisque luctus pretium posuere. Quisque mattis, sem varius consectetur sollicitudin, ex arcu fermentum risus, id finibus risus leo et enim. In molestie volutpat arcu sit amet condimentum. Maecenas nec enim lobortis arcu imperdiet hendrerit. Maecenas pulvinar justo eget commodo fermentum. Nulla porttitor dapibus felis eu ultricies. 

Phasellus ac dignissim velit, quis luctus lectus. Cras convallis vitae tortor rutrum commodo. Nunc vitae lobortis elit, eget gravida neque. Sed at auctor lorem. Vivamus cursus nibh lacus, quis cursus arcu volutpat vel. Aenean id gravida odio. Vestibulum a mollis elit.

Quisque tellus dolor, tempor eu tortor sit amet, finibus tincidunt augue.Etiam odio justo, laoreet non nunc ut, posuere sodales urna. Nunc mattis et lectus at gravida. Proin vel risus vitae tortor faucibus posuere in et enim. Maecenas sit amet purus tincidunt justo fringilla tempus sit amet lobortis nunc. In ultricies, odio sed tempus rhoncus, urna mauris hendrerit dui, condimentum pretium enim eros id diam.Maecenas eu fermentum lorem. Cras id facilisis purus, non ultrices turpis. Pellentesque nisi tortor, viverra eu rhoncus sit amet, accumsan et ipsum.Praesent varius nibh ut vulputate dictum. Vestibulum quis suscipit purus, at sollicitudin augue. Maecenas ac maximus turpis, sed dignissim lacus. Nullam fermentum lobortis vulputate. Suspendisse ac magna pharetra arcu cursus tempus sed eu turpis. Aenean elementum a libero at commodo.";
            
            // Act
			string AliceCipher = this.AliceSM.EncryptMessage(AlicePlainTextMessage);
            string BobDecryptedPlainTextMessage = this.BobSM.DecryptMessage(AliceCipher);

            // Assert
            Assert.Equal(AlicePlainTextMessage, BobDecryptedPlainTextMessage);
        }

        /// <summary>
        /// Tests Chad intercepting the message.
        /// </summary>
        [Fact]
        public void TestChadInterceptMessage() 
		{
			// Set up
			string AlicePlainTextMessage = "I think Chad is spying on us.";

            // Act
			string AliceCipher = this.AliceSM.EncryptMessage(AlicePlainTextMessage);

            // Assert
			SecureMessaging ChadSM = new SecureMessaging(this.Chad.privKey, this.Alice.pubKey, Network.TestNet);
			try
			{
				string ChadDecryptedPlainTextMessage = ChadSM.DecryptMessage(AliceCipher);
			} 
			catch (Exception e) 
            {
				Assert.True(e is System.Security.Cryptography.CryptographicException);
			}
            
		}
		/// TODO 
        /// <summary>
        /// Tests the stealth address generation.
        /// </summary>
        [Fact]
        public void TestStealthAddressGeneration()
		{
			
		}

        /// <summary>
        /// Tests the op return message builder.
        /// </summary>
		[Fact]
        public void TestOpReturnMessageBuilder()
		{
        
		}

        /// <summary>
        /// Tests the transaction builder.
        /// </summary>
		[Fact]
        public void TestTransactionBuilder()
		{
			
		}
    }
}
