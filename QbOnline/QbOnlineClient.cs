using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QbModels.QbOnlineProcessor
{
    internal static class QbOnlineClient
    {
        public static QboeAccessToken AccessToken { get; private set; }

        public static void SetToken(string accessToken, string refreshToken) => AccessToken = new() { AccessToken = accessToken, RefreshToken = refreshToken };

        public static async Task<HttpResponseMessage> DiscoverEndpointsAsync()
        {
            using HttpClient wsQbDiscovery = new HttpClient();
            return await wsQbDiscovery.GetAsync(Config.QbDiscoveryUri);
        }

        public static async Task<string> GetAuthCodesAsync()
        {
            string authScope = "com.intuit.quickbooks.accounting";
            string redirectUrl = "https://developer.intuit.com/v2/OAuth2Playground/RedirectUrl";
            string authState = $"security_token{Guid.NewGuid()}";

            using HttpClient httpClient = new();
            httpClient.BaseAddress = new Uri(Config.QboeEndpoints.AuthorizationEndpoint);
            string rqParam = $"client_id={Config.ClientInfo.ClientId}&response_type=code&scope={authScope}&redirect_uri={redirectUrl}&state={authState}";
            HttpResponseMessage response = await httpClient.GetAsync(rqParam);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }

        public static async Task<bool> SetAccessTokenAsync(string authCode = null)
        {
            if (string.IsNullOrEmpty(authCode))
            {
                return await RefreshAccessTokenAsync();
            }
            string authHeader = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(Config.ClientInfo.ClientId + ":" + Config.ClientInfo.ClientSecret))}";
            string redirectUrl = "https://developer.intuit.com/v2/OAuth2Playground/RedirectUrl";

            HttpRequestMessage request = new(HttpMethod.Post, Config.QboeEndpoints.TokenEndpoint);

            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("Authorization", authHeader);

            List<KeyValuePair<string, string>> contentList = new();
            contentList.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
            contentList.Add(new KeyValuePair<string, string>("code", authCode));
            contentList.Add(new KeyValuePair<string, string>("redirect_uri", redirectUrl));
            FormUrlEncodedContent formContent = new(contentList);
            request.Content = formContent;
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            using HttpClient httpClient = new();
            using HttpResponseMessage response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                AccessToken = await JsonSerializer.DeserializeAsync<QboeAccessToken>(await response.Content.ReadAsStreamAsync());
                File.WriteAllBytes(@".\AccessToken.json", await response.Content.ReadAsByteArrayAsync());
            }
            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> RefreshAccessTokenAsync()
        {
            string authHeader = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(Config.ClientInfo.ClientId + ":" + Config.ClientInfo.ClientSecret))}";

            HttpRequestMessage request = new(HttpMethod.Post, Config.QboeEndpoints.TokenEndpoint);

            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("Authorization", authHeader);

            List<KeyValuePair<string, string>> contentList = new();
            contentList.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
            contentList.Add(new KeyValuePair<string, string>("refresh_token", AccessToken.RefreshToken));
            FormUrlEncodedContent formContent = new(contentList);
            request.Content = formContent;
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            using HttpClient httpClient = new();
            using HttpResponseMessage response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                AccessToken = await JsonSerializer.DeserializeAsync<QboeAccessToken>(await response.Content.ReadAsStreamAsync());
            }
            return response.IsSuccessStatusCode;
        }

        public static async Task<HttpResponseMessage> GetQbOnlineWSAsync(string parameter)
        {
            HttpResponseMessage getRs;
            using (var wsQboeWeb = await Config.QbOnlineHttpClientAsync(false))
            {
                wsQboeWeb.DefaultRequestHeaders.Add("Access-Control-Request-Method", "GET");
                try
                {
                    getRs = await wsQboeWeb.GetAsync($"{parameter}");
                }
                catch (Exception ex)
                {
                    throw new HttpRequestException($"Error:  {ex.HResult}\n{ex.Message}");
                }
            }
            return getRs;
        }

        public static async Task<HttpResponseMessage> PostQbOnlineWSAsync<T>(string parameter, T data)
        {
            HttpResponseMessage postRs;
            using (var wsQboeWeb = await Config.QbOnlineHttpClientAsync(false))
            {
                var content = NewStringContent<T>(data);

                try
                {
                    postRs = await wsQboeWeb.PostAsync($"{parameter}", content);
                }
                catch (Exception ex)
                {
                    throw new HttpRequestException($"Error:  {ex.HResult}\n{ex.Message}");
                }
            }
            return postRs;
        }

        private static StringContent NewStringContent<T>(T data, string dataType = "application/xml")
        {
            var content = new StringContent(data.ToString());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dataType);
            return content;
        }
    }
}
