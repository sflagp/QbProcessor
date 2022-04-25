using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels.QbOnline;
using System.Net.Http;
using System.Threading.Tasks;

namespace QbModels.QbOnlineProcessor.TEST
{
    [TestClass]
    public class TestCompanyModels
    {
        [TestMethod]
        public async Task QboeCompanyTest()
        {
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            using (QbOnlineProcessor qboe = new())
            {
                Assert.IsFalse(string.IsNullOrEmpty(qboe.AccessToken.AccessToken));
                HttpResponseMessage getRs = await qboe.QbOnlineGet($"/v3/company/{qboe.ClientInfo.RealmId}/companyinfo/{qboe.ClientInfo.RealmId}");
                string qryRs = await getRs.Content.ReadAsStringAsync();
                CompanyOnlineRs company = new(qryRs);
                Assert.IsNull(company.ParseError);
                Assert.AreEqual("Invoicing Made Simple", company.CompanyInfo.CompanyName);
                CompanyModRq modRq = new();
                modRq.sparse = "true";
                modRq.Id = company.CompanyInfo.Id;
                modRq.SyncToken = company.CompanyInfo.SyncToken;
                modRq.CompanyName = "Invoicing Made Simple";
                modRq.LegalName = company.CompanyInfo.LegalName;
                modRq.CompanyAddr = company.CompanyInfo.CompanyAddr;
                modRq.CompanyAddr.Line1 = "3648 Kapalua Way";
                modRq.CompanyAddr.City = "Raleigh";
                modRq.CompanyAddr.CountrySubDivisionCode = "NC";
                modRq.CompanyAddr.Country = "US";
                modRq.CompanyAddr.PostalCode = "27610";
                modRq.PrimaryPhone = new() { DeviceType = "Mobile", FreeFormNumber = "919-555-4754" };
                modRq.LegalAddr = company.CompanyInfo.LegalAddr;
                modRq.Email = company.CompanyInfo.Email;
                modRq.WebAddr = company.CompanyInfo.WebAddr;
                modRq.MetaData = company.CompanyInfo.MetaData;
                HttpResponseMessage postRs = await qboe.QbOnlinePost<CompanyModRq>($"/v3/company/{qboe.ClientInfo.RealmId}/companyinfo", modRq);
                Assert.IsTrue(postRs.IsSuccessStatusCode);
                CompanyOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
                Assert.AreEqual("919-555-4754", modRs.CompanyInfo.PrimaryPhone.FreeFormNumber);
            }
        }
    }
}
