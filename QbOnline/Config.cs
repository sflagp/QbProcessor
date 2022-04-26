using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor
{
    public static class Config
    {
		#region Client ID Info
		private static ClientInfoDto clientInfo = new();
		public static ClientInfoDto ClientInfo => clientInfo;

		public static void SetClientInfo(string info) => clientInfo.SetClientInfo(JsonSerializer.Deserialize<ClientInfoDto>(info));
        #endregion

        #region Endpoints
        private static string qboDiscoveryUri = "https://developer.api.intuit.com/.well-known/openid_configuration";
		public static string QboDiscoveryUri => qboDiscoveryUri;
		private static DiscoveryEndpoints qboEndpoints;
		public static DiscoveryEndpoints QboEndpoints => qboEndpoints;
        public static void OverrideDiscoveryEndpoint(string discoveryUri) => qboDiscoveryUri = discoveryUri;
        public static void SetEndpoints(DiscoveryEndpoints endpoints) => qboEndpoints = endpoints;
        #endregion

        public static async Task<HttpClient> QBOHttpClientAsync(bool returnJson = false)
		{
			string endpoint = @"https://sandbox-quickbooks.api.intuit.com";

			if (QBOClient.AccessToken == null || string.IsNullOrEmpty(QBOClient.AccessToken.AccessToken)) return null;
			if (QBOClient.AccessToken.ShouldRefresh)
			{
				bool tokenRefreshed = await QBOClient.RefreshAccessTokenAsync();
				if (!tokenRefreshed)
				{
					throw new HttpRequestException($"Token could not be refreshed");
				}
			}

			HttpClient qboeHttpClient = new()
			{
				BaseAddress = new Uri(endpoint)
			};

			if(returnJson) qboeHttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
			qboeHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", QBOClient.AccessToken.AccessToken);

			return qboeHttpClient;
		}
	}
}
