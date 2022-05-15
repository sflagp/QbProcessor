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
    public class TestPurchaseOrderModels
    {
        readonly string testName = "IMS PurchaseOrder";

        [TestMethod]
        public async Task Step_1_QBOPurchaseOrderQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting PurchaseOrders
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from PurchaseOrder", false);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet PurchaseOrder failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            PurchaseOrderOnlineRs purchaseOrderRs = new(qryRs);
            Assert.IsNull(purchaseOrderRs.ParseError);
            Assert.AreNotEqual(0, purchaseOrderRs.TotalPurchaseOrders);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOPurchaseOrderAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting PurchaseOrders
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from PurchaseOrder"), false);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying PurchaseOrder: {await getRs.Content.ReadAsStringAsync()}");
            
            PurchaseOrderOnlineRs purchaseOrderRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding PurchaseOrder
            if (purchaseOrderRs.PurchaseOrders.Any(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false)) Assert.Inconclusive($"{testName} already exists.");

            HttpResponseMessage vendQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Vendor"));
            if (!vendQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving vendors.\n{await vendQryRq.Content.ReadAsStringAsync()}");
            VendorOnlineRs vendorRs = new(await vendQryRq.Content.ReadAsStringAsync());
            VendorDto vend = vendorRs.Vendors.OrderBy(v => Guid.NewGuid()).FirstOrDefault();

            HttpResponseMessage acctQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Account"));
            if (!acctQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving accounts.\n{await vendQryRq.Content.ReadAsStringAsync()}");
            AccountOnlineRs acctRs = new(await acctQryRq.Content.ReadAsStringAsync());
            AccountDto acct = acctRs.Accounts.OrderBy(a => Guid.NewGuid()).FirstOrDefault(a => a.AccountType == AccountType.AccountsPayable);

            HttpResponseMessage itemQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item"));
            if (!itemQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving items.\n{await itemQryRq.Content.ReadAsStringAsync()}");
            ItemOnlineRs itemRs = new(await itemQryRq.Content.ReadAsStringAsync());
            ItemDto item = itemRs.Items.OrderBy(i => Guid.NewGuid()).FirstOrDefault(i => i.Type == ItemType.Inventory);

            PurchaseOrderAddRq addRq = new();
            addRq.VendorRef = new(vend.Id, vend.FullyQualifiedName);
            addRq.APAccountRef = new(acct.Id, acct.FullyQualifiedName);
            addRq.Line = new() { new()
            {
                DetailType = LineDetailType.ItemBasedExpenseLineDetail,
                Amount = item.UnitPrice * 3,
                LineDetail = new ItemBasedExpenseLineDetailDto() { ItemRef = new(item.Id, item.Name), Qty = 3, UnitPrice = item.UnitPrice },
            }};
            addRq.PrivateNote = testName;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            PurchaseOrderOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(1, addRs.TotalPurchaseOrders);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOPurchaseOrderModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting PurchaseOrder
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from PurchaseOrder"));
            PurchaseOrderOnlineRs purchaseOrderRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating PurchaseOrder
            if (purchaseOrderRs.TotalPurchaseOrders <= 0) Assert.Inconclusive("No PurchaseOrder to update.");

            PurchaseOrderDto purchaseOrder = purchaseOrderRs.PurchaseOrders.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (purchaseOrder == null) Assert.Inconclusive($"{testName} does not exist.");

            HttpResponseMessage itemQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item"));
            if (!itemQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving items.\n{await itemQryRq.Content.ReadAsStringAsync()}");
            ItemOnlineRs itemRs = new(await itemQryRq.Content.ReadAsStringAsync());
            ItemDto item = itemRs.Items.OrderBy(i => Guid.NewGuid()).FirstOrDefault(i => i.Type == ItemType.Inventory);

            PurchaseOrderModRq modRq = new();
            modRq.CopyDto(purchaseOrder);
            modRq.sparse = "true";
            modRq.Line.Add(new() 
            {
                DetailType = LineDetailType.ItemBasedExpenseLineDetail,
                Amount = item.UnitPrice,
                LineDetail = new PurchaseOrderItemLineDetailDto() { ItemRef = new(item.Id, item.Name), Qty = 5 },
            });
            modRq.PrivateNote = $"{testName} => {purchaseOrder.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq, true);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            PurchaseOrderOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(purchaseOrder.PrivateNote, modRs.PurchaseOrders?[0]?.PrivateNote);
            Assert.AreNotEqual(purchaseOrder.MetaData.LastUpdatedTime, modRs.PurchaseOrders[0].MetaData.LastUpdatedTime);
            #endregion
        }

        [TestMethod]
        [Ignore]
        public async Task Step_4_QBOPurchaseOrderEmailTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting PurchaseOrder
            Random rdm = new();
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from PurchaseOrder"));
            PurchaseOrderOnlineRs PurchaseOrderRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Emailing PurchaseOrder
            if (PurchaseOrderRs.TotalPurchaseOrders <= 0) Assert.Inconclusive($"No {testName} to email.");

            PurchaseOrderDto PurchaseOrder = PurchaseOrderRs.PurchaseOrders.FirstOrDefault(cm => cm.PrivateNote?.StartsWith(testName) ?? false);
            if (PurchaseOrder == null) Assert.Inconclusive($"{testName} does not exist.");
            
            HttpResponseMessage postRs = await qboe.QBOPost($"/v3/company/{qboe.ClientInfo.RealmId}/purchaseorder/{PurchaseOrder.Id}/send?sendTo=sfla_gp@yahoo.com");
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            PurchaseOrderOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(PurchaseOrder.PrivateNote, modRs.PurchaseOrders?[0]?.PrivateNote);
            #endregion
        }

        [TestMethod]
        public async Task Step_5_QBOPurchaseOrderPdfTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting PurchaseOrder
            Random rdm = new();
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from PurchaseOrder"));
            PurchaseOrderOnlineRs PurchaseOrderRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Downloading PurchaseOrder
            if (PurchaseOrderRs.TotalPurchaseOrders <= 0) Assert.Inconclusive($"No {testName} to download.");

            PurchaseOrderDto PurchaseOrder = PurchaseOrderRs.PurchaseOrders.FirstOrDefault(cm => cm.PrivateNote?.StartsWith(testName) ?? false);
            if (PurchaseOrder == null) Assert.Inconclusive($"{testName} does not exist.");

            HttpResponseMessage pdfRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/purchaseorder/{PurchaseOrder.Id}/pdf", true);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"PurchaseOrder pdf download failed: {await pdfRs.Content.ReadAsStringAsync()}");

            string modRs = new(await pdfRs.Content.ReadAsStringAsync());
            Assert.AreEqual(".pdf", IMAGE.GetContentType(modRs).FileExtension(), "Webapi result is not a PDF document.");
            #endregion
        }

        [TestMethod]
        public async Task Step_6_QBOPurchaseOrderDeleteTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting PurchaseOrder
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from PurchaseOrder"));
            if (!getRs.IsSuccessStatusCode) Assert.Inconclusive($"Could not retrieve PurchaseOrder to delete: {await getRs.Content.ReadAsStringAsync()}");
            PurchaseOrderOnlineRs PurchaseOrderRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Deleting PurchaseOrder
            if (PurchaseOrderRs.TotalPurchaseOrders <= 0) Assert.Inconclusive($"No {testName} to delete.");

            PurchaseOrderDto PurchaseOrder = PurchaseOrderRs.PurchaseOrders.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (PurchaseOrder == null) Assert.Inconclusive($"{testName} does not exist.");

            DeleteRq delRq = new("PurchaseOrder", PurchaseOrder.Id, PurchaseOrder.SyncToken);
            
            HttpResponseMessage postRs = await qboe.QBOPost(delRq.ApiParameter(qboe.ClientInfo.RealmId), delRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            PurchaseOrderOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(EntityStatus.Deleted, modRs.PurchaseOrders[0].status, $"PurchaseOrder status not Deleted: {modRs.PurchaseOrders[0].status}");
            #endregion
        }
    }
}
