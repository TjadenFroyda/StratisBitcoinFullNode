
using NBitcoin;
using NBitcoin.DataEncoders;

// TODO: Add/improve Comments
namespace Stratis.Bitcoin.Features.SecureMessaging.Tests
{
    /// <summary>
    /// Test person class to help with Unit Testing 
    /// </summary>
    public class TestPerson
    {
        private Key privKey;
        private PubKey pubKey;

        public TestPerson(string pub, string priv)
        {
            this.privKey = new Key(Encoders.Hex.DecodeData(priv));
            this.pubKey = new PubKey(pub);
        }
        public string GetPrivateKeyHex()
        {
            return this.privKey.ToHex(Network.Main);
        }
        public Key GetPrivateKey()
        {
            return this.privKey;
        }
        public string GetPublicKeyHex()
        {
            return this.pubKey.ToHex(Network.Main);
        }
        public PubKey GetPublicKey()
        {
            return this.pubKey;
        }
    }
}
