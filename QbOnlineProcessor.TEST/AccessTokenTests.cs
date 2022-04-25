using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QbModels.QbOnlineProcessor.TEST
{
    [TestClass]
    public class TestAccessToken
    {
        [TestMethod]
        public async Task AccessTokenTest()
        {
            #region Get QbOnline Access Info
            string clientFile = @".\QboeInfo.json";
            string tokenFile = @".\AccessToken.json";
            string authFile = @".\GetAuthCode.html";
            string authCode = "";

            if (!File.Exists(clientFile)) Assert.Fail("Client info file missing");
            string clientInfo = await File.ReadAllTextAsync(clientFile);
            QbOnlineProcessor.SetClientInfo(clientInfo);
            #endregion

            using (QbOnlineProcessor qboe = new())
            {
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
                        if (string.IsNullOrEmpty(authCode) && File.Exists(tokenFile))
                        {
                            QboeAccessToken token = JsonSerializer.Deserialize<QboeAccessToken>(await File.ReadAllTextAsync(tokenFile));
                            qboe.ManualAccessToken(token.AccessToken, token.RefreshToken);
                            await qboe.RefreshAccessTokenAsync();
                        }
                        if (!string.IsNullOrEmpty(authCode))
                        {
                            bool tokenCreated = await qboe.SetAccessTokenAsync(authCode);
                            Assert.IsTrue(tokenCreated);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error {ex.HResult}\r\n{ex.Message}");
                    }
                }
                if (qboe.AccessToken == null) Assert.Fail("Access Token missing");
                Assert.IsTrue(qboe.AccessToken.Expires > DateTime.Now);
            }
        }
    }
}
