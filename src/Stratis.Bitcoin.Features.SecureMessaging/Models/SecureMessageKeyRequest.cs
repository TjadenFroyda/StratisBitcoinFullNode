using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Stratis.Bitcoin.Features.SecureMessaging.Models
{
    /// <summary>
    /// Get private key request.
    /// </summary>
	public class SecureMessageKeyRequest : RequestModel
    {
        public string SenderPrivateKey { get; set; }

        public string Passphrase { get; set; }
    
        public string WalletName { get; set; }      
    }
}
