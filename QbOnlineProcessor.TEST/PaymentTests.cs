using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using QbModels.QBO.ENUM;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestPaymentModels
    {
        readonly string testName = "IMS Payment";

        [TestMethod]
        public async Task Step_1_QBOPaymentQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Payments
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from Payment", false);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            PaymentOnlineRs paymentRs = new(qryRs);
            Assert.IsNull(paymentRs.ParseError);
            Assert.AreNotEqual(0, paymentRs.TotalPayments);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOPaymentAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Payments
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Payment"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying Payment: {await getRs.Content.ReadAsStringAsync()}");
            
            PaymentOnlineRs qryRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding Payment
            if (qryRs.Payments.Any(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false)) Assert.Inconclusive($"{testName} already exists.");

            HttpResponseMessage pmtMethodQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from PaymentMethod"));
            if (!pmtMethodQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving PaymentMethod.\n{await pmtMethodQryRq.Content.ReadAsStringAsync()}");
            PaymentMethodOnlineRs pmtMethodRs = new(await pmtMethodQryRq.Content.ReadAsStringAsync());
            PaymentMethodDto pmtMethod = pmtMethodRs.PaymentMethods.OrderBy(p => Guid.NewGuid()).FirstOrDefault();

            HttpResponseMessage invQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Invoice"));
            if (!invQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving Invoice.\n{await invQryRq.Content.ReadAsStringAsync()}");
            InvoiceOnlineRs invRs = new(await invQryRq.Content.ReadAsStringAsync());
            InvoiceDto inv = invRs.Invoices.OrderBy(c => Guid.NewGuid()).FirstOrDefault();

            PaymentAddRq addRq = new();
            addRq.CustomerRef = new(inv.CustomerRef.Value, inv.CustomerRef.name);
            addRq.PaymentMethodRef = new(pmtMethod.Id);
            addRq.TotalAmt = inv.TotalAmt;
            addRq.Line = new()
            {
                new() { Amount = inv.TotalAmt, LinkedTxn = new() { new(inv.Id, "Invoice") } }
            };
            addRq.PrivateNote = testName;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            PaymentOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(1, addRs.TotalPayments);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOPaymentModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Payment
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Payment"));
            PaymentOnlineRs qryRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating Payment
            if (qryRs.TotalPayments <= 0) Assert.Fail($"{testName} to update.");

            PaymentDto pmt = qryRs.Payments.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (pmt == null) Assert.Inconclusive($"{testName} does not exist.");
            
            PaymentModRq modRq = new();
            modRq.CopyDto(pmt);
            modRq.sparse = "true";
            modRq.PrivateNote = $"{testName} => {pmt.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            PaymentOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(pmt.PrivateNote, modRs.Payments?[0]?.PrivateNote);
            #endregion
        }

        [TestMethod]
        [Ignore]
        public async Task Step_4_QBOPaymentEmailTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Payment
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Payment"));
            PaymentOnlineRs qryRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Emailing Payment
            if (qryRs.TotalPayments <= 0) Assert.Inconclusive($"No {testName} to email.");

            PaymentDto pmt = qryRs.Payments.FirstOrDefault(cm => cm.PrivateNote?.StartsWith(testName) ?? false);
            if (pmt == null) Assert.Inconclusive($"{testName} does not exist.");

            HttpResponseMessage postRs = await qboe.QBOPost($"/v3/company/{qboe.ClientInfo.RealmId}/payment/{pmt.Id}/send?sendTo=sfla_gp@yahoo.com");
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            PaymentOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(pmt.PrivateNote, modRs.Payments?[0]?.PrivateNote);
            #endregion
        }

        [TestMethod]
        public async Task Step_5_QBOPaymentPdfTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Payment
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Payment"));
            PaymentOnlineRs pmtRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Downloading Payment
            if (pmtRs.TotalPayments <= 0) Assert.Inconclusive($"No {testName} to download.");

            PaymentDto payment = pmtRs.Payments.FirstOrDefault(cm => cm.PrivateNote?.StartsWith(testName) ?? false);
            if (payment == null) Assert.Inconclusive($"{testName} does not exist.");

            HttpResponseMessage pdfRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/payment/{payment.Id}/pdf", true);
            if (!pdfRs.IsSuccessStatusCode) Assert.Fail($"Payment pdf download failed: {await pdfRs.Content.ReadAsStringAsync()}");

            string modRs = new(await pdfRs.Content.ReadAsStringAsync());
            Assert.AreEqual(".pdf", IMAGE.GetContentType(modRs).FileExtension(), "Webapi result is not a PDF document.");
            #endregion
        }

        [TestMethod]
        public async Task Step_6_QBOPaymentDeleteTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Payment
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Payment"));
            if (!getRs.IsSuccessStatusCode) Assert.Inconclusive($"Could not retrieve bill payment to delete: {await getRs.Content.ReadAsStringAsync()}");

            PaymentOnlineRs qryRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Deleting Payment
            if (qryRs.TotalPayments <= 0) Assert.Inconclusive($"No {testName} to delete.");

            PaymentDto billPmt = qryRs.Payments.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (billPmt == null) Assert.Fail($"{testName} does not exist.");
            
            DeleteRq delRq = new("Payment", billPmt.Id, billPmt.SyncToken);
            
            HttpResponseMessage postRs = await qboe.QBOPost(delRq.ApiParameter(qboe.ClientInfo.RealmId), delRq, false);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            PaymentOnlineRs delRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(EntityStatus.Deleted, delRs.Payments[0].status, $"Bill status not Deleted: {delRs.Payments[0].status}");
            #endregion
        }
    }
}
