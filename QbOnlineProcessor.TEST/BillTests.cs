using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using QbModels.QBO.ENUM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestBillModels
    {
        readonly string testName = "IMS Bill";

        [TestMethod]
        public async Task Step_1_QBOBillAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Bill
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from Bill where PrivateNote='{testName}'"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying bill: {await getRs.Content.ReadAsStringAsync()}");
            BillOnlineRs acctRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding Bill
            if (acctRs.TotalBills == 0)
            {
                Random rdm = new();
                HttpResponseMessage vendQryRq = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Vendor"));
                if (!vendQryRq.IsSuccessStatusCode) Assert.Fail($"Error retrieving vendors.\n{await vendQryRq.Content.ReadAsStringAsync()}");
                VendorOnlineRs vendorRs = new(await vendQryRq.Content.ReadAsStringAsync());
                VendorDto vendor = vendorRs.Vendors.ElementAt(rdm.Next(0, vendorRs.TotalVendors));
                
                BillAddRq addRq = new();
                addRq.VendorRef = new(vendor.Id);
                addRq.Line = new() { new()
                {
                    Id = "-1",
                    Amount = 12.34M,
                    DetailType = LineDetailType.ItemBasedExpenseLineDetail,
                    LineDetail = new ItemBasedExpenseLineDetailDto()
                    {
                        ItemRef = new("11", "Pump"),
                        UnitPrice = 10M,
                        Qty = 8M,
                        TaxCodeRef = new("NON"),
                        BillableStatus = BillableStatus.NotBillable
                    }
                } };
                addRq.PrivateNote = testName;
                if (!addRq.IsEntityValid()) Assert.Fail($"addRq is invalid: {addRq.GetErrorsAsString()}");
                HttpResponseMessage postRs = await qboe.QBOPost(addRq.ApiParameter(qboe.ClientInfo.RealmId), addRq);
                if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

                BillOnlineRs addRs = new(await postRs.Content.ReadAsStringAsync());
                Assert.AreEqual(1, addRs.TotalBills);
            }
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOBillQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Bills
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from Bill", true);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            BillOnlineRs acctRs = new(qryRs);
            Assert.IsNull(acctRs.ParseError);
            Assert.AreNotEqual(0, acctRs.TotalBills);

            List<BillDto> imsAccts = acctRs.Bills.Where(a => a.PrivateNote?.Contains("IMS") ?? false)?.ToList();
            if (imsAccts.Count == 0) Assert.Fail($"No {testName} found");
            #endregion
        }

        [TestMethod]
        public async Task Step_3_QBOBillModTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Bill
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, $"select * from Bill where PrivateNote = '{testName}'"));
            BillOnlineRs acctRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating Bill
            if (acctRs.TotalBills <= 0) Assert.Fail($"No {testName} to update.");
            
            BillDto bill = acctRs.Bills.FirstOrDefault();
            if (bill == null) Assert.Fail($"{testName} does not exist.");
            
            BillModRq modRq = new();
            modRq.sparse = "true";
            modRq.Id = bill.Id;
            modRq.SyncToken = bill.SyncToken;
            modRq.Line = bill.Line;
            modRq.VendorRef = bill.VendorRef;
            modRq.DocNumber = $"{testName} Test => {bill.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            BillOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreNotEqual(bill.DocNumber, modRs.Bills?[0]?.DocNumber);
            #endregion
        }

        [TestMethod]
        public async Task Step_4_QBOBillDeleteTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Bill
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Bill where PrivateNote = 'IMS Bill'"));
            BillOnlineRs acctRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Deleting Bill
            if (acctRs.TotalBills <= 0) Assert.Fail($"No {testName} to delete.");
            BillDto bill = acctRs.Bills.FirstOrDefault();
            if (bill == null) Assert.Fail($"{testName} does not exist.");
            BillModRq modRq = new();
            modRq.Id = bill.Id;
            modRq.SyncToken = bill.SyncToken;
            HttpResponseMessage postRs = await qboe.QBOPost($"{modRq.ApiParameter(qboe.ClientInfo.RealmId)}?operation=delete", modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            BillOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual("Deleted", modRs.Bills[0].status, $"{testName} status not Deleted: {modRs.Bills[0].status}");
            #endregion
        }
    }
}
