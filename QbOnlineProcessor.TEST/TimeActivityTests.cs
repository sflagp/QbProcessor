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
    public class TestTimeActivityModels
    {
        readonly string testName = "IMS TimeActivity";

        [TestMethod]
        public async Task Step_1_QBOTimeActivityQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting TimeActivities
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from TimeActivity");
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet TimeActivity failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            TimeActivityOnlineRs timeActivityRs = new(qryRs);
            Assert.IsNull(timeActivityRs.ParseError);
            Assert.AreNotEqual(0, timeActivityRs.TotalTimeActivities);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOTimeActivityAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting TimeActivities
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from TimeActivity"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying TimeActivity: {await getRs.Content.ReadAsStringAsync()}");

            TimeActivityOnlineRs timeActivityRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding TimeActivity
            if (timeActivityRs.TimeActivities.Any(pmt => pmt.Description?.StartsWith(testName) ?? false)) Assert.Inconclusive($"{testName} already exists.");

            HttpResponseMessage empQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Employee"));
            if (!empQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving income employees.\n{await empQryRq.Content.ReadAsStringAsync()}");
            EmployeeOnlineRs empRs = new(await empQryRq.Content.ReadAsStringAsync());
            EmployeeDto employee = empRs.Employees.OrderBy(a => Guid.NewGuid()).FirstOrDefault();

            HttpResponseMessage custQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Customer"));
            if (!custQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving customers.\n{await custQryRq.Content.ReadAsStringAsync()}");
            CustomerOnlineRs custRs = new(await custQryRq.Content.ReadAsStringAsync());
            CustomerDto customer = custRs.Customers.OrderBy(a => Guid.NewGuid()).FirstOrDefault();

            TimeActivityAddRq addRq = new();
            addRq.Description = testName;
            addRq.NameOf = TimeActivityType.Employee;
            addRq.EmployeeRef = new(employee.Id);
            addRq.CustomerRef = new(customer.Id, customer.DisplayName);
            addRq.TxnDate = DateTime.Now.AddHours(-4);
            addRq.Hours = 3;
            addRq.HourlyRate = 150M;
            addRq.BillableStatus = BillableStatus.Billable;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            TimeActivityOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(1, addRs.TotalTimeActivities);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOTimeActivityModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting TimeActivity
            Random rdm = new();
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from TimeActivity"));
            TimeActivityOnlineRs TimeActivityRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating TimeActivity
            if (TimeActivityRs.TotalTimeActivities <= 0) Assert.Inconclusive($"No {testName} to update.");

            TimeActivityDto timeActivity = TimeActivityRs.TimeActivities.FirstOrDefault(t => t.Description?.StartsWith(testName) ?? false);
            if (timeActivity == null) Assert.Inconclusive($"{testName} does not exist.");
            
            TimeActivityModRq modRq = new();
            modRq.CopyDto(timeActivity);
            modRq.sparse = "true";
            modRq.Description = $"{testName} => {timeActivity.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            TimeActivityOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(timeActivity.Description, modRs.TimeActivities?[0]?.Description);
            Assert.AreNotEqual(timeActivity.MetaData.LastUpdatedTime, modRs.TimeActivities[0].MetaData.LastUpdatedTime);
            #endregion
        }

        [TestMethod]
        public async Task Step_4_QBOTimeActivityDeleteTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting TimeActivity
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from TimeActivity"));
            if (!getRs.IsSuccessStatusCode) Assert.Inconclusive($"Could not retrieve TimeActivity to delete: {await getRs.Content.ReadAsStringAsync()}");

            TimeActivityOnlineRs timeActivityRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Deleting TimeActivity
            if (timeActivityRs.TotalTimeActivities <= 0) Assert.Inconclusive($"No {testName} to delete.");

            TimeActivityDto timeActivity = timeActivityRs.TimeActivities.FirstOrDefault(pmt => pmt.Description?.StartsWith(testName) ?? false);
            if (timeActivity == null) Assert.Inconclusive($"{testName} does not exist.");

            DeleteRq delRq = new("TimeActivity", timeActivity.Id, timeActivity.SyncToken);
            
            HttpResponseMessage postRs = await qboe.QBOPost(delRq.ApiParameter(qboe.ClientInfo.RealmId), delRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            TimeActivityOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(EntityStatus.Deleted, modRs.TimeActivities[0].status, $"{testName} status not Deleted: {modRs.TimeActivities[0].status}");
            #endregion
        }
    }
}
