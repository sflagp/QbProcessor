using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestEmployeeModels
    {
        readonly string testName = "IMS Employee";

        [TestMethod]
        public async Task Step_1_QBOEmployeeQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Employees
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Employee"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRsStr = await getRs.Content.ReadAsStringAsync();
            EmployeeOnlineRs qryRs = new(qryRsStr);
            Assert.IsNull(qryRs.ParseError, $"Employee query parsing error: {qryRs.ParseError}");
            Assert.AreNotEqual(0, qryRs.TotalEmployees, "No Employees found.");
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOEmployeeAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Employee
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from Employee where FamilyName='{testName}'"));
            EmployeeOnlineRs empRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding Employee
            if (empRs.TotalEmployees > 0) Assert.Inconclusive($"{testName} already exists.");
            
            EmployeeAddRq addRq = new();
            addRq.GivenName = "Test";
            addRq.FamilyName = testName;
            addRq.DisplayName = testName;
            addRq.PrintOnCheckName = testName;
            addRq.PrimaryPhone = new("(919) 555-1212");
            addRq.PrimaryAddr = new("123 Main Street", "Main", "NC", "27601");
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq, false);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            EmployeeOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(testName, addRs.Employees?[0]?.FamilyName, $"{testName} and FamilyName should be the same.");
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOEmployeeModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Employee
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from Employee where FamilyName = '{testName}'"));
            EmployeeOnlineRs empRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating Employee
            if (empRs.TotalEmployees <= 0) Assert.Fail("No Employees to update.");
            
            EmployeeDto emp = empRs.Employees.FirstOrDefault(a => a.FamilyName.Equals(testName));
            if (emp == null) Assert.Fail($"{testName} does not exist.");
            
            EmployeeModRq modRq = new();
            modRq.sparse = "true";
            modRq.Id = emp.Id;
            modRq.GivenName = emp.GivenName;
            modRq.FamilyName = emp.FamilyName;
            modRq.SyncToken = emp.SyncToken;
            modRq.GivenName = $"Test => {emp.SyncToken}";
            modRq.PrimaryAddr = emp.PrimaryAddr;
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            EmployeeOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(emp.GivenName, modRs.Employees?[0]?.GivenName);
            #endregion
        }
    }
}
