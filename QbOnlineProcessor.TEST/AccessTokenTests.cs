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
            string authFile = @".\GetAuthCode.txt";

            QBOProcessor.SetClientInfo();
            #endregion

            using QBOProcessor qboe = new();
            if (qboe.GetNewAuthCode)
            {
                Assert.IsTrue(await qboe.GetEndpointsAsync());
                if (await qboe.GetAuthCodesAsync())
                {
                    Assert.IsTrue(File.Exists(authFile));
                    try
                    {
                        var authCode = File.ReadAllText(authFile);
                        bool tokenCreated = await qboe.SetAccessTokenAsync(authCode);
                        if (!tokenCreated) Assert.Fail("Token not created");
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail($"Error {ex.HResult}\r\n{ex.Message}");
                    }
                }
            }
            else
            {
                await qboe.RefreshAccessTokenAsync();
            }

            if (qboe.AccessToken == null) Assert.Fail("Access Token missing");
            if (qboe.AccessToken.Expires <= DateTime.Now) Assert.Fail($"Access token stale. Expired {qboe.AccessToken.Expires}.");
        }
    }
}
