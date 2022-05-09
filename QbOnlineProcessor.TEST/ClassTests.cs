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
    public class TestClassModels
    {
        [TestMethod]
        public async Task Step_1_QBOClassQueryTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Classes
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet($"/v3/company/{qboe.ClientInfo.RealmId}/query?query=select * from Class", false);
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"QBOGet failed: {await getRs.Content.ReadAsStringAsync()}");

            string qryRs = await getRs.Content.ReadAsStringAsync();
            ClassOnlineRs ClassRs = new(qryRs);
            Assert.IsNull(ClassRs.ParseError);
            Assert.AreNotEqual(0, ClassRs.TotalClasses);
            #endregion
        }

        [TestMethod]
        public async Task Step_2_QBOClassAddTest()
        {
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Classes
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Class"));
            if (!getRs.IsSuccessStatusCode) Assert.Fail($"Error querying Class: {await getRs.Content.ReadAsStringAsync()}");
            ClassOnlineRs acctRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Adding Class
            if (acctRs.Classes.Any(c => c.FullyQualifiedName?.StartsWith("IMS Class") ?? false)) Assert.Inconclusive("IMS Class already exists.");

            ClassAddRq addRq = new();
            addRq.Name = "IMS Class";
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
            #region Setting access token
            TestAccessToken accessToken = new();
            await accessToken.AccessTokenTest();
            #endregion

            using QBOProcessor qboe = new();

            #region Getting Class
            Random rdm = new();
            if (string.IsNullOrEmpty(qboe.AccessToken.AccessToken)) Assert.Fail("Token not valid.");
            HttpResponseMessage getRs = await qboe.QBOGet(QueryRq.QueryParameter(qboe.ClientInfo.RealmId, "select * from Class"));
            ClassOnlineRs ClassRs = new(await getRs.Content.ReadAsStringAsync());
            #endregion

            #region Updating Class
            if (ClassRs.TotalClasses <= 0) Assert.Fail("No Class to update.");
            ClassDto cls = ClassRs.Classes.FirstOrDefault(c => c.FullyQualifiedName.StartsWith("IMS Class"));
            if (cls == null) Assert.Inconclusive($"IMS Class does not exist.");
            ClassModRq modRq = new();
            modRq.sparse = "true";
            modRq.Id = cls.Id;
            modRq.SyncToken = cls.SyncToken;
            modRq.Name = cls.Name;
            modRq.FullyQualifiedName = $"IMS Class => {cls.SyncToken}";
            if (!modRq.IsEntityValid()) Assert.Fail($"modRq is invalid: {modRq.GetErrorsAsString()}");
            HttpResponseMessage postRs = await qboe.QBOPost(modRq.ApiParameter(qboe.ClientInfo.RealmId), modRq);
            if (!postRs.IsSuccessStatusCode) Assert.Fail($"QBOPost failed: {await postRs.Content.ReadAsStringAsync()}");

            ClassOnlineRs modRs = new(await postRs.Content.ReadAsStringAsync());
            Assert.AreEqual(cls.Name, modRs.Classes?[0]?.Name);
            #endregion
        }
    }
}
