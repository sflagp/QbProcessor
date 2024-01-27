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
        public async Task Step_1_QBOEmployeeQueryTest()
        {
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
            
            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            EmployeeOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(testName, addRs.Employees?[0]?.FamilyName, $"{testName} and FamilyName should be the same.");
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOEmployeeModTest()
        {
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
            modRq.CopyDto(emp);
            modRq.sparse = "true";
            modRq.PrintOnCheckName = $"Test => {emp.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            EmployeeOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(emp.PrintOnCheckName, modRs.Employees?[0]?.PrintOnCheckName);
            Assert.AreNotEqual(emp.MetaData.LastUpdatedTime, modRs.Employees[0].MetaData.LastUpdatedTime);
            #endregion
        }
    }
}
