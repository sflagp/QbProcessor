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
    public class TestEstimateModels
    {
        readonly string testName = "IMS Estimate";

        [TestMethod]
        public async Task Step_2_QBOEstimateAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Estimates
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Estimate"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying Estimate: {await getRs.Content.ReadAsStringAsync()}");
            EstimateOnlineRs estimateRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding Estimate
            if (estimateRs.Estimates.Any(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false)) Assert.Inconclusive($"{testName} already exists.");

            Random rdm = new();
            HttpResponseMessage custQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Customer"));
            if (!custQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving customers.\n{await custQryRq.Content.ReadAsStringAsync()}");
            CustomerOnlineRs custRs = new(await custQryRq.Content.ReadAsStringAsync());
            CustomerDto cust = custRs.Customers.ElementAt(rdm.Next(0, custRs.TotalCustomers));

            HttpResponseMessage itemQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item where Type = 'Service'"));
            if (!itemQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving items.\n{await itemQryRq.Content.ReadAsStringAsync()}");
            ItemOnlineRs itemRs = new(await itemQryRq.Content.ReadAsStringAsync());
            ItemDto item = itemRs.Items.ElementAt(rdm.Next(0, itemRs.TotalItems));

            EstimateAddRq addRq = new();
            addRq.CustomerRef = new(cust.Id, cust.FullyQualifiedName);
            addRq.Line = new() { new()
            {
                DetailType = LineDetailType.SalesItemLineDetail,
                Amount = 1.23M,
                LineDetail = new SalesItemLineDetailDto() { ItemRef = new(item.Id, item.Name) },
            }};
            addRq.PrivateNote = testName;
            if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");

            HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
            if (!postRs.IsSuccessStatusCode) Assert.Inconclusive($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            EstimateOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(1, addRs.TotalEstimates);
            #endregion
        }

        [TestMethod]
        public async Task Step_1_QBOEstimateQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Estimates
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from Estimate");
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet Estimate failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            EstimateOnlineRs estimateRs = new(qryRs);
            Assert.IsNull(estimateRs.ParseError);
            Assert.AreNotEqual(0, estimateRs.TotalEstimates);
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOEstimateModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Estimate
            Random rdm = new();
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Estimate"));
            EstimateOnlineRs estimateRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating Estimate
            if (estimateRs.TotalEstimates <= 0) Assert.Inconclusive("No Estimate to update.");
            EstimateDto estimate = estimateRs.Estimates.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (estimate == null) Assert.Inconclusive($"{testName} does not exist.");

            HttpResponseMessage itemQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Item where Type = 'Inventory'"));
            if (!itemQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving items.\n{await itemQryRq.Content.ReadAsStringAsync()}");
            ItemOnlineRs itemRs = new(await itemQryRq.Content.ReadAsStringAsync());
            ItemDto item = itemRs.Items.ElementAt(rdm.Next(0, itemRs.TotalItems));

            EstimateModRq modRq = new();
            modRq.CopyDto(estimate);
            modRq.sparse = "true";
            modRq.Line.Add(new() 
            {
                DetailType = LineDetailType.SalesItemLineDetail,
                Amount = item.UnitPrice,
                LineDetail = new SalesItemLineDetailDto() { ItemRef = new(item.Id, item.Name), Qty = 5 },
            });
            modRq.PrivateNote = $"{testName} => {estimate.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");
            
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            EstimateOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(estimate.PrivateNote, modRs.Estimates?[0]?.PrivateNote);
            Assert.AreNotEqual(estimate.MetaData.LastUpdatedTime, modRs.Estimates[0].MetaData.LastUpdatedTime);
            #endregion
        }

        [TestMethod]
        [Ignore]
        public async Task Step_4_QBOEstimateEmailTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Estimate
            Random rdm = new();
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Estimate"));
            EstimateOnlineRs estimateRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Emailing Estimate
            if (estimateRs.TotalEstimates <= 0) Assert.Inconclusive($"No {testName} to email.");

            EstimateDto estimate = estimateRs.Estimates.FirstOrDefault(cm => cm.PrivateNote?.StartsWith(testName) ?? false);
            if (estimate == null) Assert.Inconclusive($"{testName} does not exist.");
            
            HttpResponseMessage postRs = await qboe.QBOPost($"/v3/company/{qboe.ClientInfo.RealmId}/estimate/{estimate.Id}/send?sendTo=sfla_gp@yahoo.com");
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            EstimateOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(estimate.PrivateNote, modRs.Estimates?[0]?.PrivateNote);
            #endregion
        }

        [TestMethod]
        public async Task Step_5_QBOEstimateDeleteTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Estimate
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Estimate"));
            if (!getRs.IsSuccessStatusCode) Assert.Inconclusive($"Could not retrieve Estimate to delete: {await getRs.Content.ReadAsStringAsync()}");
            EstimateOnlineRs estimateRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Deleting Estimate
            if (estimateRs.TotalEstimates <= 0) Assert.Inconclusive($"No {testName} to delete.");

            EstimateDto estimate = estimateRs.Estimates.FirstOrDefault(pmt => pmt.PrivateNote?.StartsWith(testName) ?? false);
            if (estimate == null) Assert.Inconclusive($"{testName} does not exist.");

            DeleteRq delRq = new("Estimate", estimate.Id, estimate.SyncToken);
            
            HttpResponseMessage postRs = await qboe.QBOPost(delRq.ApiParameter(qboe.ClientInfo.RealmId), delRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            EstimateOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(EntityStatus.Deleted, modRs.Estimates[0].status, $"Estimate status not Deleted: {modRs.Estimates[0].status}");
            #endregion
        }
    }
}
