using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Stratis.Bitcoin.Features.SecureMessaging.Models
{
	/// <summary>
    /// Send secure message request.
    /// </summary>
	public class SecureMessageRequest
    {
		[Required(ErrorMessage = "Please enter the account name.")]
		public string AccountName { get; set; }
        
        public string DestinationAddress { get; set; }

		public string Network { get; set; }

        public string SenderPrivateKey { get; set; }

        public string Passphrase { get; set; }

        [Required(ErrorMessage = "The message is missing")]
        public string Message { get; set; }

        [Required(ErrorMessage = "The receiver's public key is required.")]
        public string ReceiverPublicKey { get; set; }

        public string WalletName { get; set; }
    }
}
