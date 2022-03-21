using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace QbProcessor.TEST
{
    [TestClass]
    public class ShipMethodTests
    {
        [TestMethod]
        public void TestShipMethodModel()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbShipMethodsView qryRs, addRs;
                ShipMethodQueryRq qryRq;
                ShipMethodAddRq addRq;

                string addRqName = "QbProcessor";
                string result;
                #endregion

                #region Query Test
                qryRq = new();
                Assert.IsTrue(qryRq.IsEntityValid());

                result = QB.ExecuteQbRequest(qryRq);
                qryRs = QB.ToView<QbShipMethodsView>(result);
                Regex statusCodes =  new(@"\b0\b|\b3250\b");
                Assert.IsTrue(statusCodes.IsMatch(qryRs.StatusCode));
                if (qryRs.StatusCode == "3250") Assert.Inconclusive(qryRs.StatusMessage);
                #endregion

                #region Add Test
                if (qryRs.TotalShipMethods == 0)
                {
                    addRq = new()
                    {
                        Name = addRqName,
                        IsActive = true
                    };
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = QB.ToView<QbShipMethodsView>(QB.ExecuteQbRequest(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");
                    Assert.IsTrue(addRs.TotalShipMethods > 0);
                }
                #endregion
            }
            Thread.Sleep(2000);
        }
    }
}
