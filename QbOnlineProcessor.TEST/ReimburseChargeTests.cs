using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestReimburseChargeModels
    {
        readonly string testName = "IMS ReimburseCharge";
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
        public async Task Step_1_QBOReimburseChargeQueryTest()
        {
            #region Getting ReimburseCharges
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from ReimburseCharge");
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet ReimburseCharge failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            ReimburseChargeOnlineRs reimburseChargeRs = new(qryRs);
            Assert.IsNull(reimburseChargeRs.ParseError);
            #endregion
        }
    }
}
