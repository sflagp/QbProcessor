using System;
using QBXMLRP2Lib;

namespace QBProcessor
{
    public abstract partial class QBRequester : IDisposable
    {
        #region Private properties
        private const int SupportQBSimpleStart = 1;
        private const int SupportQBPro = 2;
        private const int SupportQBPremier = 4;
        private const int SupportQBEnterprise = 8;
        private const int ForceAuthDialog = 80000000;
        private static string[] sdkVers = { "15.0", "14.0", "13.0", "12.0", "11.0", "10.0", "9.0", "8.0", "7.0", "6.0" };
        readonly private AuthPreferences rpPrefs;
        readonly private bool sessionBegun = false;
        readonly private string sessionStartStatus;
        private bool disposedValue;
        #endregion

        #region Internal Properties
        internal RequestProcessor2 rp;
        internal bool SessionActive => sessionBegun;
        internal string StartStatus => sessionStartStatus;
        public string ApiVersion => $"v{SdkVersion}";
        internal static string SdkVersion => sdkVers[useVersion];
        internal bool VersionsEOF => useVersion > sdkVers.Length - 1;
        internal static int useVersion { get; set; } = 0;
        #endregion

        #region Public properties
        readonly public string AppID = "Invoicing Made Simple";
        public string QbSessionTicket { get; private set; }
        #endregion

        protected QBRequester()
        {
            int authFlags = 0;
            sessionBegun = false;

            //Create the Request Processor object
            rp = new RequestProcessor2();
            //Connect to QuickBooks and begin a session
            rp.OpenConnection2("", AppID, QBXMLRPConnectionType.localQBDLaunchUI);
            authFlags |= SupportQBEnterprise;
            authFlags |= SupportQBPremier;
            authFlags |= SupportQBPro;
            authFlags |= SupportQBSimpleStart;
            rpPrefs = (AuthPreferences)rp.AuthPreferences;
            rpPrefs.PutAuthFlags(authFlags);
            try
            {
                QbSessionTicket = rp.BeginSession("", QBFileMode.qbFileOpenDoNotCare);
                sessionBegun = true;
                sessionStartStatus = QbSessionTicket;
            }
            catch (Exception ex)
            {
                sessionBegun = false;
                sessionStartStatus = ex.Message.ToString();
                rp.CloseConnection();
            }
        }

        public void NextVer()
        {
            if (useVersion < sdkVers.Length)
            {
                useVersion += 1;
            }
        }

        public void DisconnectQB()
        {
            if (SessionActive && !string.IsNullOrEmpty(QbSessionTicket))
            {
                try
                {
                    System.Threading.Thread.Sleep(1500);
                    rp.EndSession(QbSessionTicket);
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(1500);
                    rp.EndSession(QbSessionTicket);
                }
                rp.CloseConnection();
            }
            QbSessionTicket = null;
            rp = null;
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                DisconnectQB();
                disposedValue = true;
            }
        }

        ~QBRequester()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}