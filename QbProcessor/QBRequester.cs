using System;
using QBXMLRP2Lib;

namespace QBProcessor
{
    /// <summary>QBRequester Class</summary>
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
        internal static string SdkVersion => sdkVers[useVersion];
        internal bool VersionsEOF => useVersion > sdkVers.Length - 1;
        internal static int useVersion { get; set; } = 0;
        #endregion

        #region Public properties
        /// <summary>The application identifier</summary>
        readonly public string AppID = "Invoicing Made Simple";

        /// <summary>Gets the API version that is currently being used for the QBXML request.</summary>
        /// <value>The API version.</value>
        public string ApiVersion => $"v{SdkVersion}";

        /// <summary>Gets the qb session ticket.</summary>
        /// <value>The qb session ticket.</value>
        public string QbSessionTicket { get; private set; }
        #endregion

        /// <summary>Initializes a new instance of the <see cref="QBRequester" /> class.</summary>
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

        /// <summary>Increment the list position of the SdkVers to use as the QBXML version.</summary>
        public void NextVer()
        {
            if (useVersion < sdkVers.Length)
            {
                useVersion += 1;
            }
        }

        /// <summary>Disconnects the request processor from Quickbooks.</summary>
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
        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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

        /// <summary>Finalizes an instance of the <see cref="QBRequester" /> class.</summary>
        ~QBRequester()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}