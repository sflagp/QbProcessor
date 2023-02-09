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
    public class TestPaymentMethodModels
    {
        readonly string testName = "IMS PaymentMethod";

        [TestMethod]
        public async Task Step_1a_QBOPaymentMethodQueryXmlTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting PaymentMethods
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from PaymentMethod"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            PaymentMethodOnlineRs paymentMethodRs = new(qryRs);
            Assert.IsNull(paymentMethodRs.ParseError);
            Assert.AreNotEqual(0, paymentMethodRs.TotalPaymentMethods);
            #endregion
        }

        [TestMethod]
        public async Task Step_1b_QBOPaymentMethodQueryJsonTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting PaymentMethods
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from PaymentMethod"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            PaymentMethodOnlineRs paymentMethodRs = new(qryRs);
            Assert.IsNull(paymentMethodRs.ParseError);
            Assert.AreNotEqual(0, paymentMethodRs.TotalPaymentMethods);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOPaymentMethodAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting PaymentMethod
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from PaymentMethod where Name='IMS PaymentMethod'"));
            PaymentMethodOnlineRs paymentMethodRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding PaymentMethod
            if (paymentMethodRs.TotalPaymentMethods > 0) Assert.Inconclusive($"{testName} already exists.");

            HttpResponseMessage acctQryRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Account"));
            AccountOnlineRs acctRs = new(await acctQryRs.Content.ReadAsStringAsync());
            AccountDto expense = acctRs.Accounts.OrderBy(a => Guid.NewGuid()).FirstOrDefault(a => a.AccountType == AccountType.Income);

            PaymentMethodAddRq addRq = new();
            addRq.Name = testName;
            addRq.Type = "NON_CREDIT_CARD";
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            PaymentMethodOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(testName, addRs.PaymentMethods?[0]?.Name);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOPaymentMethodModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting PaymentMethod
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from PaymentMethod where Name = '{testName}'"));
            PaymentMethodOnlineRs acctRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating PaymentMethod
            if (acctRs.TotalPaymentMethods <= 0) Assert.Fail($"No {testName} to update.");

            PaymentMethodDto pmtMethod = acctRs.PaymentMethods.FirstOrDefault(a => a.Name.Equals(testName));
            if (pmtMethod == null) Assert.Fail($"{testName} does not exist.");

            PaymentMethodModRq modRq = new();
            modRq.CopyDto(pmtMethod);
            modRq.sparse = "true";
            modRq.Type = modRq.Type == "CREDIT_CARD" ? "NON_CREDIT_CARD" : "CREDIT_CARD";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            PaymentMethodOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(pmtMethod.Type, modRs.PaymentMethods[0]?.Type);
            Assert.AreNotEqual(pmtMethod.MetaData.LastUpdatedTime, modRs.PaymentMethods[0].MetaData.LastUpdatedTime);
            #endregion
        }
    }
}
