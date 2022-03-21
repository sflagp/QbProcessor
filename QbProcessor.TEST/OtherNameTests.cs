using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels;
using System;
using System.Threading;

namespace QbProcessor.TEST
{
    [TestClass]
    public class OtherNameTests
    {
        [TestMethod]
        public void TestOtherNameModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbOtherNamesView qryRs, addRs = new(), modRs;
                OtherNameAddRq addRq = new();
                OtherNameModRq modRq = new();
                string addRqName = $"QbProcessor {addRq.GetType().Name}";
                #endregion

                #region Query Test
                OtherNameQueryRq qryRq = new();
                qryRq.NameFilter = new() { Name = addRqName, MatchCriterion = "StartsWith" };
                qryRq.ActiveStatus = "All";
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = QB.ToView<QbOtherNamesView>(QB.ExecuteQbRequest(qryRq));
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                #endregion

                #region Add Test
                if (qryRs.TotalOtherNames == 0)
                {
                    addRq.Name = addRqName;
                    addRq.IsActive = true;
                    addRq.FirstName = "Greg";
                    addRq.LastName = "Prieto";
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = QB.ToView<QbOtherNamesView>(QB.ExecuteQbRequest(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");

                }
                #endregion

                #region Mod Test
                string Note = $"{addRqName} updated by {modRq.GetType().Name} on {DateTime.Now}";
                OtherNameRetDto acct = qryRs.TotalOtherNames == 0 ? addRs.OtherNames[0] : qryRs.OtherNames[0];
                modRq.ListID = acct.ListID;
                modRq.EditSequence = acct.EditSequence;
                modRq.Name = acct.Name;
                modRq.Notes = Note;
                Assert.IsTrue(modRq.IsEntityValid());

                modRs = QB.ToView<QbOtherNamesView>(QB.ExecuteQbRequest(modRq));
                Assert.IsTrue(modRs.StatusCode == "0");
                Assert.IsTrue(modRs.OtherNames[0].Notes == Note);

                modRq.ListID = modRs.OtherNames[0].ListID;
                modRq.EditSequence = modRs.OtherNames[0].EditSequence;
                modRq.Email = "greg.prieto@ncsecu.org";
                Assert.IsTrue(modRq.IsEntityValid());

                modRs = QB.ToView<QbOtherNamesView>(QB.ExecuteQbRequest(modRq));
                Assert.IsTrue(modRs.StatusCode == "0");
                #endregion
            }
            Thread.Sleep(2000);
        }
    }
}
