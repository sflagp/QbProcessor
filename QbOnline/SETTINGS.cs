using System;
using System.IO;
using System.Text.Json;

namespace QbModels.QBOProcessor
{
    internal static class Settings
    {
        #region Private Properties
        private static string SettingsFile;
        private static LocalSettings localSettings = new();
        private static ClientInfoDto clientInfo = new();
        #endregion

        #region Readonly Properties
        public static string ClientInfoFile => localSettings.ClientInfoFile;
        public static string IntuitEndpoint => localSettings.IntuitEndpoint;
        public static string DiscoveryUri => localSettings.DiscoveryUri;
        public static bool GetNewAuthCode => localSettings.GetNewAuthCode;
        public static string AuthCode => localSettings.AuthCode;
        public static string AuthScope => localSettings.AuthScope;
        public static string RedirectUri => localSettings.RedirectUri;
        #endregion

        #region Properties
        public static ClientInfoDto ClientInfo => clientInfo;
        public static DiscoveryEndpoints QboDiscoveryEndpoints
        {
            get { return localSettings.QboDiscoveryEndpoints; }
            set { localSettings.QboDiscoveryEndpoints = value; }
        }
        public static QboAccessToken AccessToken
        {
            get { return localSettings.AccessToken; }
            set { localSettings.GetNewAuthCode = default; localSettings.AccessToken = value; }
        }
        #endregion

        #region Methods
        private static string ReadClientInfo()
        {
            if (File.Exists(localSettings.ClientInfoFile))
            {
                return File.ReadAllText(localSettings.ClientInfoFile);
            }
            throw new FileNotFoundException($"File {localSettings.ClientInfoFile} not found.");
        }
        public static void SetSettings(string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName)) fileName = SettingsFile;
            if (File.Exists(fileName))
            {
                SettingsFile = fileName;

                try
                {
                    string settingsInfo = File.ReadAllText(fileName);
                    localSettings = JsonSerializer.Deserialize<LocalSettings>(settingsInfo);
                    return;
                }
                catch (Exception ex)
                {
                    throw new FileLoadException($"Could not load settings file {SettingsFile}\n{ex.Message}");
                }
            }
            throw new FileNotFoundException($"Settings file {fileName} not found");
        }
        public static void SaveSettings()
        {
            try
            {
                if (string.IsNullOrEmpty(SettingsFile)) return;
                string settings = JsonSerializer.Serialize(localSettings, typeof(LocalSettings));
                File.WriteAllText(SettingsFile, settings);
            }
            catch (Exception ex)
            {
                throw new FileLoadException($"Could nto save settings file {SettingsFile}\n{ex.Message}");
            }
        }
        public static void SetClientInfo(string settingsFile)
        {
            SetSettings(settingsFile);
            string info = ReadClientInfo();
            clientInfo.SetClientInfo(JsonSerializer.Deserialize<ClientInfoDto>(info));
        }
        #endregion

        #region Private Settings class
#pragma warning disable S3218 // Inner class members should not shadow outer class "static" or type members
#pragma warning disable S3459 // Unassigned members should be removed
        [Serializable]
        private sealed class LocalSettings
        {
            public string ClientInfoFile { get; set; }
            public string IntuitEndpoint { get; set; }
            public string DiscoveryUri { get; set; }
            public DiscoveryEndpoints QboDiscoveryEndpoints { get; set; }
            public bool GetNewAuthCode { get; set; }
            public string AuthCode { get; set; }
            public string AuthScope { get; set; }
            public string RedirectUri { get; set; }
            public QboAccessToken AccessToken { get; set; }
        }
#pragma warning restore S3459 // Unassigned members should be removed
#pragma warning restore S3218 // Inner class members should not shadow outer class "static" or type members
        #endregion
    }
}
