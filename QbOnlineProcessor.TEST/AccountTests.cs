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
            if (imsAccts.Count == 0) Assert.Fail("No IMS accounts found");
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
            if (acctRs.TotalAccounts > 0) Assert.Inconclusive("IMS Account already exists.");
            AccountAddRq addRq = new();
            addRq.Name = "IMS Account";
            addRq.AccountType = AccountType.Income;
            addRq.Description = "IMS Account Test";
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");
            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            AccountOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual("IMS Account", addRs.Accounts?[0]?.Name);
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
            if (acctRs.TotalAccounts <= 0) Assert.Fail("No accounts to update.");
            AccountDto acct = acctRs.Accounts.FirstOrDefault(a => a.Name.Equals("IMS Account"));
            if (acct == null) Assert.Fail($"IMS Account does not exist.");
            AccountModRq modRq = new();
            modRq.sparse = "true";
            modRq.Id = acct.Id;
            modRq.Name = acct.Name;
            modRq.SyncToken = acct.SyncToken;
            modRq.Description = $"IMS Account Test => {acct.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            AccountOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(acct.Description, modRs.Accounts?[0]?.Description);
            #endregion
        }
    }
}
