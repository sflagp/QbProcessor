using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestTermModels
    {
        readonly string testName = "IMS Term";
        private QBOProcessor qboe;

        [TestInitialize]
        public async Task InitializeTest()
        {
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();

            qboe = new();
        }

        [TestCleanup]
        public Task CleanupTest()
        {
            qboe.Dispose();
            return Task.CompletedTask;
        }

        [TestMethod]
        public async Task Step_1_QBOTermQueryTest()
        {
            #region Getting Terms
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Term"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRsStr = await getRs.Content.ReadAsStringAsync();
            TermOnlineRs qryRs = new(qryRsStr);
            Assert.IsNull(qryRs.ParseError, $"Term query parsing error: {qryRs.ParseError}");
            Assert.AreNotEqual(0, qryRs.TotalTerms, "No Terms found.");
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOTermAddTest()
        {
            #region Getting Term
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from Term where Name='{testName}'"));
            
            TermOnlineRs termRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding Term
            if (termRs.TotalTerms > 0) Assert.Inconclusive($"{testName} already exists.");

            TermAddRq addRq = new();
            addRq.Name = testName;
            addRq.DueDays = 0;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            TermOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(testName, addRs.Terms?[0]?.Name);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOTermModTest()
        {
            #region Getting Term
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from Term"));
            
            TermOnlineRs termRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating Term
            if (termRs.TotalTerms <= 0) Assert.Fail($"No {testName} to update.");

            TermDto term = termRs.Terms.FirstOrDefault(a => a.Name.Equals(testName));
            if (term == null) Assert.Fail($"{testName} does not exist.");

            TermModRq modRq = new();
            modRq.CopyDto(term);
            modRq.sparse = "true";
            modRq.DueDays = (term.DueDays ?? 0) + 30;
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            TermOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(term.DueDays, modRs.Terms?[0]?.DueDays);
            Assert.AreNotEqual(term.MetaData.LastUpdatedTime, modRs.Terms[0].MetaData.LastUpdatedTime);
            #endregion
        }
    }
}
