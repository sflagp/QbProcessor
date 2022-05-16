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
    public class TestJournalEntryModels
    {
        readonly string testName = "IMS JournalEntry";

        [TestMethod]
        public async Task Step_1_QBOJournalEntryQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting JournalEntries
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from JournalEntry");
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet JournalEntry failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            JournalEntryOnlineRs journalEntryRs = new(qryRs);
            Assert.IsNull(journalEntryRs.ParseError);
            Assert.AreNotEqual(0, journalEntryRs.TotalJournalEntries);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOJournalEntryAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting JournalEntries
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from JournalEntry"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying JournalEntry: {await getRs.Content.ReadAsStringAsync()}");
            
            JournalEntryOnlineRs depRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding JournalEntry
            if (depRs.JournalEntries.Any(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false)) Assert.Inconclusive($"{testName} already exists.");

            HttpResponseMessage acctQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Account where AccountType = 'Income'"));
            if (!acctQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving income accounts.\n{await acctQryRq.Content.ReadAsStringAsync()}");
            AccountOnlineRs acctRs = new(await acctQryRq.Content.ReadAsStringAsync());
            AccountDto income = acctRs.Accounts.OrderBy(a => Guid.NewGuid()).FirstOrDefault();

            HttpResponseMessage expenseQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Account where AccountType = 'Expense'"));
            if (!expenseQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving expense accounts.\n{await expenseQryRq.Content.ReadAsStringAsync()}");
            AccountOnlineRs expenseRs = new(await expenseQryRq.Content.ReadAsStringAsync());
            AccountDto expense = expenseRs.Accounts.OrderBy(a => Guid.NewGuid()).FirstOrDefault();

            JournalEntryAddRq addRq = new();
            addRq.PrivateNote = testName;
            addRq.Line = new() { new()
            {
                DetailType = LineDetailType.JournalEntryLineDetail,
                Amount = 123.45M,
                LineDetail = new JournalEntryLineDetailDto() { AccountRef = new(income.Id, income.Name), PostingType = PostingType.Credit },
                Description = $"{testName} credit posting."
            }};
            addRq.Line.Add(new()
            {
                DetailType = LineDetailType.JournalEntryLineDetail,
                Amount = 123.45M,
                LineDetail = new JournalEntryLineDetailDto() { AccountRef = new(income.Id, income.Name), PostingType = PostingType.Debit },
                Description = $"{testName} debit posting."
            });
            addRq.PrivateNote = testName;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            JournalEntryOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(1, addRs.TotalJournalEntries);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOJournalEntryModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting JournalEntry
            Random rdm = new();
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from JournalEntry"));
            JournalEntryOnlineRs journalEntryRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating JournalEntry
            if (journalEntryRs.TotalJournalEntries <= 0) Assert.Inconclusive($"No {testName} to update.");

            JournalEntryDto journalEntry = journalEntryRs.JournalEntries.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (journalEntry == null) Assert.Inconclusive($"{testName} does not exist.");
            
            JournalEntryModRq modRq = new();
            modRq.sparse = "true";
            modRq.Id = journalEntry.Id;
            modRq.SyncToken = journalEntry.SyncToken;
            modRq.TotalAmt = journalEntry.TotalAmt;
            modRq.Line = journalEntry.Line;
            modRq.PrivateNote = $"{testName} => {journalEntry.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            JournalEntryOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(journalEntry.PrivateNote, modRs.JournalEntries?[0]?.PrivateNote);
            Assert.AreNotEqual(journalEntry.MetaData.LastUpdatedTime, modRs.JournalEntries[0].MetaData.LastUpdatedTime);
            #endregion
        }

        [TestMethod]
        public async Task Step_4_QBOJournalEntryDeleteTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting JournalEntry
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from JournalEntry"));
            if (!getRs.IsSuccessStatusCode) Assert.Inconclusive($"Could not retrieve JournalEntry to delete: {await getRs.Content.ReadAsStringAsync()}");
            JournalEntryOnlineRs journalEntryRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Deleting JournalEntry
            if (journalEntryRs.TotalJournalEntries <= 0) Assert.Inconclusive($"No {testName} to delete.");

            JournalEntryDto journalEntry = journalEntryRs.JournalEntries.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (journalEntry == null) Assert.Inconclusive($"{testName} does not exist.");

            DeleteRq delRq = new("JournalEntry", journalEntry.Id, journalEntry.SyncToken);
            
            HttpResponseMessage postRs = await qboe.QBOPost(delRq.ApiParameter(qboe.ClientInfo.RealmId), delRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            JournalEntryOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(EntityStatus.Deleted, modRs.JournalEntries[0].status, $"{testName} status not Deleted: {modRs.JournalEntries[0].status}");
            #endregion
        }
    }
}
