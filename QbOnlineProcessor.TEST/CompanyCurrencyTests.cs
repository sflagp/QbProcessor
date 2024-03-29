using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestCompanyCurrencyModels
    {
        readonly string testName = "IMS CompanyCurrency";
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
        public async Task Step_1_QBOCompanyCurrencyQueryTest()
        {
            #region Getting Company Currencies
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from CompanyCurrency");
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            CompanyCurrencyOnlineRs cmpyCurrRs = new(qryRs);
            Assert.IsNull(cmpyCurrRs.ParseError);

            if (cmpyCurrRs.TotalCompanyCurrencies <= 0) Assert.Inconclusive("Did not find any Company Currency");
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOCompanyCurrencyAddTest()
        {
            #region Getting Classes
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from CompanyCurrency"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying CompanyCurrency: {await getRs.Content.ReadAsStringAsync()}");
            
            CompanyCurrencyOnlineRs cmpCurrRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding Class
            if (cmpCurrRs.TotalCompanyCurrencies > 0 && cmpCurrRs.CompanyCurrencies.Any(c => c.Code?.Equals("IMS") ?? false)) Assert.Inconclusive($"{testName} already exists.");

            CompanyCurrencyAddRq addRq = new();
            addRq.Name = testName;
            addRq.Code = "IMS";
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            CompanyCurrencyOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.IsTrue(addRs.ParseError == null);
            #endregion
        }
    }
}
