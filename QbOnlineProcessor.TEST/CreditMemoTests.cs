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
    public class TestCreditMemoModels
    {
        readonly string testName = "IMS Credit Memo";
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
        public async Task Step_1_QBOCreditMemoQueryTest()
        {
            #region Getting CreditMemos
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from CreditMemo");
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet CreditMemo failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            CreditMemoOnlineRs creditMemoRs = new(qryRs);
            Assert.IsNull(creditMemoRs.ParseError);
            Assert.AreNotEqual(0, creditMemoRs.TotalCreditMemos);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOCreditMemoAddTest()
        {
            #region Getting CreditMemos
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from CreditMemo"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying CreditMemo: {await getRs.Content.ReadAsStringAsync()}");
            
            CreditMemoOnlineRs creditMemoRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding CreditMemo
            if (creditMemoRs.CreditMemos.Any(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false)) Assert.Inconclusive($"{testName} already exists.");

            HttpResponseMessage custQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Customer"));
            if (!custQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving customers.\n{await custQryRq.Content.ReadAsStringAsync()}");

            CustomerOnlineRs custRs = new(await custQryRq.Content.ReadAsStringAsync());
            CustomerDto cust = custRs.Customers.OrderBy(c => Guid.NewGuid()).FirstOrDefault();

            HttpResponseMessage itemQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item where Type = 'Service'"));
            if (!itemQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving items.\n{await itemQryRq.Content.ReadAsStringAsync()}");
            ItemOnlineRs itemRs = new(await itemQryRq.Content.ReadAsStringAsync());
            ItemDto item = itemRs.Items.OrderBy(i => Guid.NewGuid()).FirstOrDefault();

            CreditMemoAddRq addRq = new();
            addRq.CustomerRef = new(cust.Id, cust.FullyQualifiedName);
            addRq.Line = new() { new()
            {
                DetailType = LineDetailType.SalesItemLineDetail,
                Amount = 1.23M,
                LineDetail = new SalesItemLineDetailDto() { ItemRef = new(item.Id, item.Name) },
            }};
            addRq.PrivateNote = testName;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            CreditMemoOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(1, addRs.TotalCreditMemos);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOCreditMemoModTest()
        {
            #region Getting CreditMemo
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from CreditMemo"));
            
            CreditMemoOnlineRs creditMemoRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating CreditMemo
            if (creditMemoRs.TotalCreditMemos <= 0) Assert.Inconclusive("No CreditMemo to update.");
            
            CreditMemoDto creditMemo = creditMemoRs.CreditMemos.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (creditMemo == null) Assert.Inconclusive($"{testName} does not exist.");

            HttpResponseMessage itemQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item where Type = 'Inventory'"));
            if (!itemQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving items.\n{await itemQryRq.Content.ReadAsStringAsync()}");
            
            ItemOnlineRs itemRs = new(await itemQryRq.Content.ReadAsStringAsync());
            ItemDto item = itemRs.Items.OrderBy(i => Guid.NewGuid()).FirstOrDefault();

            CreditMemoModRq modRq = new();
            modRq.CopyDto(creditMemo);
            modRq.sparse = "true";
            modRq.Line.Add(new() 
            {
                DetailType = LineDetailType.SalesItemLineDetail,
                Amount = item.UnitPrice,
                LineDetail = new SalesItemLineDetailDto() { ItemRef = new(item.Id, item.Name), Qty = 5 },
            });
            modRq.PrivateNote = $"{testName} => {creditMemo.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            CreditMemoOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(creditMemo.PrivateNote, modRs.CreditMemos?[0]?.PrivateNote);
            Assert.AreNotEqual(creditMemo.MetaData.LastUpdatedTime, modRs.CreditMemos[0].MetaData.LastUpdatedTime);
            #endregion
        }

        [TestMethod]
        [Ignore]
        public async Task Step_4_QBOCreditMemoEmailTest()
        {
            #region Getting CreditMemo
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from CreditMemo"));
            
            CreditMemoOnlineRs creditMemoRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Emailing CreditMemo
            if (creditMemoRs.TotalCreditMemos <= 0) Assert.Inconclusive($"No {testName} to email.");

            CreditMemoDto creditMemo = creditMemoRs.CreditMemos.FirstOrDefault(cm => cm.PrivateNote?.StartsWith(testName) ?? false);
            if (creditMemo == null) Assert.Inconclusive($"{testName} does not exist.");
            
            HttpResponseMessage postRs = await qboe.QBOPost($"/v3/company/{qboe.ClientInfo.RealmId}/creditmemo/{creditMemo.Id}/send?sendTo=sfla_gp@yahoo.com");
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            CreditMemoOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(creditMemo.PrivateNote, modRs.CreditMemos?[0]?.PrivateNote);
            #endregion
        }

        [TestMethod]
        public async Task Step_5_QBOCreditMemoDeleteTest()
        {
            #region Getting CreditMemo
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from CreditMemo"));
            if (!getRs.IsSuccessStatusCode) Assert.Inconclusive($"Could not retrieve CreditMemo to delete: {await getRs.Content.ReadAsStringAsync()}");
            
            CreditMemoOnlineRs creditMemoRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Deleting CreditMemo
            if (creditMemoRs.TotalCreditMemos <= 0) Assert.Inconclusive($"No {testName} to delete.");

            CreditMemoDto creditMemo = creditMemoRs.CreditMemos.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (creditMemo == null) Assert.Inconclusive($"{testName} does not exist.");

            DeleteRq delRq = new("CreditMemo", creditMemo.Id, creditMemo.SyncToken);
            
            HttpResponseMessage postRs = await qboe.QBOPost(delRq.ApiParameter(qboe.ClientInfo.RealmId), delRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            CreditMemoOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(EntityStatus.Deleted, modRs.CreditMemos[0].status, $"CreditMemo status not Deleted: {modRs.CreditMemos[0].status}");
            #endregion
        }
    }
}
