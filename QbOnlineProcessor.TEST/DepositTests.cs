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
    public class TestDepositModels
    {
        readonly string testName = "IMS Deposit";

        [TestMethod]
        public async Task Step_1_QBODepositQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Deposits
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from Deposit", true);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet Deposit failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            DepositOnlineRs depositRs = new(qryRs);
            Assert.IsNull(depositRs.ParseError);
            Assert.AreNotEqual(0, depositRs.TotalDeposits);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBODepositAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Deposits
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Deposit"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying Deposit: {await getRs.Content.ReadAsStringAsync()}");
            DepositOnlineRs depRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding Deposit
            if (depRs.Deposits.Any(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false)) Assert.Inconclusive($"{testName} already exists.");

            Random rdm = new();
            HttpResponseMessage acctQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Account where AccountType = 'Income'"));
            if (!acctQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving customers.\n{await acctQryRq.Content.ReadAsStringAsync()}");
            AccountOnlineRs acctRs = new(await acctQryRq.Content.ReadAsStringAsync());
            AccountDto acct = acctRs.Accounts.ElementAt(rdm.Next(0, acctRs.TotalAccounts));

            HttpResponseMessage bankQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Account where AccountType = 'Bank'"));
            if (!bankQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving customers.\n{await acctQryRq.Content.ReadAsStringAsync()}");
            AccountOnlineRs bankRs = new(await bankQryRq.Content.ReadAsStringAsync());
            AccountDto bank = bankRs.Accounts.ElementAt(rdm.Next(0, bankRs.TotalAccounts));

            DepositAddRq addRq = new();
            addRq.DepositToAccountRef = new(bank.Id, bank.FullyQualifiedName);
            addRq.Line = new() { new()
            {
                DetailType = LineDetailType.DepositLineDetail,
                Amount = 1.23M,
                LineDetail = new DepositLineDetailDto() { AccountRef = new(acct.Id, acct.Name) },
            }};
            addRq.PrivateNote = testName;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            DepositOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(1, addRs.TotalDeposits);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBODepositModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Deposit
            Random rdm = new();
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Deposit"));
            DepositOnlineRs DepositRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating Deposit
            if (DepositRs.TotalDeposits <= 0) Assert.Inconclusive($"No {testName} to update.");
            DepositDto deposit = DepositRs.Deposits.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (deposit == null) Assert.Inconclusive($"{testName} does not exist.");
            DepositModRq modRq = new();
            modRq.sparse = "true";
            modRq.Id = deposit.Id;
            modRq.SyncToken = deposit.SyncToken;
            modRq.TotalAmt = deposit.TotalAmt;
            modRq.Line = deposit.Line;
            modRq.DepositToAccountRef = deposit.DepositToAccountRef;
            modRq.PrivateNote = $"{testName} => {deposit.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            DepositOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(deposit.PrivateNote, modRs.Deposits?[0]?.PrivateNote);
            #endregion
        }

        [TestMethod]
        public async Task Step_4_QBODepositDeleteTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Deposit
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Deposit"));
            if (!getRs.IsSuccessStatusCode) Assert.Inconclusive($"Could not retrieve Deposit to delete: {await getRs.Content.ReadAsStringAsync()}");
            DepositOnlineRs DepositRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Deleting Deposit
            if (DepositRs.TotalDeposits <= 0) Assert.Inconclusive($"No {testName} to delete.");

            DepositDto deposit = DepositRs.Deposits.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (deposit == null) Assert.Inconclusive($"{testName} does not exist.");

            DeleteRq delRq = new("Deposit", deposit.Id, deposit.SyncToken);
            
            HttpResponseMessage postRs = await qboe.QBOPost(delRq.ApiParameter(qboe.ClientInfo.RealmId), delRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            DepositOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(EntityStatus.Deleted, modRs.Deposits[0].status, $"{testName} status not Deleted: {modRs.Deposits[0].status}");
            #endregion
        }
    }
}
