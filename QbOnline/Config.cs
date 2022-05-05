using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor
{
    public static class Config
    {
        public static async Task<HttpClient> QBOHttpClientAsync(bool returnXml = false)
        {
            string endpoint = Settings.IntuitEndpoint;

            if (string.IsNullOrEmpty(Settings.AccessToken?.AccessToken)) return null;
            if (Settings.AccessToken.ShouldRefresh)
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

            if (!returnXml) qboeHttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            qboeHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", Settings.AccessToken.AccessToken);

            return qboeHttpClient;
        }
    }
}
