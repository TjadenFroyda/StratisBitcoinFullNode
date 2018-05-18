
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
        private Network network;

        public TestPerson(string pub, string priv, Network net)
        {
            this.privKey = new Key(Encoders.Hex.DecodeData(priv));
            this.pubKey = new PubKey(pub);
            this.network = net;
        }
        public string GetPrivateKeyHex()
        {
            return this.privKey.ToHex(this.network);
        }
        public Key GetPrivateKey()
        {
            return this.privKey;
        }
        public string GetPublicKeyHex()
        {
            return this.pubKey.ToHex(this.network);
        }
        public PubKey GetPublicKey()
        {
            return this.pubKey;
        }
    }
}
