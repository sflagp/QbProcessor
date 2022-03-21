using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels;
using System;
using System.Threading;

namespace QbProcessor.TEST
{
    [TestClass]
    public class PriceLevelTests
    {
        [TestMethod]
        public void TestPriceLevelModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbPriceLevelsView qryRs, addRs = new(), modRs;
                PriceLevelAddRq addRq = new();
                PriceLevelModRq modRq = new();
                string addRqName = $"QbProcessor {addRq.GetType().Name}";
                string result;
                #endregion

                #region Query Test
                PriceLevelQueryRq qryRq = new();
                qryRq.NameFilter = new() { Name = addRqName, MatchCriterion = "StartsWith" };
                qryRq.ActiveStatus = "All";
                Assert.IsTrue(qryRq.IsEntityValid());

                result = QB.ExecuteQbRequest(qryRq);
                qryRs = QB.ToView<QbPriceLevelsView>(result);
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                #endregion

                #region Add Test
                if (qryRs.TotalPriceLevels == 0)
                {
                    addRq.Name = addRqName;
                    addRq.IsActive = true;
                    addRq.PriceLevelFixedPercentage = "10";
                    Assert.IsTrue(addRq.IsEntityValid());

                    result = QB.ExecuteQbRequest(addRq);
                    addRs = QB.ToView<QbPriceLevelsView>(result);
                    Assert.IsTrue(addRs.StatusCode == "0");
                    Assert.IsTrue(addRs.TotalPriceLevels == 1);
                    Assert.IsTrue(addRs.PriceLevels[0].PriceLevelFixedPercentage == "10.00");

                }
                #endregion

                #region Mod Test
                PriceLevelRetDto acct = qryRs.TotalPriceLevels == 0 ? addRs.PriceLevels[0] : qryRs.PriceLevels[0];
                modRq.ListID = acct.ListID;
                modRq.EditSequence = acct.EditSequence;
                modRq.Name = acct.Name;
                modRq.PriceLevelFixedPercentage = "15";
                Assert.IsTrue(modRq.IsEntityValid());

                result = QB.ExecuteQbRequest(modRq);
                modRs = QB.ToView<QbPriceLevelsView>(result);
                Assert.IsTrue(modRs.StatusCode == "0");
                Assert.IsTrue(modRs.TotalPriceLevels == 1);
                Assert.IsTrue(modRs.PriceLevels[0].PriceLevelFixedPercentage == "15.00");

                #endregion
            }
            Thread.Sleep(2000);
        }
    }
}
