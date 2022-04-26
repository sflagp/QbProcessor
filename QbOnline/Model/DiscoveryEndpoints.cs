using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QbModels.QBOProcessor
{
    public class DiscoveryEndpoints
    {
        [JsonPropertyName("issuer")]
        public string Issuer { get; set; }

        [JsonPropertyName("authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; }

        [JsonPropertyName("token_endpoint")]
        public string TokenEndpoint { get; set; }

        [JsonPropertyName("userinfo_endpoint")]
        public string UserInfoEndpoint { get; set; }

        [JsonPropertyName("revocation_endpoint")]
        public string RevocationEndpoint { get; set; }

        [JsonPropertyName("jwks_uri")]
        public string JwksUri { get; set; }

        [JsonPropertyName("response_types_supported")]
        public List<string> ResponseTypesSupported { get; set; }

        [JsonPropertyName("id_token_signing_alg_values_supported")]
        public List<string> IdTokenSigningAlgorithmsSupported { get; set; }

        [JsonPropertyName("scopes_supported")]
        public List<string> ScopesSupported { get; set; }

        [JsonPropertyName("token_endpoint_auth_methods_supported")]
        public List<string> TokenAuthMethodsSupported { get; set; }

        [JsonPropertyName("claims_Supported")]
        public List<string> ClaimsSupported { get; set; }

        public string TokenEndpointBaseAddress
        {
            get
            {
                if (string.IsNullOrEmpty(TokenEndpoint)) return "";
                int index = TokenEndpoint.IndexOf('/', 10);
                return TokenEndpoint.Substring(0, index);
            }
        }
        public string TokenEndpointParameter
        {
            get
            {
                if (string.IsNullOrEmpty(TokenEndpoint)) return "";
                int index = TokenEndpoint.IndexOf('/', 10);
                return TokenEndpoint.Substring(index + 1);
            }
        }
    }
}
