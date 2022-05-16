using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestJournalCodeModels
    {
        readonly string testName = "IMS JournalCode";

        [TestMethod]
        public async Task Step_1_QBOJournalCodeQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting JournalCodes
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from JournalCode"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRsStr = await getRs.Content.ReadAsStringAsync();
            JournalCodeOnlineRs qryRs = new(qryRsStr);
            Assert.IsNull(qryRs.ParseError, $"JournalCode query parsing error: {qryRs.ParseError}");
            Assert.AreNotEqual(0, qryRs.TotalJournalCodes, "No JournalCodes found.");
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOJournalCodeAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting JournalCode
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from JournalCode where Name='{testName}'"));
            
            JournalCodeOnlineRs acctRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding JournalCode
            if (acctRs.TotalJournalCodes > 0) Assert.Inconclusive($"{testName} already exists.");
            
            JournalCodeAddRq addRq = new();
            addRq.Name = testName;
            addRq.Type = "Sales";
            addRq.Description = testName;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            JournalCodeOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(testName, addRs.JournalCodes?[0]?.Name);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOJournalCodeModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting JournalCode
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from JournalCode"));
            
            JournalCodeOnlineRs deptRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating JournalCode
            if (deptRs.TotalJournalCodes <= 0) Assert.Inconclusive($"No JournalCode to update.");

            JournalCodeDto dept = deptRs.JournalCodes.FirstOrDefault(a => a.Name.StartsWith(testName));
            if (dept == null) Assert.Inconclusive($"{testName} does not exist.");

            JournalCodeModRq modRq = new();
            modRq.CopyDto(dept);
            modRq.sparse = "true";
            modRq.Description = $"{testName} => {modRq.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            JournalCodeOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(dept.Description, modRs.JournalCodes[0].Description);
            Assert.AreNotEqual(dept.MetaData.LastUpdatedTime, modRs.JournalCodes[0].MetaData?.LastUpdatedTime);
            #endregion
        }
    }
}
