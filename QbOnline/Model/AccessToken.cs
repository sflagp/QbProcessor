using System;
using System.Text.Json.Serialization;

namespace QbModels.QbOnlineProcessor
{
    public class QboeAccessToken
    {
        public QboeAccessToken()
        {
            TimeCreated = DateTime.Now;
        }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("x_refresh_token_expires_in")]
        public int RefreshTokenExpiresIn { get; set; }

        public DateTime TimeCreated { get; set; }

        public DateTime Expires => TimeCreated.AddSeconds(ExpiresIn);

        public DateTime RefreshTokenExpires => TimeCreated.AddSeconds(RefreshTokenExpiresIn);

        public bool ShouldRefresh => (Expires - DateTime.Now).TotalSeconds < 120;
    }
}
