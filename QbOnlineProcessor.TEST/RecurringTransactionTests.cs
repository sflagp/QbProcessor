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
    public class TestRecurringTransactionModels
    {
        readonly string testName = "IMS RecurringTransaction";

        [TestMethod]
        public async Task Step_1_QBORecurringTransactionQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting RecurringTransactions
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from RecurringTransaction", false);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet RecurringTransaction failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            RecurringTransactionOnlineRs recurringTransactionRs = new(qryRs);
            Assert.IsNull(recurringTransactionRs.ParseError);
            Assert.AreNotEqual(0, recurringTransactionRs.TotalRecurringTransactions);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBORecurringTransactionAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting RecurringTransactions
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from RecurringTransaction"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying RecurringTransaction: {await getRs.Content.ReadAsStringAsync()}");
            
            RecurringTransactionOnlineRs recurringTransactionRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding RecurringTransaction
            if (recurringTransactionRs.RecurringTransactions.Any(txn => txn.Bill?.RecurringInfo.Name?.StartsWith(testName) ?? false)) Assert.Inconclusive($"{testName} already exists.");

            HttpResponseMessage acctQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Account"));
            if (!acctQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving accounts.\n{await acctQryRq.Content.ReadAsStringAsync()}");
            AccountOnlineRs acctRs = new(await acctQryRq.Content.ReadAsStringAsync());
            AccountDto expenseAcct = acctRs.Accounts.OrderBy(a => Guid.NewGuid()).FirstOrDefault(a => a.AccountType.Equals(AccountType.Expense));

            RecurringTransactionAddRq addRq = new();
            BillDto bill = new();
            bill.SalesTermRef = new("3");
            bill.Balance = 123.45M;
            bill.CurrencyRef = new("USD");
            bill.RecurDataRef = new("3");
            bill.VendorRef = new("40");
            bill.APAccountRef = new("33");
            bill.Line = new() { new()
            {
                Description = $"{testName}",
                Amount = 321M,
                DetailType = LineDetailType.AccountBasedExpenseLineDetail,
                LineDetail = new AccountBasedExpenseLineDetailDto() { AccountRef = new(expenseAcct.Id), BillableStatus = BillableStatus.NotBillable, TaxCodeRef = new("NON") }
            }};
            bill.TotalAmt = 123.45M;
            bill.RecurringInfo = new()
            {
                Name = $"{testName}",
                RecurType = "Automated",
                Active = true,
                ScheduleInfo = new() { IntervalType = "Monthly", NumInterval = 1, DayOfMonth = 1, NextDate = DateTime.Now.AddDays(30) }
            };
            addRq.Bill = bill;

            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            RecurringTransactionOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(1, addRs.TotalRecurringTransactions);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBORecurringTransactionModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting RecurringTransaction
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from RecurringTransaction"));
            
            RecurringTransactionOnlineRs recurringTransactionRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating RecurringTransaction
            if (recurringTransactionRs.TotalRecurringTransactions <= 0) Assert.Inconclusive("No RecurringTransaction to update.");

            RecurringTransactionDto recurringTransaction = recurringTransactionRs.RecurringTransactions.FirstOrDefault(txn => txn.Bill?.RecurringInfo.Name?.StartsWith(testName) ?? false);
            if (recurringTransaction == null) Assert.Inconclusive($"{testName} does not exist.");

            RecurringTransactionModRq modRq = new();
            modRq.CopyDto(recurringTransaction);
            modRq.Id = modRq.Bill.Id;
            modRq.SyncToken = modRq.Bill.SyncToken;
            modRq.Bill.MetaData = null;
            modRq.sparse = "true";
            modRq.Bill.RecurringInfo.ScheduleInfo.NextDate = DateTime.Today.AddDays(30);
            modRq.Bill.Line[0].Description = $"{testName} => {modRq.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            RecurringTransactionOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(1, modRs.TotalRecurringTransactions);
            #endregion
        }

        [TestMethod]
        [Ignore]
        public async Task Step_4_QBORecurringTransactionDeleteTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting RecurringTransaction
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from RecurringTransaction"));
            if (!getRs.IsSuccessStatusCode) Assert.Inconclusive($"Could not retrieve RecurringTransaction to delete: {await getRs.Content.ReadAsStringAsync()}");
            
            RecurringTransactionOnlineRs recurringTransactionRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Deleting RecurringTransaction
            if (recurringTransactionRs.TotalRecurringTransactions <= 0) Assert.Inconclusive($"No {testName} to delete.");

            RecurringTransactionDto recurringTransaction = recurringTransactionRs.RecurringTransactions.FirstOrDefault(txn => txn.Bill?.RecurringInfo.Name?.StartsWith(testName) ?? false);
            if (recurringTransaction == null) Assert.Inconclusive($"{testName} does not exist.");

            DeleteRq delRq = new("recurringtransaction", recurringTransaction.Bill.Id, recurringTransaction.Bill.SyncToken);

            HttpResponseMessage postRs = await qboe.QBOPost(delRq.ApiParameter(qboe.ClientInfo.RealmId), delRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            RecurringTransactionOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.IsNull(modRs.ParseError, $"Delete RecurringTransaction result parse error: {modRs.ParseError}");
            #endregion
        }
    }
}
