using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor
{
    public class QBOProcessor : IDisposable
    {
        public QBOProcessor() { }

        private bool disposedValue;
        private string authCodePage;
        public string AuthCodePage => authCodePage;
        public ClientInfoDto ClientInfo => Config.ClientInfo;

        public static void SetClientInfo(string info) => Config.SetClientInfo(info);

        public QboAccessToken AccessToken => QBOClient.AccessToken;

        public async Task<bool> GetEndpointsAsync()
        {
            HttpResponseMessage response = await QBOClient.DiscoverEndpointsAsync();
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
            authCodePage = await QBOClient.GetAuthCodesAsync();
            if (!string.IsNullOrEmpty(authCodePage))
            {
                File.WriteAllBytes(@".\GetAuthCode.html", Encoding.ASCII.GetBytes(authCodePage));
                return true;
            }
            return false;
        }

        public async Task<bool> SetAccessTokenAsync(string authCode)
        {
            return await QBOClient.SetAccessTokenAsync(authCode);
        }

        public async Task<bool> RefreshAccessTokenAsync()
        {
            return await QBOClient.RefreshAccessTokenAsync();
        }

        public void ManualAccessToken(QboAccessToken accessToken) => QBOClient.SetTokenManually(accessToken);

        public async Task<HttpResponseMessage> QBOGet(string parameter) => await QBOClient.GetQBOAsync(parameter);

        public async Task<HttpResponseMessage> QBOPost<T>(string parameter, T data) => await QBOClient.PostQBOAsync<T>(parameter, data);

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
