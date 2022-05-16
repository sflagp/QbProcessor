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
    public class TestVendorCreditModels
    {
        readonly string testName = "IMS VendorCredit";

        [TestMethod]
        public async Task Step_1_QBOVendorCreditQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting VendorCredits
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from VendorCredit");
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet VendorCredit failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            VendorCreditOnlineRs vendorCreditRs = new(qryRs);
            Assert.IsNull(vendorCreditRs.ParseError);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOVendorCreditAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting VendorCredits
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from VendorCredit"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying VendorCredit: {await getRs.Content.ReadAsStringAsync()}");
            
            VendorCreditOnlineRs vendorCreditRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding VendorCredit
            if (vendorCreditRs.VendorCredits?.Any(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false) ?? false) Assert.Inconclusive($"{testName} already exists.");

            HttpResponseMessage purchasesQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Purchase"));
            if (!purchasesQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving accounts.\n{await purchasesQryRq.Content.ReadAsStringAsync()}");
            PurchaseOnlineRs purchaseRs = new(await purchasesQryRq.Content.ReadAsStringAsync());
            PurchaseDto purchase = purchaseRs.Purchases.OrderBy(a => Guid.NewGuid()).FirstOrDefault(p => p.EntityRef?.type.Equals("Vendor") ?? false);

            VendorCreditAddRq addRq = new();
            addRq.VendorRef = purchase.EntityRef;
            addRq.Line = purchase.Line;
            addRq.PrivateNote = testName;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            VendorCreditOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(1, addRs.TotalVendorCredits);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOVendorCreditModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting VendorCredit
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from VendorCredit"));
            
            VendorCreditOnlineRs vendorCreditRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating VendorCredit
            if (vendorCreditRs.TotalVendorCredits <= 0) Assert.Inconclusive("No VendorCredit to update.");

            VendorCreditDto vendorCredit = vendorCreditRs.VendorCredits.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (vendorCredit == null) Assert.Inconclusive($"{testName} does not exist.");

            HttpResponseMessage itemQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item where Type = 'Inventory'"));
            if (!itemQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving items.\n{await itemQryRq.Content.ReadAsStringAsync()}");
            ItemOnlineRs itemRs = new(await itemQryRq.Content.ReadAsStringAsync());
            ItemDto item = itemRs.Items.OrderBy(i => Guid.NewGuid()).FirstOrDefault();

            VendorCreditModRq modRq = new();
            modRq.CopyDto(vendorCredit);
            modRq.sparse = "true";
            modRq.Line.Add(new() 
            {
                DetailType = LineDetailType.ItemBasedExpenseLineDetail,
                Amount = item.UnitPrice,
                LineDetail = new ItemBasedExpenseLineDetailDto() { ItemRef = new(item.Id, item.Name), Qty = 5 },
            });
            modRq.PrivateNote = $"{testName} => {vendorCredit.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            VendorCreditOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(vendorCredit.PrivateNote, modRs.VendorCredits?[0]?.PrivateNote);
            Assert.AreNotEqual(vendorCredit.MetaData.LastUpdatedTime, modRs.VendorCredits[0].MetaData.LastUpdatedTime);
            #endregion
        }

        [TestMethod]
        public async Task Step_4_QBOVendorCreditDeleteTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting VendorCredit
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from VendorCredit"));
            if (!getRs.IsSuccessStatusCode) Assert.Inconclusive($"Could not retrieve VendorCredit to delete: {await getRs.Content.ReadAsStringAsync()}");
            
            VendorCreditOnlineRs vendorCreditRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Deleting VendorCredit
            if (vendorCreditRs.TotalVendorCredits <= 0) Assert.Inconclusive($"No {testName} to delete.");

            VendorCreditDto vendorCredit = vendorCreditRs.VendorCredits.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (vendorCredit == null) Assert.Inconclusive($"{testName} does not exist.");

            DeleteRq delRq = new("VendorCredit", vendorCredit.Id, vendorCredit.SyncToken);
            
            HttpResponseMessage postRs = await qboe.QBOPost(delRq.ApiParameter(qboe.ClientInfo.RealmId), delRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            VendorCreditOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(EntityStatus.Deleted, modRs.VendorCredits[0].status, $"VendorCredit status not Deleted: {modRs.VendorCredits[0].status}");
            #endregion
        }
    }
}
