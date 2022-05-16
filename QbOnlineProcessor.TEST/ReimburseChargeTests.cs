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

        [TestMethod]
        public async Task Step_1_QBOReimburseChargeQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

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
