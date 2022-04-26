using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
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
            string clientFile = @".\QboeInfo.json";
            string tokenFile = @".\AccessToken.json";
            string authFile = @".\GetAuthCode.html";
            string manualAuthCode = "";

            if (!File.Exists(clientFile)) Assert.Fail("Client info file missing");
            string clientInfo = await File.ReadAllTextAsync(clientFile);
            QBOProcessor.SetClientInfo(clientInfo);
            #endregion

            using QBOProcessor qboe = new();

            Assert.IsTrue(await qboe.GetEndpointsAsync());
            if (await qboe.GetAuthCodesAsync())
            {
                Assert.IsTrue(File.Exists(authFile));
                try
                {
                    #region Load webpage
                    Process p = new();
                    p.StartInfo = new ProcessStartInfo(authFile)
                    {
                        UseShellExecute = true
                    };
                    //p.Start();
                    #endregion
                    if (!string.IsNullOrEmpty(manualAuthCode))
                    {
                        bool tokenCreated = await qboe.SetAccessTokenAsync(manualAuthCode);
                        if (!tokenCreated) Assert.Fail("Token not created");
                    }
                    if (File.Exists(tokenFile))
                    {
                        QboAccessToken token = JsonSerializer.Deserialize<QboAccessToken>(await File.ReadAllTextAsync(tokenFile));
                        qboe.ManualAccessToken(token);
                        await qboe.RefreshAccessTokenAsync();
                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Error {ex.HResult}\r\n{ex.Message}");
                }
            }
            if (qboe.AccessToken == null) Assert.Fail("Access Token missing");
            if(qboe.AccessToken.Expires <= DateTime.Now) Assert.Fail($"Access token stale. Expired {qboe.AccessToken.Expires}.");
        }
    }
}
