using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestClassModels
    {
        readonly string testName = "IMS Class";
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
        public async Task Step_1_QBOClassQueryTest()
        {
            #region Getting Classes
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from Class");
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            ClassOnlineRs ClassRs = new(qryRs);
            Assert.IsNull(ClassRs.ParseError);
            if (ClassRs.TotalClasses <= 0) Assert.Inconclusive("No classes found");
            Assert.AreNotEqual(0, ClassRs.TotalClasses);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOClassAddTest()
        {
            #region Getting Classes
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Class"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying Class: {await getRs.Content.ReadAsStringAsync()}");

            ClassOnlineRs clsRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding Class
            if (clsRs.Classes != null && clsRs.Classes.Any(c => c.FullyQualifiedName?.StartsWith(testName) ?? false)) Assert.Inconclusive($"{testName} already exists.");

            ClassAddRq addRq = new();
            addRq.Name = testName;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            ClassOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(1, addRs.TotalClasses);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOClassModTest()
        {
            #region Getting Class
            Random rdm = new();
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Class"));
            ClassOnlineRs ClassRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating Class
            if (ClassRs.TotalClasses <= 0) Assert.Inconclusive($"No {testName} to update.");

            ClassDto cls = ClassRs.Classes.FirstOrDefault(c => c.FullyQualifiedName.StartsWith(testName));
            if (cls == null) Assert.Inconclusive($"{testName} does not exist.");

            ClassModRq modRq = new();
            modRq.CopyDto(cls);
            modRq.sparse = "true";
            modRq.SubClass = !modRq.SubClass;

            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            ClassOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(cls.Name, modRs.Classes?[0]?.Name);
            #endregion
        }

        [TestMethod]
        public async Task Step_4_QBOClassDelTest()
        {
            #region Getting Class
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Class"));
            ClassOnlineRs ClassRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Deleting Class
            if (ClassRs.TotalClasses <= 0) Assert.Inconclusive($"No {testName} to update.");

            ClassDto cls = ClassRs.Classes.FirstOrDefault(c => c.FullyQualifiedName.StartsWith(testName));
            if (cls == null) Assert.Inconclusive($"{testName} does not exist.");

            ClassModRq modRq = new();
            modRq.CopyDto(cls);
            modRq.sparse = "true";
            modRq.Active = false;
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            ClassOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.IsTrue(modRs.Classes?[0]?.Name.Contains("deleted"));
            #endregion
        }
    }
}
