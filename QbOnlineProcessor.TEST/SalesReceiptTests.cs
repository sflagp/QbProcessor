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
    public class TestSalesReceiptModels
    {
        readonly string testName = "IMS SalesReceipt";
        private QBOProcessor qboe;

        [TestInitialize]
        public async Task InitializeTest()
        {
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();

            qboe = new();
        }

        [TestCleanup]
        public Task CleanupTest()
        {
            qboe.Dispose();
            return Task.CompletedTask;
        }

        [TestMethod]
        public async Task Step_1_QBOSalesReceiptQueryTest()
        {
            #region Getting SalesReceipts
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from SalesReceipt");
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet SalesReceipt failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            SalesReceiptOnlineRs salesReceiptRs = new(qryRs);
            Assert.IsNull(salesReceiptRs.ParseError);
            Assert.AreNotEqual(0, salesReceiptRs.TotalSalesReceipts);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOSalesReceiptAddTest()
        {
            #region Getting SalesReceipts
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from SalesReceipt"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying SalesReceipt: {await getRs.Content.ReadAsStringAsync()}");
            
            SalesReceiptOnlineRs salesReceiptRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding SalesReceipt
            if (salesReceiptRs.SalesReceipts.Any(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false)) Assert.Inconclusive($"{testName} already exists.");

            HttpResponseMessage custQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Customer"));
            if (!custQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving customers.\n{await custQryRq.Content.ReadAsStringAsync()}");
            CustomerOnlineRs custRs = new(await custQryRq.Content.ReadAsStringAsync());
            CustomerDto cust = custRs.Customers.OrderBy(c => Guid.NewGuid()).FirstOrDefault();

            HttpResponseMessage itemQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item where Type = 'Service'"));
            if (!itemQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving items.\n{await itemQryRq.Content.ReadAsStringAsync()}");
            ItemOnlineRs itemRs = new(await itemQryRq.Content.ReadAsStringAsync());
            ItemDto item = itemRs.Items.OrderBy(i => Guid.NewGuid()).FirstOrDefault();

            SalesReceiptAddRq addRq = new();
            addRq.CustomerRef = new(cust.Id, cust.FullyQualifiedName);
            addRq.Line = new() { new()
            {
                DetailType = LineDetailType.SalesItemLineDetail,
                Amount = item.UnitPrice * 3,
                LineDetail = new SalesItemLineDetailDto() { ItemRef = new(item.Id, item.Name), Qty = 3, UnitPrice = item.UnitPrice },
            }};
            addRq.PrivateNote = testName;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            SalesReceiptOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(1, addRs.TotalSalesReceipts);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOSalesReceiptModTest()
        {
            #region Getting SalesReceipt
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from SalesReceipt"));

            SalesReceiptOnlineRs salesReceiptRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating SalesReceipt
            if (salesReceiptRs.TotalSalesReceipts <= 0) Assert.Inconclusive("No SalesReceipt to update.");

            SalesReceiptDto salesReceipt = salesReceiptRs.SalesReceipts.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (salesReceipt == null) Assert.Inconclusive($"{testName} does not exist.");

            HttpResponseMessage itemQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item where Type = 'Inventory'"));
            if (!itemQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving items.\n{await itemQryRq.Content.ReadAsStringAsync()}");
            
            ItemOnlineRs itemRs = new(await itemQryRq.Content.ReadAsStringAsync());
            ItemDto item = itemRs.Items.OrderBy(i => Guid.NewGuid()).FirstOrDefault();

            SalesReceiptModRq modRq = new();
            modRq.CopyDto(salesReceipt);
            modRq.sparse = "true";
            modRq.Line.Add(new() 
            {
                DetailType = LineDetailType.SalesItemLineDetail,
                Amount = item.UnitPrice,
                LineDetail = new SalesItemLineDetailDto() { ItemRef = new(item.Id, item.Name), Qty = 5 },
            });
            modRq.PrivateNote = $"{testName} => {salesReceipt.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            SalesReceiptOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(salesReceipt.PrivateNote, modRs.SalesReceipts?[0]?.PrivateNote);
            Assert.AreNotEqual(salesReceipt.MetaData.LastUpdatedTime, modRs.SalesReceipts[0].MetaData.LastUpdatedTime);
            #endregion
        }

        [TestMethod]
        [Ignore]
        public async Task Step_4_QBOSalesReceiptEmailTest()
        {
            #region Getting SalesReceipt
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from SalesReceipt"));

            SalesReceiptOnlineRs SalesReceiptRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Emailing SalesReceipt
            if (SalesReceiptRs.TotalSalesReceipts <= 0) Assert.Inconclusive($"No {testName} to email.");

            SalesReceiptDto salesReceipt = SalesReceiptRs.SalesReceipts.FirstOrDefault(cm => cm.PrivateNote?.StartsWith(testName) ?? false);
            if (salesReceipt == null) Assert.Inconclusive($"{testName} does not exist.");
            
            HttpResponseMessage postRs = await qboe.QBOPost($"/v3/company/{qboe.ClientInfo.RealmId}/salesreceipt/{salesReceipt.Id}/send?sendTo=sfla_gp@yahoo.com");
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            SalesReceiptOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(salesReceipt.PrivateNote, modRs.SalesReceipts?[0]?.PrivateNote);
            #endregion
        }

        [TestMethod]
        public async Task Step_5_QBOSalesReceiptPdfTest()
        {
            #region Getting SalesReceipt
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from SalesReceipt"));

            SalesReceiptOnlineRs salesReceiptRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Downloading SalesReceipt
            if (salesReceiptRs.TotalSalesReceipts <= 0) Assert.Inconclusive($"No {testName} to download.");

            SalesReceiptDto SalesReceipt = salesReceiptRs.SalesReceipts.FirstOrDefault(cm => cm.PrivateNote?.StartsWith(testName) ?? false);
            if (SalesReceipt == null) Assert.Inconclusive($"{testName} does not exist.");

            HttpResponseMessage pdfRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/salesreceipt/{SalesReceipt.Id}/pdf", true);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"SalesReceipt pdf download failed: {await pdfRs.Content.ReadAsStringAsync()}");

            string modRs = new(await pdfRs.Content.ReadAsStringAsync());
            Assert.AreEqual(".pdf", IMAGE.GetContentType(modRs).FileExtension(), "Webapi result is not a PDF document.");
            #endregion
        }

        [TestMethod]
        public async Task Step_6_QBOSalesReceiptDeleteTest()
        {
            #region Getting SalesReceipt
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from SalesReceipt"));
            if (!getRs.IsSuccessStatusCode) Assert.Inconclusive($"Could not retrieve SalesReceipt to delete: {await getRs.Content.ReadAsStringAsync()}");
            
            SalesReceiptOnlineRs salesReceiptRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Deleting SalesReceipt
            if (salesReceiptRs.TotalSalesReceipts <= 0) Assert.Inconclusive($"No {testName} to delete.");

            SalesReceiptDto salesReceipt = salesReceiptRs.SalesReceipts.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (salesReceipt == null) Assert.Inconclusive($"{testName} does not exist.");

            DeleteRq delRq = new("SalesReceipt", salesReceipt.Id, salesReceipt.SyncToken);
            
            HttpResponseMessage postRs = await qboe.QBOPost(delRq.ApiParameter(qboe.ClientInfo.RealmId), delRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            SalesReceiptOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(EntityStatus.Deleted, modRs.SalesReceipts[0].status, $"SalesReceipt status not Deleted: {modRs.SalesReceipts[0].status}");
            #endregion
        }
    }
}
