using System;
using System.Collections.Generic;
using System.Text;
using NBitcoin;
using NBitcoin.DataEncoders;
using Stratis.Bitcoin.Features.Wallet;

namespace Stratis.Bitcoin.Features.SecureMessaging
{
    public class Contact
    {
        private Network network;
        private Key internalPrivateKey;
        private PubKey internalPublicKey;
        private PubKey externalPublicKey;
        private BitcoinAddress contactReceivingAddress;
        private BitcoinAddress myReceivingAddress;
        private SecureMessagingEngine sm;
        private Key sharedSecret;
        private CoinType coinType;
        private string name;

        public Contact(string name, Key privateKey, PubKey externalPubKey, Network network)
        {
            this.name = name;
            this.network = network;
            this.coinType = CoinType.Stratis;
            this.internalPrivateKey = privateKey;
            this.internalPublicKey = this.internalPrivateKey.PubKey;
            this.externalPublicKey = externalPubKey;
            this.sm = new SecureMessagingEngine(this.internalPrivateKey, this.externalPublicKey, this.network);
            this.sharedSecret = this.sm.GetSharedSecretMasterPrivateKey();
        }

        internal void SetSendAndReceiveAddresses()
        {
            // First need to generate 2 addresses from the private seed.
            ExtKey seedKey = new ExtKey(this.sharedSecret.ToHex(this.network));
            ExtPubKey seedPubKey = new ExtPubKey(seedKey.Neuter().ToBytes());
            List<HdAddress> addresses = new List<HdAddress>();
            int AddressesToCreate = 10;

            for (int i = 0; i < AddressesToCreate; i++)
            {
                // Generate a new address.
                PubKey pubkey = HdOperations.GeneratePublicKey(seedPubKey.ToString(this.network), i, false);
                BitcoinPubKeyAddress address = pubkey.GetAddress(this.network);

                // Add the new address details to the list of addresses.
                HdAddress newAddress = new HdAddress
                {
                    Index = i,
                    HdPath = HdOperations.CreateHdPath((int)this.coinType, 0, i, false),
                    ScriptPubKey = address.ScriptPubKey,
                    Pubkey = pubkey.ScriptPubKey,
                    Address = address.ToString(),
                    Transactions = new List<TransactionData>()
                };

                addresses.Add(newAddress);
                addressesCreated.Add(newAddress);
            }
        }
    }
}
