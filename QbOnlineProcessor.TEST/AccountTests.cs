using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using QbModels.QBO.ENUM;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestAccountModels
    {
        readonly string testName = "IMS Account";

        [TestMethod]
        public async Task Step_1_QBOAccountQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting accounts
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Account"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            AccountOnlineRs acctRs = new(qryRs);
            Assert.IsNull(acctRs.ParseError);
            Assert.AreNotEqual(0, acctRs.TotalAccounts);

            List<AccountDto> imsAccts = acctRs.Accounts.Where(a => a.Description?.Contains("IMS") ?? false)?.ToList();
            if (imsAccts.Count == 0) Assert.Fail($"No {testName} found");
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOAccountAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting account
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Account where Name='IMS Account'"));
            AccountOnlineRs acctRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding account
            if (acctRs.TotalAccounts > 0) Assert.Inconclusive($"{testName} already exists.");

            AccountAddRq addRq = new();
            addRq.Name = testName;
            addRq.AccountType = AccountType.Income;
            addRq.Description = $"{testName} Test";
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            AccountOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(testName, addRs.Accounts?[0]?.Name);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOAccountModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting account
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Account where Name = 'IMS Account'"));
            AccountOnlineRs acctRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating account
            if (acctRs.TotalAccounts <= 0) Assert.Fail($"No {testName} to update.");

            AccountDto acct = acctRs.Accounts.FirstOrDefault(a => a.Name.Equals(testName));
            if (acct == null) Assert.Fail($"{testName} does not exist.");
            
            AccountModRq modRq = new();
            modRq.CopyDto(acct);
            modRq.sparse = "true";
            modRq.Description = $"{testName} Test => {acct.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            AccountOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(acct.Description, modRs.Accounts?[0]?.Description);
            Assert.AreNotEqual(acct.MetaData.LastUpdatedTime, modRs.Accounts[0].MetaData.LastUpdatedTime);
            #endregion
        }
    }
}
