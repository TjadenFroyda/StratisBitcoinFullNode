using Stratis.Bitcoin.Features.SecureMessaging.Interfaces;
using Stratis.Bitcoin.Tests.Common.Logging;

namespace Stratis.Bitcoin.Features.SecureMessaging.Tests
{
	public class SymmetricEncryptionTests : LogsTestBase
    {
		private ISymmetricEncryption SymmetricEncryption;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Stratis.Bitcoin.Features.SecureMessaging.Tests.SymmetricEncryptionTests"/> class.
        /// </summary>
        public SymmetricEncryptionTests()
        {
			// Private key enerated from https://kjur.github.io/jsrsasign/sample/sample-ecdsa.html using SECP256K1
			this.SymmetricEncryption = new AES("2116a712e29181ee79ef070dea21dc20fe3e02bd02ab916a90e8f202c77be599");
        }

        /// <summary>
        /// Tests the encrypt.
        /// </summary>
        public void TestEncrypt()
		{
			
		}

        /// <summary>
        /// Tests the decrypt.
        /// </summary>
        public void TestDecrypt()
		{
			
		}

        /// <summary>
        /// Tests the bad secret too long.
        /// </summary>
        public void TestBadSecretTooLong()
		{
			ISymmetricEncryption tooLongSecret = new AES("2116a712e29181ee79ef070dea21dc20fe3e02bd02ab916a90e8f202c77be59916a712e29181ee79ef070d");
			
		}

        /// <summary>
        /// Tests the bad secret too short.
        /// </summary>
		public void TestBadSecretTooShort()
		{
			ISymmetricEncryption tooShortSecret = new AES("2116a712e29181ee79ef070dea21dc2090e8f202c77be599");
		}
    }
}
