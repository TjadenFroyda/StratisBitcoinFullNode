using Stratis.Bitcoin.Features.SecureMessaging.Interfaces;
using Stratis.Bitcoin.Tests.Common.Logging;

// TODO: Add Logging
// TODO: Add/improve Comments
// TODO: Check coding style guide
// TODO: Safety checks
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
            // TODO: This was tested functionally in the SecureMessaging unit tests, so should be trivial. 
        }

        /// <summary>
        /// Tests the decrypt.
        /// </summary>
        public void TestDecrypt()
        {
            // TODO: This was tested functionally in the SecureMessaging unit tests, so should be trivial. 
        }

        /// <summary>
        /// Tests the bad secret too long.
        /// </summary>
        public void TestBadSecretTooLong()
        {
            ISymmetricEncryption tooLongSecret = new AES("2116a712e29181ee79ef070dea21dc20fe3e02bd02ab916a90e8f202c77be59916a712e29181ee79ef070d");
            // TODO: Need to ensure proper error handling. 
            // Private key for EC should be 64 Hex characters (32 bytes).
            // Could consider combining this test with the too short test below. 
        }

        /// <summary>
        /// Tests the bad secret too short.
        /// </summary>
        public void TestBadSecretTooShort()
        {
            ISymmetricEncryption tooShortSecret = new AES("2116a712e29181ee79ef070dea21dc2090e8f202c77be599");
            // TODO: Need to ensure proper error handling. 
            // I anticipate many people will try using the receiving address as the public shared key.
            // This is shorter than the shared secret, so should give corrective prompt. 
        }
    }
}
