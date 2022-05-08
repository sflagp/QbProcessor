using System;
using System.Text.Json.Serialization;

namespace QbModels.QBOProcessor
{
    public class QboAccessToken
    {
        public QboAccessToken() { }// { TimeCreated = DateTime.Now; }

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

        [JsonPropertyName("tokenCreated")]
        public DateTime TokenCreated { get; set; }

        [JsonPropertyName("refreshTokenCreated")]
        public DateTime RefreshTokenCreated { get; set; }

        public DateTime Expires => RefreshTokenCreated == default ? TokenCreated.AddSeconds(ExpiresIn) : RefreshTokenCreated.AddSeconds(ExpiresIn);

        public DateTime RefreshTokenExpires => RefreshTokenCreated.AddSeconds(RefreshTokenExpiresIn);

        public bool ShouldRefresh => (Expires - DateTime.Now).TotalSeconds < 120;
    }
}
