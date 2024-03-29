using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QBO;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestCompanyModels
    {
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
        public async Task Step_1_QBOCompanyQueryTest()
        {
            #region Getting company info
            Assert.IsFalse(string.IsNullOrEmpty(qboe.AccessToken.AccessToken));
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.CompanyInfo(qboe.ClientInfo.RealmId));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");
            string qryRs = await getRs.Content.ReadAsStringAsync();
            CompanyOnlineRs company = new(qryRs);
            if(!string.IsNullOrEmpty(company.ParseError)) Assert.Fail($"ParseError: {company.ParseError}");
            Assert.AreEqual("(954) 925-1900", company.CompanyInfo.PrimaryPhone.FreeFormNumber);
            Assert.AreEqual("Invoicing Made Simple", company.CompanyInfo.CompanyName);
            Assert.AreEqual("David Becker CPA", company.CompanyInfo.LegalName);
            Assert.AreEqual("Invoicing Made Simple", company.CompanyInfo.CompanyName);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOCompanyModTest()
        {
            #region Updating company
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("AccessToken not valid.");

            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/companyinfo/{qboe.ClientInfo.RealmId}");
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");
            
            string qryRs = await getRs.Content.ReadAsStringAsync();
            CompanyOnlineRs company = new(qryRs);
            
            CompanyModRq modRq = new();
            modRq.CopyDto(company.CompanyInfo);
            modRq.sparse = "true";
            modRq.CompanyName = "Invoicing Made Simple";
            modRq.LegalName = "David Becker CPA";
            modRq.CompanyAddr.Line1 = "3874 Sheridan Street";
            modRq.CompanyAddr.City = "Hollywood";
            modRq.CompanyAddr.CountrySubDivisionCode = "FL";
            modRq.CompanyAddr.Country = "US";
            modRq.CompanyAddr.PostalCode = "33021";
            modRq.PrimaryPhone = new() { DeviceType = "LandLine", FreeFormNumber = "(954) 925-1900" };
            modRq.LegalAddr.Line1 = "3874 Sheridan Street";
            modRq.LegalAddr.City = "Hollywood";
            modRq.LegalAddr.CountrySubDivisionCode = "FL";
            modRq.LegalAddr.Country = "US";
            modRq.LegalAddr.PostalCode = "33021";
            modRq.Email.Address = "DavidBeckerCPA@gmail.com";
            modRq.WebAddr = company.CompanyInfo.WebAddr;
            modRq.WebAddr.URI = "https://www.invoicingmadesimple.com";
            
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");
            
            CompanyOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual("(954) 925-1900", modRs.CompanyInfo.PrimaryPhone.FreeFormNumber);
            Assert.AreEqual("Invoicing Made Simple", modRs.CompanyInfo.CompanyName);
            Assert.AreEqual("David Becker CPA", modRs.CompanyInfo.LegalName);
            #endregion
        }
    }
}
