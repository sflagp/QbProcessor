using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels;
using System;
using System.Threading;

namespace QbProcessor.TEST
{
    [TestClass]
    public class ClassTests
    {
        [TestMethod]
        public void TestClassModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbClassesView qryRs, addRs = new(), modRs;
                ClassAddRq addRq = new();
                ClassModRq modRq = new();
                string addRqName = $"QbProcessor Class";
                #endregion

                #region Query Test
                ClassQueryRq qryRq = new();
                qryRq.NameFilter = new() { Name = addRqName, MatchCriterion = "StartsWith" };
                qryRq.ActiveStatus = "All";
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = QB.ToView<QbClassesView>(QB.ExecuteQbRequest(qryRq));
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                #endregion

                #region Add Test
                if (qryRs.TotalClasses == 0)
                {
                    addRq.Name = addRqName;
                    addRq.IsActive = true;

                    addRs = QB.ToView<QbClassesView>(QB.ExecuteQbRequest(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");
                }
                #endregion

                #region Mod Test
                ClassRetDto acct = qryRs.TotalClasses == 0 ? addRs.Classes[0] : qryRs.Classes[0];
                modRq.ListID = acct.ListID;
                modRq.EditSequence = acct.EditSequence;
                modRq.Name = acct.Name;
                Assert.IsTrue(modRq.IsEntityValid());

                modRs = QB.ToView<QbClassesView>(QB.ExecuteQbRequest(modRq));
                Assert.IsTrue(modRs.StatusCode == "0");
                #endregion
            }
            Thread.Sleep(2000);
        }
    }
}
