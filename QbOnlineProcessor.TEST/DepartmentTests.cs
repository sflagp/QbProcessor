using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestDepartmentModels
    {
        readonly string testName = "IMS Department";

        [TestMethod]
        public async Task Step_1_QBODepartmentQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Departments
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Department"), false);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRsStr = await getRs.Content.ReadAsStringAsync();
            DepartmentOnlineRs qryRs = new(qryRsStr);
            Assert.IsNull(qryRs.ParseError, $"Department query parsing error: {qryRs.ParseError}");
            Assert.AreNotEqual(0, qryRs.TotalDepartments, "No Departments found.");
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBODepartmentAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Department
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from Department where Name='{testName}'"));
            DepartmentOnlineRs acctRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding Department
            if (acctRs.TotalDepartments > 0) Assert.Inconclusive($"{testName} already exists.");
            DepartmentAddRq addRq = new();
            addRq.Name = testName;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");
            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            DepartmentOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(testName, addRs.Departments?[0]?.FullyQualifiedName);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBODepartmentModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Department
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from Department where Name like '{testName}%'"));
            DepartmentOnlineRs deptRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating Department
            if (deptRs.TotalDepartments <= 0) Assert.Fail($"No {testName} to update.");

            DepartmentDto dept = deptRs.Departments.FirstOrDefault(a => a.FullyQualifiedName.StartsWith(testName));
            if (dept == null) Assert.Fail($"{testName} does not exist.");

            DepartmentModRq modRq = new();
            modRq.sparse = "true";
            modRq.Id = dept.Id;
            modRq.SyncToken = dept.SyncToken;
            modRq.Name = dept.Name;
            modRq.Address = new() { Line1 = "123 Main St", City = "Hollywood", CountrySubDivisionCode = "FL" };
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            DepartmentOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(0, modRs.TotalDepartments);
            #endregion
        }
    }
}
