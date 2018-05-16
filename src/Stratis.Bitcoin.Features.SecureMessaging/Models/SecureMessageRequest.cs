using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Stratis.Bitcoin.Features.SecureMessaging.Models
{
    /// <summary>
    /// Send secure message request.
    /// </summary>
    public class SecureMessageRequest : ActionMessageRequest
    {
		[Required(ErrorMessage = "The receiver's address is required.")]
        public string DestinationAddress { get; set; }
    }
}
