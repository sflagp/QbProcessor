using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using QbModels.QBO.ENUM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestItemModels
    {
        readonly string testName = "IMS Item";

        [TestMethod]
        public async Task Step_1a_QBOItemQueryXmlTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Items
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item"), true);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            ItemOnlineRs acctRs = new(qryRs);
            Assert.IsNull(acctRs.ParseError);
            Assert.AreNotEqual(0, acctRs.TotalItems);
            #endregion
        }

        [TestMethod]
        public async Task Step_1b_QBOItemQueryJsonTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Items
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item"), false);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            ItemOnlineRs acctRs = new(qryRs);
            Assert.IsNull(acctRs.ParseError);
            Assert.AreNotEqual(0, acctRs.TotalItems);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOItemAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Item
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item where Name='IMS Item'"));
            ItemOnlineRs itemRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding Item
            if (itemRs.TotalItems > 0) Assert.Inconclusive($"{testName} already exists.");

            HttpResponseMessage acctQryRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Account"));
            AccountOnlineRs acctRs = new(await acctQryRs.Content.ReadAsStringAsync());
            AccountDto expense = acctRs.Accounts.OrderBy(a => Guid.NewGuid()).FirstOrDefault(a => a.AccountType == AccountType.Income);
            
            ItemAddRq addRq = new();
            addRq.Name = testName;
            addRq.Type = ItemType.Inventory;
            addRq.QtyOnHand = 123.45M;
            addRq.Description = $"{testName} Test";
            addRq.InvStartDate = DateTime.Today;
            addRq.IncomeAccountRef = new(expense.Id, expense.Name);
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            ItemOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(testName, addRs.Items?[0]?.Name);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOItemModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Item
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item where Name = 'IMS Item'"));
            ItemOnlineRs acctRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating Item
            if (acctRs.TotalItems <= 0) Assert.Fail($"No {testName} to update.");

            ItemDto item = acctRs.Items.FirstOrDefault(a => a.Name.Equals(testName));
            if (item == null) Assert.Fail($"{testName} does not exist.");
            
            ItemModRq modRq = new();
            modRq.CopyDto(item);
            modRq.sparse = "true";
            modRq.Description = $"{testName} Test => {item.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq, false);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            ItemOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(item.Description, modRs.Items?[0]?.Description);
            Assert.AreNotEqual(item.MetaData.LastUpdatedTime, modRs.Items[0].MetaData.LastUpdatedTime);
            #endregion
        }
    }
}
