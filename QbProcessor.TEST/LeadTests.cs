using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels;
using System;
using System.Threading;

namespace QbProcessor.TEST
{
    [TestClass]
    public class LeadTests
    {
        [TestMethod]
        public void TestLeadModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbLeadsView qryRs, addRs = new(), modRs;
                LeadAddRq addRq = new();
                LeadModRq modRq = new();
                string addRqName = $"QbProcessor";
                string result;
                #endregion

                #region Query Test
                LeadQueryRq qryRq = new();
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRq.NameFilter = new() { Name = addRqName, MatchCriterion = "StartsWith" };
                Assert.IsTrue(qryRq.IsEntityValid());

                result = QB.ExecuteQbRequest(qryRq);
                qryRs = QB.ToView<QbLeadsView>(result);
                if (qryRs.StatusCode == "3231") Assert.Inconclusive(qryRs.StatusMessage);
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                #endregion

                #region Add Test
                if (qryRs.TotalLeads == 0)
                {
                    addRq.FullName = $"{addRqName}.{addRq.GetType().Name}";
                    addRq.Status = "Cold";
                    addRq.MainPhone = "305-775-4754";
                    Assert.IsTrue(addRq.IsEntityValid());

                    result = QB.ExecuteQbRequest(addRq);
                    addRs = QB.ToView<QbLeadsView>(result);
                    if (addRs.StatusCode == "3250") Assert.Inconclusive(addRs.StatusMessage);
                    Assert.IsTrue(addRs.StatusCode == "0");
                }
                #endregion

                #region Mod Test
                LeadRetDto Lead = qryRs.TotalLeads == 0 ? addRs.Leads[0] : qryRs.Leads[0];
                modRq.ListID = Lead.ListID;
                modRq.EditSequence = Lead.EditSequence;
                modRq.FullName = $"{addRqName}.{modRq.GetType().Name}";
                modRq.Status = "Hot";
                Assert.IsTrue(modRq.IsEntityValid());

                result = QB.ExecuteQbRequest(modRq);
                modRs = QB.ToView<QbLeadsView>(result);
                Assert.IsTrue(modRs.StatusCode == "0");
                #endregion
            }
            Thread.Sleep(2000);
        }
    }
}
