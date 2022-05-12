using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestCustomerModels
    {
        readonly string testName = "IMS Customer";

        [TestMethod]
        public async Task Step_1_QBOCustomerQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Customers
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Customer"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRsStr = await getRs.Content.ReadAsStringAsync();
            CustomerOnlineRs qryRs = new(qryRsStr);
            Assert.IsNull(qryRs.ParseError, $"Customer query parsing error: {qryRs.ParseError}");
            Assert.AreNotEqual(0, qryRs.TotalCustomers, "No customers found.");
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOCustomerAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Customer
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from Customer where DisplayName='{testName}'"));
            CustomerOnlineRs acctRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding Customer
            if (acctRs.TotalCustomers > 0) Assert.Inconclusive($"{testName} already exists.");

            CustomerAddRq addRq = new();
            addRq.GivenName = testName;
            addRq.DisplayName = testName;
            addRq.Notes = $"{testName} Test";
            addRq.PrimaryPhone = new("(919) 555-1212");
            addRq.BillAddr = new("123 Main Street", "Main", "NC", "12345" );
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            CustomerOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(testName, addRs.Customers?[0]?.FullyQualifiedName);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOCustomerModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Customer
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from Customer where DisplayName = '{testName}'"));
            CustomerOnlineRs acctRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating Customer
            if (acctRs.TotalCustomers <= 0) Assert.Fail($"No {testName} to update.");

            CustomerDto acct = acctRs.Customers.FirstOrDefault(a => a.FullyQualifiedName.Equals(testName));
            if (acct == null) Assert.Fail($"{testName} does not exist.");

            CustomerModRq modRq = new();
            modRq.sparse = "true";
            modRq.Id = acct.Id;
            modRq.GivenName = acct.GivenName;
            modRq.SyncToken = acct.SyncToken;
            modRq.Notes = $"{testName} Test => {acct.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            CustomerOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(acct.Notes, modRs.Customers?[0]?.Notes);
            #endregion
        }

        [TestMethod]
        public async Task Step_5_QBOCustomerTypeQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Customers
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from CustomerType"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRsStr = await getRs.Content.ReadAsStringAsync();
            CustomerTypeOnlineRs qryRs = new(qryRsStr);
            Assert.IsNull(qryRs.ParseError, $"CustomerType query parsing error: {qryRs.ParseError}");
            if(qryRs.TotalCustomerTypes == 0) Assert.Inconclusive("No CustomerTypes found.");
            #endregion
        }
    }
}
