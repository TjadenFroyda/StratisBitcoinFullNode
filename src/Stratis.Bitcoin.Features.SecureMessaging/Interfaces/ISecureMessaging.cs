using NBitcoin;
using Stratis.Bitcoin.Features.Wallet;
using System.Collections.Generic;

// TODO: Add/improve comments
namespace Stratis.Bitcoin.Features.SecureMessaging.Interfaces
{
    public interface ISecureMessaging
    {
        Key GetSharedSecretMasterPrivateKey();
        Script GetDestScriptPubKey();
        string EncryptMessage(string plaintextMessage);
        string DecryptMessage(string hexcipher);    
        List<string> prepareOPReturnMessageList(string hexEncodedEncryptedMessage);
    }
}
