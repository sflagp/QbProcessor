using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor
{
    internal static class QBOClient
    {
        public static async Task<HttpResponseMessage> DiscoverEndpointsAsync()
        {
            using HttpClient wsQbDiscovery = new HttpClient();
            return await wsQbDiscovery.GetAsync(Settings.DiscoveryUri);
        }

        public static async Task<string> GetAuthCodesAsync()
        {
            AuthCodeResponse authCodeResponse = new();
            string authScope = Settings.AuthScope;
            string redirectUrl = Settings.RedirectUri;
            string authState = $"security_token{Guid.NewGuid()}";

            using HttpClient httpClient = new();
            httpClient.BaseAddress = new Uri(Settings.QboDiscoveryEndpoints.AuthorizationEndpoint);
            string rqParam = $"?client_id={Settings.ClientInfo.ClientId}&scope={authScope}&redirect_uri={redirectUrl}&response_type=code&state={authState}";

            using (HttpListener authListener = new HttpListener())
            {
                try
                {
                    authListener.Prefixes.Add($"{Settings.RedirectUri}/");
                    authListener.Start();

                    ProcessStartInfo authRq = new ProcessStartInfo($"{Settings.QboDiscoveryEndpoints.AuthorizationEndpoint}{rqParam}")
                    {
                        UseShellExecute = true,
                        Verb = "Open"
                    };
                    Process.Start(authRq);

                    var authCtxt = await authListener.GetContextAsync();

                    // Sends an HTTP response to the browser.
                    authCodeResponse = extractAuthCode(authCtxt);
                    var authResp = authCtxt.Response;
                    Stream authOutput = authResp.OutputStream;
                    string responseString = GenAuthRespHtml(authState, authCodeResponse);
                    var buffer = Encoding.UTF8.GetBytes(responseString);
                    authResp.ContentLength64 = buffer.Length;
                    Task responseTask = authOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
                    {
                        authOutput.Close();
                        authListener.Stop();
                        Console.WriteLine("HTTP server stopped.");
                    });
                    if(authCodeResponse.State != authState)
                    {
                        return null;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"{ex.HResult} {ex.Message}");
                }
            }

            return authCodeResponse.AuthCode;
        }

        private static AuthCodeResponse extractAuthCode(HttpListenerContext ctxt)
        {
            AuthCodeResponse response = new();
            string[] codeResp = ctxt.Request.Url.ToString().Replace($"{Settings.RedirectUri}?", string.Empty).Split('?', '&');
            foreach(string resp in codeResp)
            {
                string[] r = resp.Split('=');
                switch (r[0].ToLower())
                {
                    case "code":
                        response.AuthCode = r[1];
                        break;
                    case "state":
                        response.State = r[1];
                        break;
                    case "realmid":
                        response.RealmId = r[1];
                        break;
                }
            }
            return response;
        }
        
        private static string GenAuthRespHtml(string authState, AuthCodeResponse authCodeResponse)
        {
            return $@"
                        <html>
                            <head>
                                <h1>Authentication response</h1><br />
                                Authentication State = {authState}<br />
                            </head>
                            <body>
                                Authorization State = {authCodeResponse.State}<br />
                                Authorization Code = {authCodeResponse.AuthCode}<br />
                                RealmId = {authCodeResponse.RealmId}<br />
                            </body>
                        </html>
            ";
        }

        public static async Task<bool> SetAccessTokenAsync(string authCode)
        {
            string authHeader = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(Settings.ClientInfo.ClientId + ":" + Settings.ClientInfo.ClientSecret))}";
            string redirectUrl = Settings.RedirectUri;

            HttpRequestMessage request = new(HttpMethod.Post, Settings.QboDiscoveryEndpoints.TokenEndpoint);

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
                QboAccessToken accessToken = await JsonSerializer.DeserializeAsync<QboAccessToken>(await response.Content.ReadAsStreamAsync());
                Settings.AccessToken = accessToken;
                Settings.AccessToken.TokenCreated = DateTime.Now;
                Settings.SaveSettings();
            }
            return response.IsSuccessStatusCode;
        }

        public static async Task<bool> RefreshAccessTokenAsync()
        {
            string authHeader = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes(Settings.ClientInfo.ClientId + ":" + Settings.ClientInfo.ClientSecret))}";

            HttpRequestMessage request = new(HttpMethod.Post, Settings.QboDiscoveryEndpoints.TokenEndpoint);

            request.Headers.TryAddWithoutValidation("Accept", "application/json");
            request.Headers.TryAddWithoutValidation("Authorization", authHeader);

            List<KeyValuePair<string, string>> contentList = new();
            contentList.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
            contentList.Add(new KeyValuePair<string, string>("refresh_token", Settings.AccessToken.RefreshToken));
            FormUrlEncodedContent formContent = new(contentList);
            request.Content = formContent;
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            using HttpClient httpClient = new();
            using HttpResponseMessage response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                DateTime tokenCreated = Settings.AccessToken.TokenCreated;
                Settings.AccessToken = await JsonSerializer.DeserializeAsync<QboAccessToken>(await response.Content.ReadAsStreamAsync());
                Settings.AccessToken.TokenCreated = tokenCreated;
                Settings.AccessToken.RefreshTokenCreated = DateTime.Now;
                Settings.SaveSettings();
            }
            return response.IsSuccessStatusCode;
        }

        public static async Task<HttpResponseMessage> GetQBOAsync(string parameter, bool asXml)
        {
            HttpResponseMessage getRs;
            using (var wsQboeWeb = await Config.QBOHttpClientAsync(asXml))
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

        public static async Task<HttpResponseMessage> PostQBOAsync<T>(string parameter, T data)
        {
            HttpResponseMessage postRs;
            using (var wsQboeWeb = await Config.QBOHttpClientAsync(false))
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
            content.Headers.ContentType = new MediaTypeHeaderValue(dataType);
            return content;
        }
    }
}
