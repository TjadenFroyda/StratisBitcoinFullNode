using System;

// TODO: Add/improve Comments
// TODO: Check coding style guide
// TODO: Safety checks
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
}
