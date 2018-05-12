using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Stratis.Bitcoin.Features.SecureMessaging.Models
{
    /// <summary>
    /// Get destination script pub key request.
    /// </summary>
	public class GetDestScriptPubKeyRequest
    {      
		[Required(ErrorMessage = "The destination address is missing.")]
		public string DestinationAddress { get; set; }        
    }
}
