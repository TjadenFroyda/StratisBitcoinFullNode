using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Stratis.Bitcoin.Features.SecureMessaging.Models
{
    /// <summary>
    /// Send secure message request.
    /// </summary>
    public class ActionMessageRequest : SecureMessageKeyRequest
    {
        [Required(ErrorMessage = "The message is missing")]
        public string Message { get; set; }
 
        public string AccountName { get; set; }
 
        [Required(ErrorMessage = "The receiver's public key is required.")]
        public string ReceiverPublicKey { get; set; }
    }
}
