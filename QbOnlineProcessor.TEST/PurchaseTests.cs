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
    public class TestPurchaseModels
    {
        readonly string testName = "IMS Purchase";

        [TestMethod]
        public async Task Step_1_QBOPurchaseQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Purchases
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from Purchase", true);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet Purchase failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            PurchaseOnlineRs purchaseRs = new(qryRs);
            Assert.IsNull(purchaseRs.ParseError);
            Assert.AreNotEqual(0, purchaseRs.TotalPurchases);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOPurchaseAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Purchases
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Purchase"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying Purchase: {await getRs.Content.ReadAsStringAsync()}");
            
            PurchaseOnlineRs purchaseRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding Purchase
            if (purchaseRs.Purchases.Any(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false)) Assert.Inconclusive($"{testName} already exists.");

            HttpResponseMessage acctQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Account"));
            if (!acctQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving accounts.\n{await acctQryRq.Content.ReadAsStringAsync()}");
            AccountOnlineRs acctRs = new(await acctQryRq.Content.ReadAsStringAsync());
            AccountDto expenseAcct = acctRs.Accounts.OrderBy(a => Guid.NewGuid()).FirstOrDefault(a => a.AccountType.Equals(AccountType.Expense));
            AccountDto payAcct = acctRs.Accounts.OrderBy(a => Guid.NewGuid()).FirstOrDefault(a => a.AccountType.Equals(AccountType.Bank));

            PurchaseAddRq addRq = new();
            addRq.AccountRef = new(payAcct.Id, payAcct.FullyQualifiedName);
            addRq.PaymentType = PaymentType.Check;
            addRq.TotalAmt = 123.45M;
            addRq.TxnDate = DateTime.Now;
            addRq.Line = new() { new()
            {
                DetailType = LineDetailType.AccountBasedExpenseLineDetail,
                Amount = 123.45M,
                LineDetail = new AccountBasedExpenseLineDetailDto() { AccountRef = new(expenseAcct.Id, expenseAcct.Name) }
            }};
            addRq.PrivateNote = testName;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            PurchaseOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(1, addRs.TotalPurchases);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOPurchaseModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Purchase
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Purchase"));
            PurchaseOnlineRs purchaseRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating Purchase
            if (purchaseRs.TotalPurchases <= 0) Assert.Inconclusive("No Purchase to update.");

            PurchaseDto purchase = purchaseRs.Purchases.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (purchase == null) Assert.Inconclusive($"{testName} does not exist.");

            HttpResponseMessage itemQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item where Type = 'Inventory'"));
            if (!itemQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving items.\n{await itemQryRq.Content.ReadAsStringAsync()}");
            ItemOnlineRs itemRs = new(await itemQryRq.Content.ReadAsStringAsync());
            ItemDto item = itemRs.Items.OrderBy(i => Guid.NewGuid()).FirstOrDefault(i => i.Type == ItemType.Inventory);

            PurchaseModRq modRq = new();
            modRq.CopyDto(purchase, "MetaData");
            modRq.sparse = "true";
            modRq.Line.Add(new() 
            {
                DetailType = LineDetailType.ItemBasedExpenseLineDetail,
                Amount = item.UnitPrice,
                LineDetail = new ItemBasedExpenseLineDetailDto() { ItemRef = new(item.Id, item.Name), Qty = 50 },
            });
            modRq.PrivateNote = $"{testName} => {purchase.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            PurchaseOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(purchase.PrivateNote, modRs.Purchases?[0]?.PrivateNote);
            #endregion
        }

        [TestMethod]
        public async Task Step_4_QBOPurchaseDeleteTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Purchase
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Purchase"));
            if (!getRs.IsSuccessStatusCode) Assert.Inconclusive($"Could not retrieve Purchase to delete: {await getRs.Content.ReadAsStringAsync()}");
            
            PurchaseOnlineRs purchaseRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Deleting Purchase
            if (purchaseRs.TotalPurchases <= 0) Assert.Inconclusive($"No {testName} to delete.");

            PurchaseDto Purchase = purchaseRs.Purchases.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (Purchase == null) Assert.Inconclusive($"{testName} does not exist.");

            DeleteRq delRq = new("Purchase", Purchase.Id, Purchase.SyncToken);
            
            HttpResponseMessage postRs = await qboe.QBOPost(delRq.ApiParameter(qboe.ClientInfo.RealmId), delRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            PurchaseOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(EntityStatus.Deleted, modRs.Purchases[0].status, $"Purchase status not Deleted: {modRs.Purchases[0].status}");
            #endregion
        }
    }
}
