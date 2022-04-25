using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace QbModels.QbOnlineProcessor
{
    public static class Config
    {
		#region Client ID Info
		private static ClientInfoDto clientInfo = new();
		public static ClientInfoDto ClientInfo => clientInfo;

		public static void SetClientInfo(string info) => clientInfo.SetClientInfo(JsonSerializer.Deserialize<ClientInfoDto>(info));
        #endregion

        #region Endpoints
        private static string qbDiscoveryUri = "https://developer.api.intuit.com/.well-known/openid_configuration";
		public static string QbDiscoveryUri => qbDiscoveryUri;
		private static DiscoveryEndpoints qboeEndpoints;
		public static DiscoveryEndpoints QboeEndpoints => qboeEndpoints;
        public static void OverrideDiscoveryEndpoint(string discoveryUri) => qbDiscoveryUri = discoveryUri;
        public static void SetEndpoints(DiscoveryEndpoints endpoints) => qboeEndpoints = endpoints;
        #endregion

        public static async Task<HttpClient> QbOnlineHttpClientAsync(bool returnJson = false)
		{
			string endpoint = @"https://sandbox-quickbooks.api.intuit.com";

			if (QbOnlineClient.AccessToken == null || string.IsNullOrEmpty(QbOnlineClient.AccessToken.AccessToken)) return null;
			if (QbOnlineClient.AccessToken.ShouldRefresh)
			{
				bool tokenRefreshed = await QbOnlineClient.RefreshAccessTokenAsync();
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
			qboeHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", QbOnlineClient.AccessToken.AccessToken);

			return qboeHttpClient;
		}
	}
}
