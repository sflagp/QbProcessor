using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
            QBOProcessor.SetClientInfo();
            #endregion

            using QBOProcessor qboe = new();
            Assert.IsTrue(await qboe.GetEndpointsAsync());
            bool tokenCreated = await qboe.RefreshAccessTokenAsync();
            if (!tokenCreated)
            {
                string authCodeResponse = await qboe.GetAuthCodesAsync();
                if (authCodeResponse != null)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(authCodeResponse), "Response AuthCode is not valid.");
                    try
                    {
                        tokenCreated = await qboe.SetAccessTokenAsync(authCodeResponse);
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
