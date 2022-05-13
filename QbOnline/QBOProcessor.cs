using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor
{
    public class QBOProcessor : IDisposable
    {
        public QBOProcessor() { }

        private bool disposedValue;

        public ClientInfoDto ClientInfo => Settings.ClientInfo;
        public static void SetClientInfo(string settingsFile = @".\appsettings.QbProcessor.QBO.json") => Settings.SetClientInfo(settingsFile);

        public QboAccessToken AccessToken => Settings.AccessToken;
        public string RedirectUri => Settings.RedirectUri;

        public async Task<bool> GetEndpointsAsync()
        {
            HttpResponseMessage response = await QBOClient.DiscoverEndpointsAsync();
            if (response.IsSuccessStatusCode)
            {
                DiscoveryEndpoints endpoints = await JsonSerializer.DeserializeAsync<DiscoveryEndpoints>(await response.Content.ReadAsStreamAsync());
                Settings.QboDiscoveryEndpoints = endpoints;
                Settings.SaveSettings();
                return true;
            }
            return false;
        }

        public async Task<string> GetAuthCodesAsync() => await QBOClient.GetAuthCodesAsync();

        public async Task<bool> SetAccessTokenAsync(string authCode) => await QBOClient.SetAccessTokenAsync(authCode);

        public async Task<bool> RefreshAccessTokenAsync() => await QBOClient.RefreshAccessTokenAsync();

        public async Task<HttpResponseMessage> QBOGet(string parameter, bool asXml = false) => await QBOClient.GetQBOAsync(parameter, asXml);

        public async Task<HttpResponseMessage> QBOPost<T>(string parameter, T data, bool asXml = false) where T : QBO.IQbRq => await QBOClient.PostQBOAsync<T>(parameter, data, asXml);

        public async Task<HttpResponseMessage> QBOPost(string parameter) => await QBOClient.PostQBOAsync(parameter);

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
