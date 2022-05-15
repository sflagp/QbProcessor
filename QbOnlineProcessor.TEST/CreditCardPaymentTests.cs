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
            if (qryRs.TotalCreditCardPayments > 0 && qryRs.CreditCardPayments.Any(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false)) Assert.Inconclusive($"{testName} already exists.");

            HttpResponseMessage acctQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Account"));
            if (!acctQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving accounts.\n{await acctQryRq.Content.ReadAsStringAsync()}");
            AccountOnlineRs acctsRs = new(await acctQryRq.Content.ReadAsStringAsync());
            AccountDto bank = acctsRs.Accounts.OrderBy(ba => Guid.NewGuid()).FirstOrDefault(b => b.AccountType == AccountType.Bank);
            AccountDto creditCard = acctsRs.Accounts.OrderBy(c => Guid.NewGuid()).FirstOrDefault(cc => cc.AccountType == AccountType.CreditCard);

            CreditCardPaymentAddRq addRq = new();
            addRq.BankAccountRef = new(bank.Id, bank.Name);
            addRq.CreditCardAccountRef = new(creditCard.Id, creditCard.Name);
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
            modRq.CopyDto(ccPmt);
            modRq.sparse = "true";
            modRq.PrivateNote = $"{testName} => {ccPmt.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq,false);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            CreditCardPaymentOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(ccPmt.PrivateNote, modRs.CreditCardPayments?[0]?.PrivateNote);
            Assert.AreNotEqual(ccPmt.MetaData.LastUpdatedTime, modRs.CreditCardPayments[0].MetaData.LastUpdatedTime);
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

            DeleteRq delRq = new("CreditCardPayment", ccPmt.Id, ccPmt.SyncToken);

            // Credit card payment will only work as Json.  #becauseintuit
            HttpResponseMessage postRs = await qboe.QBOPost(delRq.ApiParameter(qboe.ClientInfo.RealmId), delRq, false);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            CreditCardPaymentOnlineRs delRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual("Deleted", delRs.CreditCardPayments[0].status, $"{testName} status not Deleted: {delRs.CreditCardPayments[0].status}");
            #endregion
        }
    }
}
