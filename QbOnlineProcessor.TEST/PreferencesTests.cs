using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestPreferencesModels
    {
        [TestMethod]
        public async Task Step_1_QBOPreferencesQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Preferences info
            Assert.IsFalse(string.IsNullOrEmpty(qboe.AccessToken.AccessToken));

            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from Preferences", true);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            PreferencesOnlineRs Preferences = new(qryRs);
            if (!string.IsNullOrEmpty(Preferences.ParseError)) Assert.Fail($"ParseError: {Preferences.ParseError}");
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOPreferencesModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Updating Preferences
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("AccessToken not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from Preferences", true);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            PreferencesOnlineRs preferences = new(qryRs);

            HttpResponseMessage cmpyRq = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/companyinfo/{qboe.ClientInfo.RealmId}");
            CompanyOnlineRs cmpyRs = new(await cmpyRq.Content.ReadAsStringAsync());
            CompanyInfoDto company = cmpyRs.CompanyInfo;

            string msgEstimate = $"Please review the estimate below.  Feel free to contact us if you have any questions.\nWe look forward to working with you.\n\nSincerely,\n{company.CompanyName}";
            string subEstimate = $"Estimate from {company.CompanyName}";
            string msgInvoice = $"Your invoice is attached. Please remit payment at your earliest convenience.\nThank you for your business - we appreciate it very much.\n\nSincerely,\n{company.CompanyName}";
            string subInvoice = $"Invoice from {company.CompanyName}";
            string msgSalesReceipt = $"Here is your sales receipt.  Please keep in your records for future reference.\nThank you for your business - we appreciate it very much.\n\nSincerely,\n{company.CompanyName}";
            string subSalesReceipt = $"Sales receipt from {company.CompanyName}";
            string msgStatement = $"Please review the statement below.  Feel free to contact us if you have any questions.\nWe look forward to working with you.\n\nSincerely,\n{company.CompanyName}";
            string subStatement = $"Statement from {company.CompanyName}";

            PreferencesModRq modRq = new();
            modRq.CopyDto(preferences.Preferences, "MetaData");
            modRq.sparse = "true";
            modRq.EmailMessagesPrefs.EstimateMessage.Message = msgEstimate;
            modRq.EmailMessagesPrefs.EstimateMessage.Subject = subEstimate;
            modRq.EmailMessagesPrefs.InvoiceMessage.Message = msgInvoice;
            modRq.EmailMessagesPrefs.InvoiceMessage.Subject = subInvoice;
            modRq.EmailMessagesPrefs.SalesReceiptMessage.Message = msgSalesReceipt;
            modRq.EmailMessagesPrefs.SalesReceiptMessage.Subject = subSalesReceipt;
            modRq.EmailMessagesPrefs.StatementMessage.Message = msgStatement;
            modRq.EmailMessagesPrefs.StatementMessage.Subject = subStatement;

            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq, true);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");
            
            PreferencesOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(msgEstimate, modRq.EmailMessagesPrefs.EstimateMessage.Message);
            Assert.AreEqual(subEstimate, modRq.EmailMessagesPrefs.EstimateMessage.Subject);
            Assert.AreEqual(msgInvoice, modRq.EmailMessagesPrefs.InvoiceMessage.Message);
            Assert.AreEqual(subInvoice, modRq.EmailMessagesPrefs.InvoiceMessage.Subject);
            Assert.AreEqual(msgSalesReceipt, modRq.EmailMessagesPrefs.SalesReceiptMessage.Message);
            Assert.AreEqual(subSalesReceipt, modRq.EmailMessagesPrefs.SalesReceiptMessage.Subject);
            Assert.AreEqual(msgStatement, modRq.EmailMessagesPrefs.StatementMessage.Message);
            Assert.AreEqual(subStatement, modRq.EmailMessagesPrefs.StatementMessage.Subject);
            #endregion
        }
    }
}
