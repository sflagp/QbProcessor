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
            Assert.IsTrue(await qboe.GetEndpointsAsync());
            bool tokenCreated = await qboe.RefreshAccessTokenAsync();
            if (!tokenCreated)
            {
                if (await qboe.GetAuthCodesAsync())
                {
                    Assert.IsTrue(File.Exists(authFile));
                    try
                    {
                        var authCode = File.ReadAllText(authFile);
                        tokenCreated = await qboe.SetAccessTokenAsync(authCode);
                        if (!tokenCreated) Assert.Fail("Token not created");
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail($"Error {ex.HResult}\r\n{ex.Message}");
                    }
                }
            }

            if (!tokenCreated) Assert.Fail("Did not create or refresh Access Token missing");
            if (qboe.AccessToken.Expires <= DateTime.Now) Assert.Fail($"Access token stale. Expired {qboe.AccessToken.Expires}.");
        }
    }
}
