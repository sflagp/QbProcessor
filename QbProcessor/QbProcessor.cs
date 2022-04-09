using System;
using System.Xml;
using System.Data;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using QbHelpers;
using QbModels;

namespace QBProcessor
{
    public partial class QbProcessor : QBRequester
    {
        #region Private Variables and Properties
        private QbCompanyView QbCompany;
        #endregion

        #region Public Variables and Properties
        public event EventHandler<string> OnRequestEvent;
        public DateTime LicenseExpires => new DateTime(2022, 12, 31);
        public bool LicenseValid => DateTime.Today <= LicenseExpires;
        public string CompanyName => QbCompany?.Company?.CompanyName;
        #endregion

        #region Constructors and QB Connection
#pragma warning disable S112 // General exceptions should never be thrown
        #region Constructor
        public QbProcessor()
        {
            if (SessionActive)
            {
                if (!InitQBCompany())
                {
                    throw new Exception("Quickbooks not compatible with Invoicing Made Simple");
                }
            }
            else
            {
                throw new Exception(StartStatus.ToString());
            }
        }
        #endregion

#pragma warning restore S112 // General exceptions should never be thrown

        private bool InitQBCompany()
        {
            bool bolRequestComplete = false;

            do
            {
                try
                {
                    var xmlCompany = GetCompany(); 
                    if(xmlCompany != "The version of QBXML that was requested is not supported or is unknown.")
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
        #endregion

        #region Cleanup

        ~QbProcessor()
        {
            DisconnectQB();
        }
        #endregion

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
        #endregion

        #region Account Methods and Properites
        // Look for clsQBAccount in Partials folder
        #endregion

        #region Customer Methods and Properties
        // Look for clsQBCustomer in Partials folder
        #endregion

        #region Vendor Methods and Properties
        // Look for clsQBVendor in Partials folder
        #endregion

        #region Invoice Methods and Properties
        // Look for clsQBInvoice in Partials folder
        #endregion

        #region Check Methods and Properties
        // Look for clsQBCheck in Partials folder
        #endregion

        #region Bill Payment Methods and Properties
        // Look for clsQBBillPayment in Partials folder
        #endregion

        #region Item Methods and Properties
        // Look for clsQBItem in Partials folder
        #endregion

        #region Time and Journal Methods and Properties
        // Look for clsQBTimeAndJournal in Partials folder
        #endregion

        #region Employees Methods and Properties
        // Look for clsQBEmployee in Partials folder
        #endregion

        #region Request Builders
        // Go to QbRequestHelpers.cs to edit/view
        #endregion

        #region Test Methods for creating XSD files
        internal void TestFunction(string strFunction)
        {
            XmlDocument requestXmlDoc = new XmlDocument();
            string xmlTest = null;
            string xmlResponse = null;
            //QBFunctions qbTest = new QBFunctions(strFunction);

            requestXmlDoc.BuildQbRequest(strFunction);
            
            //xmlTest = qbTest.functionXML(requestXmlDoc);

            xmlResponse = CallQB(xmlTest);
            //qbTest.readResponse(xmlResponse);

            var tmp = new DataSet();
            tmp.ReadXml(new MemoryStream(Encoding.ASCII.GetBytes(xmlResponse)));
            try
            {
                tmp.WriteXmlSchema($"{strFunction}.xsd");
            }
            catch(Exception ex)
            {
                System.IO.File.WriteAllBytes($"{strFunction}.err", Encoding.ASCII.GetBytes($"{strFunction} error: {ex.Message}"));
            }
        }

        public void testFunction(string strFunction, string nameRangeFilter, string fromName, string toName)
        {
            var requestXmlDoc = new XmlDocument();
            string xmlTest = null;
            string xmlResponse = null;
            //QBFunctions qbTest = new QBFunctions(strFunction);

            requestXmlDoc.BuildQbRequest(strFunction);

            xmlTest = CallQB(requestXmlDoc.OuterXml);

            xmlResponse = CallQB(xmlTest);
            //qbTest.readResponse(xmlResponse);
        }
        #endregion
    }
}