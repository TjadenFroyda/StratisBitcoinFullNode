using Newtonsoft.Json;

namespace Stratis.Bitcoin.Features.SecureMessaging.Models
{
    public class RequestModel
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }    
}
