using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QbModels.QbOnlineProcessor
{
    public class QbOnlineProcessor : IDisposable
    {
        public QbOnlineProcessor() { }

        private bool disposedValue;
        private string authCodePage;
        public string AuthCodePage => authCodePage;
        public ClientInfoDto ClientInfo => Config.ClientInfo;

        public static void SetClientInfo(string info) => Config.SetClientInfo(info);

        public QboeAccessToken AccessToken => QbOnlineClient.AccessToken;

        public async Task<bool> GetEndpointsAsync()
        {
            HttpResponseMessage response = await QbOnlineClient.DiscoverEndpointsAsync();
            if (response.IsSuccessStatusCode)
            {
                DiscoveryEndpoints endpoints = await JsonSerializer.DeserializeAsync<DiscoveryEndpoints>(await response.Content.ReadAsStreamAsync());
                Config.SetEndpoints(endpoints);
                return true;
            }
            return false;
        }

        public async Task<bool> GetAuthCodesAsync()
        {
            authCodePage = await QbOnlineClient.GetAuthCodesAsync();
            if (!string.IsNullOrEmpty(authCodePage))
            {
                File.WriteAllBytes(@".\GetAuthCode.html", Encoding.ASCII.GetBytes(authCodePage));
                return true;
            }
            return false;
        }

        public async Task<bool> SetAccessTokenAsync(string authCode)
        {
            return await QbOnlineClient.SetAccessTokenAsync(authCode);
        }

        public async Task<bool> RefreshAccessTokenAsync()
        {
            return await QbOnlineClient.RefreshAccessTokenAsync();
        }

        public void ManualAccessToken(string accessToken, string refreshToken) => QbOnlineClient.SetToken(accessToken, refreshToken);

        public async Task<HttpResponseMessage> QbOnlineGet(string parameter) => await QbOnlineClient.GetQbOnlineWSAsync(parameter);

        public async Task<HttpResponseMessage> QbOnlinePost<T>(string parameter, T data) => await QbOnlineClient.PostQbOnlineWSAsync<T>(parameter, data);

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~QboeProcessor()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
