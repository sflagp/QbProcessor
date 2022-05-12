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
    public class TestCreditCardPaymentModels
    {
        readonly string testName = "IMS CreditCardPayment";

        [TestMethod]
        public async Task Step_1_QBOCreditCardPaymentQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting CreditCardPayments
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from CreditCardPayment", true);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            CreditCardPaymentOnlineRs creditCardPaymentRs = new(qryRs);
            Assert.IsNull(creditCardPaymentRs.ParseError);
            //Assert.AreNotEqual(0, creditCardPaymentRs.TotalCreditCardPayments);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOCreditCardPaymentAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting CreditCardPayments
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from CreditCardPayment"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying CreditCardPayment: {await getRs.Content.ReadAsStringAsync()}");
            CreditCardPaymentOnlineRs qryRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding CreditCardPayment
            if (qryRs.CreditCardPayments.Any(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false)) Assert.Inconclusive($"{testName} already exists.");

            Random rdm = new();
            HttpResponseMessage acctQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Account"));
            if (!acctQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving bank accounts.\n{await acctQryRq.Content.ReadAsStringAsync()}");
            AccountOnlineRs bankRs = new(await acctQryRq.Content.ReadAsStringAsync());
            AccountDto bank = bankRs.Accounts.Where(b => b.AccountType == AccountType.Bank).OrderBy(ba => Guid.NewGuid()).FirstOrDefault();

            HttpResponseMessage ccQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Account"));
            if (!ccQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving credit card accounts.\n{await ccQryRq.Content.ReadAsStringAsync()}");
            AccountOnlineRs ccRs = new(await ccQryRq.Content.ReadAsStringAsync());
            AccountDto creditCard = ccRs.Accounts.Where(cc => cc.AccountType == AccountType.CreditCard).OrderBy(c => Guid.NewGuid()).FirstOrDefault();

            CreditCardPaymentAddRq addRq = new();
            addRq.BankAccountRef = new() { name = bank.Name, Value = bank.Id };
            addRq.CreditCardAccountRef = new() { name = creditCard.Name, Value = creditCard.Id };
            addRq.Amount = 12.34M;
            addRq.TxnDate = DateTime.Now;
            addRq.PrivateNote = testName;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");
            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            CreditCardPaymentOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(1, addRs.TotalCreditCardPayments);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOCreditCardPaymentModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting CreditCardPayment
            Random rdm = new();
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from CreditCardPayment"));
            CreditCardPaymentOnlineRs creditCardPaymentRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating CreditCardPayment
            if (creditCardPaymentRs.TotalCreditCardPayments <= 0) Assert.Fail($"No {testName} to update.");

            CreditCardPaymentTxnDto ccPmt = creditCardPaymentRs.CreditCardPayments.FirstOrDefault(pmt => pmt.PrivateNote.StartsWith(testName));
            if (ccPmt == null) Assert.Inconclusive($"{testName} does not exist.");

            CreditCardPaymentModRq modRq = new();
            modRq.sparse = "true";
            modRq.Id = ccPmt.Id;
            modRq.SyncToken = ccPmt.SyncToken;
            modRq.TxnDate = ccPmt.TxnDate;
            modRq.Amount = ccPmt.Amount;
            modRq.BankAccountRef = ccPmt.BankAccountRef;
            modRq.CreditCardAccountRef = ccPmt.CreditCardAccountRef;
            modRq.PrivateNote = $"{testName} => {ccPmt.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq,false);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            CreditCardPaymentOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(ccPmt.PrivateNote, modRs.CreditCardPayments?[0]?.PrivateNote);
            #endregion
        }

        [TestMethod]
        public async Task Step_4_QBOCreditCardPaymentDeleteTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting CreditCardPayment
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from CreditCardPayment"));
            if (!getRs.IsSuccessStatusCode) Assert.Inconclusive($"Could not retrieve CreditCardPayment to delete: {await getRs.Content.ReadAsStringAsync()}");
            CreditCardPaymentOnlineRs billPmtRs = new(await getRs.Content.ReadAsStringAsync());

            #endregion

            #region Deleting CreditCardPayment
            if (billPmtRs.TotalCreditCardPayments <= 0) Assert.Fail($"No {testName} to delete.");

            CreditCardPaymentTxnDto ccPmt = billPmtRs.CreditCardPayments.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (ccPmt == null) Assert.Fail($"IMS CreditCardPayment does not exist.");

            CreditCardPaymentModRq modRq = new();
            modRq.Id = ccPmt.Id;
            modRq.SyncToken = ccPmt.SyncToken;
            HttpResponseMessage postRs = await qboe.QBOPost($"{modRq.ApiParameter(qboe.ClientInfo.RealmId)}?operation=delete", modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            CreditCardPaymentOnlineRs delRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual("Deleted", delRs.CreditCardPayments[0].status, $"{testName} status not Deleted: {delRs.CreditCardPayments[0].status}");
            #endregion
        }
    }
}
