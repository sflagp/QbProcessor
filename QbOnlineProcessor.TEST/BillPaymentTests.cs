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
    public class TestBillPaymentModels
    {
        readonly string testName = "IMS Bill Payment";

        [TestMethod]
        public async Task Step_1_QBOBillPaymentQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting BillPayments
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from BillPayment", true);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            BillPaymentOnlineRs billPaymentRs = new(qryRs);
            Assert.IsNull(billPaymentRs.ParseError);
            Assert.AreNotEqual(0, billPaymentRs.TotalBillPayments);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOBillPaymentAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting BillPayments
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from BillPayment"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying bill: {await getRs.Content.ReadAsStringAsync()}");
            BillPaymentOnlineRs acctRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding BillPayment
            if (acctRs.BillPayments.Any(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false)) Assert.Inconclusive($"{testName} already exists.");

            HttpResponseMessage acctQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Account where AccountType = 'Bank'"));
            if (!acctQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving bank accounts.\n{await acctQryRq.Content.ReadAsStringAsync()}");
            AccountOnlineRs accountRs = new(await acctQryRq.Content.ReadAsStringAsync());
            AccountDto bank = accountRs.Accounts.OrderBy(a => Guid.NewGuid()).FirstOrDefault();

            HttpResponseMessage billQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Bill"));
            if (!billQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving bills.\n{await billQryRq.Content.ReadAsStringAsync()}");
            BillOnlineRs billRs = new(await billQryRq.Content.ReadAsStringAsync());
            BillDto bill = billRs.Bills.OrderBy(b => Guid.NewGuid()).FirstOrDefault(b => b.Balance > 0);

            BillPaymentAddRq addRq = new();
            addRq.VendorRef = bill.VendorRef;
            addRq.TotalAmt = 12.34M;
            addRq.PayType = BillPaymentType.Check;
            addRq.Line = new() { new()
            {
                Amount = 12.34M,
                LinkedTxn = new() { new() { TxnId = bill.Id, TxnType = "Bill" } }
            }};
            addRq.CheckPayment = new() { BankAccountRef = new(bank.Id, bank.Name) };
            addRq.PrivateNote = testName;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            BillPaymentOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(1, addRs.TotalBillPayments);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOBillPaymentModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting BillPayment
            Random rdm = new();
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from BillPayment"));
            BillPaymentOnlineRs billPaymentRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating Bill
            if (billPaymentRs.TotalBillPayments <= 0) Assert.Fail($"No {testName} to update.");

            BillPaymentDto pmt = billPaymentRs.BillPayments.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (pmt == null) Assert.Inconclusive($"{testName} does not exist.");
            
            BillPaymentModRq modRq = new();
            modRq.CopyDto(pmt);
            modRq.sparse = "true";
            modRq.PrivateNote = $"{testName} => {pmt.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            BillPaymentOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(pmt.PrivateNote, modRs.BillPayments?[0]?.PrivateNote);
            #endregion
        }

        [TestMethod]
        public async Task Step_4_QBOBillPaymentDeleteTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting BillPayment
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from BillPayment"));
            if (!getRs.IsSuccessStatusCode) Assert.Inconclusive($"Could not retrieve bill payment to delete: {await getRs.Content.ReadAsStringAsync()}");
            BillPaymentOnlineRs billPmtRs = new(await getRs.Content.ReadAsStringAsync());

            #endregion

            #region Deleting BillPayment
            if (billPmtRs.TotalBillPayments <= 0) Assert.Fail($"No {testName} to delete.");

            BillPaymentDto billPmt = billPmtRs.BillPayments.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (billPmt == null) Assert.Fail($"{testName} does not exist.");

            DeleteRq delRq = new("BillPayment", billPmt.Id, billPmt.SyncToken);
            
            HttpResponseMessage postRs = await qboe.QBOPost(delRq.ApiParameter(qboe.ClientInfo.RealmId), delRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            BillPaymentOnlineRs delRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(EntityStatus.Deleted, delRs.BillPayments[0].status, $"Bill status not Deleted: {delRs.BillPayments[0].status}");
            #endregion
        }
    }
}
