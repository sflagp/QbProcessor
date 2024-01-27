using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestExchangeRateModels
    {
        readonly string testName = "IMS ExchangeRate";
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
        public async Task Step_1_QBOExchangeRateQueryTest()
        {
            #region Getting Company Currencies
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from ExchangeRate");
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            ExchangeRateOnlineRs exchgRateRs = new(qryRs);
            Assert.IsNull(exchgRateRs.ParseError);
            if (exchgRateRs.TotalExchangeRates <= 0) Assert.Inconclusive("Did not find any ExchangeRate.");
            Assert.AreNotEqual(0, exchgRateRs.TotalExchangeRates);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOExchangeRateModTest()
        {
            #region Getting ExchangeRate
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from ExchangeRate"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying ExchangeRate: {await getRs.Content.ReadAsStringAsync()}");
            
            ExchangeRateOnlineRs exchgRteRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating ExchangeRate
            if (!exchgRteRs.ExchangeRates.Any(c => c.SourceCurrencyCode?.Equals("IMS") ?? false)) Assert.Inconclusive($"{testName} already exists.");
            ExchangeRateDto exchgRte = exchgRteRs.ExchangeRates.FirstOrDefault(r => r.SourceCurrencyCode.Equals("IMS"));

            ExchangeRateModRq modRq = new();
            modRq.CopyDto(exchgRte);
            modRq.sparse = "true";
            modRq.SourceCurrencyCode = "IMS";
            modRq.AsOfDate = DateTime.Today;
            modRq.Rate = 1.11M;
            if (!modRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {modRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            ExchangeRateOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.IsTrue(modRs.ParseError == null);
            Assert.AreNotEqual(exchgRte.MetaData.LastUpdatedTime, modRs.ExchangeRates[0].MetaData.LastUpdatedTime);
            #endregion
        }
    }
}
