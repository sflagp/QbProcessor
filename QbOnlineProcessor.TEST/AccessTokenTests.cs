using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace QbModels.QBOProcessor.TEST
{
    [TestClass]
    public class TestAccessToken
    {
        [TestMethod]
        public async Task AccessTokenTest()
        {
            #region Get QBO Access Info
            string authFile = @".\GetAuthCode.html";

            QBOProcessor.SetClientInfo();
            #endregion

            using QBOProcessor qboe = new();
            Assert.IsTrue(await qboe.GetEndpointsAsync());
            if (await qboe.GetAuthCodesAsync())
            {
                Assert.IsTrue(File.Exists(authFile));
                try
                {
                    if (!string.IsNullOrEmpty(qboe.AuthCode))
                    {
                        bool tokenCreated = await qboe.SetAccessTokenAsync(qboe.AuthCode);
                        if (!tokenCreated) Assert.Fail("Token not created");
                    }
                    if (qboe.AccessToken.ShouldRefresh)
                    {
                        await qboe.RefreshAccessTokenAsync();
                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Error {ex.HResult}\r\n{ex.Message}");
                }
            }
            if (qboe.AccessToken == null) Assert.Fail("Access Token missing");
            if (qboe.AccessToken.Expires <= DateTime.Now) Assert.Fail($"Access token stale. Expired {qboe.AccessToken.Expires}.");
        }
    }
}
