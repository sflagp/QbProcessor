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
        [TestMethod]
        public async Task Step_1_QBOCompanyCurrencyQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Company Currencies
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from CompanyCurrency", false);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            CompanyCurrencyOnlineRs cmpyCurrRs = new(qryRs);
            Assert.IsNull(cmpyCurrRs.ParseError);
            if (cmpyCurrRs.TotalCompanyCurrencies <= 0) Assert.Inconclusive("Did not find any Company Currency");
            Assert.AreNotEqual(0, cmpyCurrRs.TotalCompanyCurrencies);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOCompanyCurrencyAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Classes
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from CompanyCurrency"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying Class: {await getRs.Content.ReadAsStringAsync()}");
            CompanyCurrencyOnlineRs cmpCurrRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding Class
            if (cmpCurrRs.CompanyCurrencies.Any(c => c.Code?.Equals("IMS") ?? false)) Assert.Inconclusive("IMS CompanyCurrency already exists.");

            CompanyCurrencyAddRq addRq = new();
            addRq.Name = "IMS CompanyCurrency";
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
