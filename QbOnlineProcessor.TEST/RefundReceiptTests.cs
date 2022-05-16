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
    public class TestRefundReceiptModels
    {
        readonly string testName = "IMS RefundReceipt";

        [TestMethod]
        public async Task Step_1_QBORefundReceiptQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting RefundReceipts
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from RefundReceipt");
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet RefundReceipt failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            RefundReceiptOnlineRs refundReceiptRs = new(qryRs);
            Assert.IsNull(refundReceiptRs.ParseError);
            Assert.AreNotEqual(0, refundReceiptRs.TotalRefundReceipts);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBORefundReceiptAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting RefundReceipts
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from RefundReceipt"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying RefundReceipt: {await getRs.Content.ReadAsStringAsync()}");
            RefundReceiptOnlineRs refundRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding RefundReceipt
            if (refundRs.RefundReceipts.Any(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false)) Assert.Inconclusive($"{testName} already exists.");

            Random rdm = new();
            HttpResponseMessage acctQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Account"));
            if (!acctQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving accounts.\n{await acctQryRq.Content.ReadAsStringAsync()}");
            AccountOnlineRs acctRs = new(await acctQryRq.Content.ReadAsStringAsync());
            AccountDto acct = acctRs.Accounts.OrderBy(a => Guid.NewGuid()).FirstOrDefault(a => a.AccountType == AccountType.Bank || a.AccountType == AccountType.OtherCurrentAsset);

            HttpResponseMessage itemQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item where Type = 'Service'"));
            if (!itemQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving items.\n{await itemQryRq.Content.ReadAsStringAsync()}");
            ItemOnlineRs itemRs = new(await itemQryRq.Content.ReadAsStringAsync());
            ItemDto item = itemRs.Items.ElementAt(rdm.Next(0, itemRs.TotalItems));

            RefundReceiptAddRq addRq = new();
            addRq.DepositToAccountRef = new(acct.Id, acct.FullyQualifiedName);
            addRq.Line = new() { new()
            {
                DetailType = LineDetailType.SalesItemLineDetail,
                Amount = 12.35M,
                LineDetail = new SalesItemLineDetailDto() { ItemRef = new(item.Id, item.Name) },
            }};
            addRq.PrivateNote = testName;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            RefundReceiptOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(1, addRs.TotalRefundReceipts);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBORefundReceiptModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting RefundReceipt
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from RefundReceipt"));
            RefundReceiptOnlineRs RefundReceiptRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating RefundReceipt
            if (RefundReceiptRs.TotalRefundReceipts <= 0) Assert.Inconclusive("No RefundReceipt to update.");

            RefundReceiptDto refundReceipt = RefundReceiptRs.RefundReceipts.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (refundReceipt == null) Assert.Inconclusive($"{testName} does not exist.");

            HttpResponseMessage itemQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item where Type = 'Inventory'"));
            if (!itemQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving items.\n{await itemQryRq.Content.ReadAsStringAsync()}");
            ItemOnlineRs itemRs = new(await itemQryRq.Content.ReadAsStringAsync());
            ItemDto item = itemRs.Items.OrderBy(i => Guid.NewGuid()).FirstOrDefault();

            RefundReceiptModRq modRq = new();
            modRq.CopyDto(refundReceipt);
            modRq.sparse = "true";
            modRq.Line.Add(new() 
            {
                DetailType = LineDetailType.SalesItemLineDetail,
                Amount = item.UnitPrice,
                LineDetail = new SalesItemLineDetailDto() { ItemRef = new(item.Id, item.Name), Qty = 5 },
            });
            modRq.PrivateNote = $"{testName} => {refundReceipt.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            RefundReceiptOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(refundReceipt.PrivateNote, modRs.RefundReceipts?[0]?.PrivateNote);
            Assert.AreNotEqual(refundReceipt.MetaData.LastUpdatedTime, modRs.RefundReceipts[0].MetaData.LastUpdatedTime);
            #endregion
        }

        [TestMethod]
        [Ignore]
        public async Task Step_4_QBORefundReceiptEmailTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting RefundReceipt
            Random rdm = new();
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from RefundReceipt"));
            RefundReceiptOnlineRs RefundReceiptRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Emailing RefundReceipt
            if (RefundReceiptRs.TotalRefundReceipts <= 0) Assert.Inconclusive($"No {testName} to email.");
            RefundReceiptDto RefundReceipt = RefundReceiptRs.RefundReceipts.FirstOrDefault(cm => cm.PrivateNote?.StartsWith(testName) ?? false);
            if (RefundReceipt == null) Assert.Inconclusive($"{testName} does not exist.");
            HttpResponseMessage postRs = await qboe.QBOPost($"/v3/company/{qboe.ClientInfo.RealmId}/refundreceipt/{RefundReceipt.Id}/send?sendTo=sfla_gp@yahoo.com");
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            RefundReceiptOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(RefundReceipt.PrivateNote, modRs.RefundReceipts?[0]?.PrivateNote);
            #endregion
        }

        [TestMethod]
        public async Task Step_5_QBORefundReceiptPdfTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting RefundReceipt
            Random rdm = new();
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from RefundReceipt"));
            
            RefundReceiptOnlineRs RefundReceiptRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Downloading RefundReceipt
            if (RefundReceiptRs.TotalRefundReceipts <= 0) Assert.Inconclusive($"No {testName} to download.");

            RefundReceiptDto refundReceipt = RefundReceiptRs.RefundReceipts.FirstOrDefault(cm => cm.PrivateNote?.StartsWith(testName) ?? false);
            if (refundReceipt == null) Assert.Inconclusive($"{testName} does not exist.");

            HttpResponseMessage pdfRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/refundreceipt/{refundReceipt.Id}/pdf", true);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"RefundReceipt pdf download failed: {await pdfRs.Content.ReadAsStringAsync()}");

            string modRs = new(await pdfRs.Content.ReadAsStringAsync());
            Assert.AreEqual(".pdf", IMAGE.GetContentType(modRs).FileExtension(), "Webapi result is not a PDF document.");
            #endregion
        }

        [TestMethod]
        public async Task Step_6_QBORefundReceiptDeleteTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting RefundReceipt
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from RefundReceipt"));
            if (!getRs.IsSuccessStatusCode) Assert.Inconclusive($"Could not retrieve RefundReceipt to delete: {await getRs.Content.ReadAsStringAsync()}");
            
            RefundReceiptOnlineRs refundReceiptRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Deleting RefundReceipt
            if (refundReceiptRs.TotalRefundReceipts <= 0) Assert.Inconclusive($"No {testName} to delete.");

            RefundReceiptDto refundReceipt = refundReceiptRs.RefundReceipts.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (refundReceipt == null) Assert.Inconclusive($"{testName} does not exist.");

            DeleteRq delRq = new("RefundReceipt", refundReceipt.Id, refundReceipt.SyncToken);
            
            HttpResponseMessage postRs = await qboe.QBOPost(delRq.ApiParameter(qboe.ClientInfo.RealmId), delRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            RefundReceiptOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(EntityStatus.Deleted, modRs.RefundReceipts[0].status, $"RefundReceipt status not Deleted: {modRs.RefundReceipts[0].status}");
            #endregion
        }
    }
}
