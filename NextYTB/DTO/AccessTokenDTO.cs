using Newtonsoft.Json;

namespace NextYTB.DTO
{
    public class AccessTokenDTO
    {
        [JsonProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")]
        public string Email { get; set; }

        public int Id { get; set; }

        public long Exp { get; set; }

        public string Iss { get; set; }

        public string Aud { get; set; }
    }
}
