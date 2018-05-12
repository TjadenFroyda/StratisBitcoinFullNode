using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Stratis.Bitcoin.Features.SecureMessaging.Models
{
    /// <summary>
    /// Get private key request.
    /// </summary>
	public class GetPrivateKeyRequest
    {
		[Required(ErrorMessage = "Please enter the account name.")]
		public string AccountName { get; set; }

        public string SenderPrivateKey { get; set; }

        public string Passphrase { get; set; }
        
        public string WalletName { get; set; }      
    }
}
