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
    public class TestTransferModels
    {
        readonly string testName = "IMS Transfer";

        [TestMethod]
        public async Task Step_1_QBOTransferQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Transfers
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Transfer"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRsStr = await getRs.Content.ReadAsStringAsync();
            TransferOnlineRs qryRs = new(qryRsStr);
            Assert.IsNull(qryRs.ParseError, $"Transfer query parsing error: {qryRs.ParseError}");
            Assert.AreNotEqual(0, qryRs.TotalTransfers, "No Transfers found.");
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOTransferAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Transfer
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from Transfer where Name='{testName}'"));
            
            TransferOnlineRs TransferRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding Transfer
            if (TransferRs.TotalTransfers > 0) Assert.Inconclusive($"{testName} already exists.");

            HttpResponseMessage accountRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from Account"));
            Assert.IsTrue(accountRs.IsSuccessStatusCode, "Could not retrieve Accounts for transfer.");
            AccountOnlineRs accounts = new(await accountRs.Content.ReadAsStringAsync());
            AccountDto fromAcct = accounts.Accounts.OrderBy(a => Guid.NewGuid()).FirstOrDefault(a => a.AccountType == AccountType.FixedAsset);
            AccountDto toAcct = accounts.Accounts.OrderBy(a => Guid.NewGuid()).FirstOrDefault(a => a.AccountType == AccountType.FixedAsset && a.Id != fromAcct.Id);

            TransferAddRq addRq = new();
            addRq.FromAccountRef = new(fromAcct.Id);
            addRq.ToAccountRef = new(toAcct.Id);
            addRq.Amount = 12;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            TransferOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(fromAcct.Id, addRs.Transfers[0].FromAccountRef.Value);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOTransferModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Transfer
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from Transfer"));
            
            TransferOnlineRs TransferRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating Transfer
            if (TransferRs.TotalTransfers <= 0) Assert.Fail($"No {testName} to update.");

            TransferDto transfer = TransferRs.Transfers?.OrderBy(t => Guid.NewGuid()).FirstOrDefault();
            if (transfer == null) Assert.Fail($"{testName} does not exist.");

            TransferModRq modRq = new();
            modRq.CopyDto(transfer);
            modRq.Amount += 7;
            modRq.sparse = "true";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            TransferOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(transfer.MetaData.LastUpdatedTime, modRs.Transfers[0].MetaData.LastUpdatedTime);
            #endregion
        }

        [TestMethod]
        public async Task Step_4_QBOTransferDeleteTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Transfer
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Transfer"));
            if (!getRs.IsSuccessStatusCode) Assert.Inconclusive($"Could not retrieve Transfer to delete: {await getRs.Content.ReadAsStringAsync()}");

            TransferOnlineRs TransferRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Deleting Transfer
            if (TransferRs.TotalTransfers <= 0) Assert.Inconclusive($"No {testName} to delete.");

            TransferDto Transfer = TransferRs.Transfers?.OrderByDescending(t => t.MetaData.CreateTime).FirstOrDefault();
            if (Transfer == null) Assert.Inconclusive($"No transfers to delete.");

            DeleteRq delRq = new("Transfer", Transfer.Id, Transfer.SyncToken);

            HttpResponseMessage postRs = await qboe.QBOPost(delRq.ApiParameter(qboe.ClientInfo.RealmId), delRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            TransferOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(EntityStatus.Deleted, modRs.Transfers[0].status, $"Transfer status not Deleted: {modRs.Transfers[0].status}");
            #endregion
        }
    }
}
