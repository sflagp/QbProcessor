using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestVendorModels
    {
        readonly string testName = "IMS Vendor";
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
        public async Task Step_1_QBOVendorQueryTest()
        {
            #region Getting Vendors
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Vendor"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRsStr = await getRs.Content.ReadAsStringAsync();
            VendorOnlineRs qryRs = new(qryRsStr);
            Assert.IsNull(qryRs.ParseError, $"Vendor query parsing error: {qryRs.ParseError}");
            Assert.AreNotEqual(0, qryRs.TotalVendors, "No Vendors found.");
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOVendorAddTest()
        {
            using QBOProcessor qboe = new();

            #region Getting Vendor
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from Vendor where DisplayName='{testName}'"));
            
            VendorOnlineRs acctRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding Vendor
            if (acctRs.TotalVendors > 0) Assert.Inconclusive($"{testName} already exists.");

            VendorAddRq addRq = new();
            addRq.GivenName = testName;
            addRq.DisplayName = testName;
            addRq.PrintOnCheckName = $"{testName} Test";
            addRq.PrimaryPhone = new("(919) 555-1212");
            addRq.BillAddr = new("123 Main Street", "Main", "NC", "12345" );
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            VendorOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(testName, addRs.Vendors?[0]?.DisplayName);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOVendorModTest()
        {
            #region Getting Vendor
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from Vendor where DisplayName = '{testName}'"));
            
            VendorOnlineRs acctRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating Vendor
            if (acctRs.TotalVendors <= 0) Assert.Fail($"No {testName} to update.");

            VendorDto vendor = acctRs.Vendors.FirstOrDefault(a => a.DisplayName.Equals(testName));
            if (vendor == null) Assert.Fail($"{testName} does not exist.");

            VendorModRq modRq = new();
            modRq.CopyDto(vendor);
            modRq.sparse = "true";
            modRq.AcctNum = $"{testName} Test => {vendor.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            VendorOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(vendor.AcctNum, modRs.Vendors?[0]?.AcctNum);
            #endregion
        }
    }
}
