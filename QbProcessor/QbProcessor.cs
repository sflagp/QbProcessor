using System;
using System.Threading;
using System.Threading.Tasks;
using QbHelpers;

namespace QbModels.QbProcessor
{
    public partial class RequestProcessor : QBRequester
    {
        #region Private Variables and Properties

        private CompanyRs QbCompany;

        #endregion Private Variables and Properties

        #region Public Variables and Properties
        /// <summary>Occurs when [on request event].</summary>
        public event EventHandler<string> OnRequestEvent;

        /// <summary>Gets the date the license expires.</summary>
        /// <value>The license expiration date.</value>
        public DateTime LicenseExpires => new DateTime(2024, 12, 31);

        /// <summary>Gets a value indicating whether [license valid].</summary>
        /// <value>
        ///   <c>true</c> if [license valid]; otherwise, <c>false</c>.</value>
        public bool LicenseValid => DateTime.Today <= LicenseExpires;

        /// <summary>Gets the name of the Quickbooks company.</summary>
        /// <value>The name of the Quickbooks company.</value>
        public string CompanyName => QbCompany?.Company?.CompanyName;
        #endregion Public Variables and Properties

        #region Constructors and QB Connection
        /// <summary>Initializes a new instance of the <see cref="RequestProcessor" /> class.</summary>
        /// <exception cref="System.Exception">Quickbooks not compatible with Invoicing Made Simple
        /// or</exception>
        public RequestProcessor()
        {
            if (SessionActive)
            {
                if (!InitQBCompany())
                {
                    throw new NotImplementedException("Quickbooks not compatible with Invoicing Made Simple");
                }
            }
            else
            {
                throw new NotSupportedException(StartStatus.ToString());
            }
        }

        private bool InitQBCompany()
        {
            bool bolRequestComplete = false;

            do
            {
                try
                {
                    var xmlCompany = GetCompany();
                    if (xmlCompany != "The version of QBXML that was requested is not supported or is unknown.")
                    {
                        QbCompany = new(xmlCompany);
                        bolRequestComplete = true;
                    }
                    else
                    {
                        NextVer();
                        XmlHelper.SetSdkVersion(SdkVersion);
                    }
                }
                catch (Exception)
                {
                    NextVer();
                    XmlHelper.SetSdkVersion(SdkVersion);
                }
            } while (!(bolRequestComplete || VersionsEOF));

            return bolRequestComplete;
        }
        private string GetCompany() => QbObjectProcessor(new CompanyQueryRq(), Guid.NewGuid());
        #endregion Constructors and QB Connection

        #region Cleanup

        /// <summary>Finalizes an instance of the <see cref="QBRequester" /> class.</summary>
        ~RequestProcessor()
        {
            DisconnectQB();
        }

        #endregion Cleanup

        #region QB Requests Methods
        /// <summary>Execute QB request and return result</summary>
        /// <param name="xmlRequest"></param>
        /// <returns>XML string</returns>
        internal string CallQB(string xmlRequest)
        {
            if (!LicenseValid) return "License expired";
            var rpResponse = rp.ProcessRequest(QbSessionTicket, xmlRequest);

            return rpResponse;
        }

        internal delegate string QbRequestAsync(string ticket, string xml);

        /// <summary>Execute QB request and return result asyncronously</summary>
        /// <param name="xmlRequest"></param>
        /// <returns>XML string</returns>
        internal async Task<string> CallQBAsync(string xmlRequest)
        {
            var rpResponse = default(string);
            rpResponse = await Task<string>.Run(() =>
            {
                return rp.ProcessRequest(QbSessionTicket, xmlRequest);
            });
            return rpResponse;
        }

        /// <summary>Invokes the OnRequestEvent to fire event for consuming application.</summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="requesterId">The requester identifier.  If a requester identifier is passed in, it will include it on the response.</param>
        /// <param name="requestType">Quickbooks processing type of the request.</param>
        /// <param name="eventResponse">The data to send to the consuming application.</param>
        /// <param name="qbRequest">The XML string sent to the Quickbooks processing system.</param>
        protected void InvokeRequestEvent(object sender, Guid requesterId, string requestType, string eventResponse, string qbRequest = "")
        {
            OnRequestEvent?.Invoke(new RequestEventReplySender(sender, requesterId, requestType, qbRequest, eventResponse), eventResponse);
        }
        #endregion QB Requests Methods
    }
}