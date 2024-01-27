using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using QbModels.QBO.ENUM;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestBatchRequestModels
    {
        readonly string testName = "IMS BatchRequest";
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
        public async Task Step_1_QBOBatchRequestTest()
        {
            #region Creating Batch Query Request
            BatchRequestsRq batchQueryRq = new();
            batchQueryRq.AddQuery("select * from Account");
            batchQueryRq.AddQuery("select * from Customer");
            batchQueryRq.AddQuery("select * from Employee");
            batchQueryRq.AddQuery("select * from Vendor");
            Assert.AreEqual(4, batchQueryRq.BatchItemRequest.Count);

            HttpResponseMessage postBatchQryRs = await qboe.QBOPost(batchQueryRq.ApiParameter(qboe.ClientInfo.RealmId), batchQueryRq);
            if (!postBatchQryRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postBatchQryRs.Content.ReadAsStringAsync()}");

            string qryRs = await postBatchQryRs.Content.ReadAsStringAsync();
            BatchRequestOnlineRs batchQryRs = new(qryRs);
            Assert.IsNull(batchQryRs.ParseError, $"Parse error for Query BatchItemRequest => {batchQryRs.ParseError}");
            #endregion

            #region Creating Batch Mod Request
            AccountDto acct = batchQryRs.GetResults<AccountDto>().FirstOrDefault(a => a.Name.Equals("IMS Account"));
            AccountModRq modAcctRq = new();
            modAcctRq.CopyDto(acct);
            modAcctRq.sparse = "true";
            modAcctRq.Description = $"{testName} Test => {acct.SyncToken}";

            CustomerDto cust = batchQryRs.GetResults<CustomerDto>().FirstOrDefault(c => c.FullyQualifiedName.Equals("IMS Customer"));
            CustomerModRq modCustRq = new();
            modCustRq.CopyDto(cust);
            modCustRq.sparse = "true";
            modCustRq.Notes = $"{testName} Test => {cust.SyncToken}";

            EmployeeDto emp = batchQryRs.GetResults<EmployeeDto>().FirstOrDefault(e => e.FamilyName.Equals("IMS Employee"));
            EmployeeModRq modEmpRq = new();
            modEmpRq.CopyDto(emp);
            modEmpRq.sparse = "true";
            modEmpRq.PrintOnCheckName = $"{testName} Test => {emp.SyncToken}";

            VendorDto vend = batchQryRs.GetResults<VendorDto>().FirstOrDefault(v => v.DisplayName.Equals("IMS Vendor"));
            VendorModRq modVendRq = new();
            modVendRq.CopyDto(vend);
            modVendRq.sparse = "true";
            modVendRq.AcctNum = $"{testName} Test => {vend.SyncToken}";

            BatchRequestsRq batchModRq = new();
            batchModRq.AddRequest(modAcctRq, ItemChoiceType8.Account, Operation.update);
            batchModRq.AddQuery("select * from Account");
            batchModRq.AddRequest(modCustRq, ItemChoiceType8.Customer, Operation.update);
            batchModRq.AddQuery("select * from Customer");
            batchModRq.AddRequest(modEmpRq, ItemChoiceType8.Employee, Operation.update);
            batchModRq.AddQuery("select * from Employee");
            batchModRq.AddRequest(modVendRq, ItemChoiceType8.Vendor, Operation.update);
            batchModRq.AddQuery("select * from Vendor");
            Assert.AreEqual(8, batchModRq.BatchItemRequest.Count);

            HttpResponseMessage postBatchModRs = await qboe.QBOPost(batchQueryRq.ApiParameter(qboe.ClientInfo.RealmId), batchModRq);
            if (!postBatchModRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postBatchModRs.Content.ReadAsStringAsync()}");

            string modRs = await postBatchModRs.Content.ReadAsStringAsync();
            BatchRequestOnlineRs batchModRs = new(modRs);
            Assert.IsNull(batchModRs.ParseError, $"Parse error for Mod BatchItemRequest {batchModRs.ParseError}");
            Assert.AreNotEqual(0, batchModRs.TotalBatchResponses, $"Number of batch responses does not match expected results.");
            #endregion
        }
    }
}
