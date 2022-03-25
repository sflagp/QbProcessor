using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace QbProcessor.TEST
{
    [TestClass]
    public class CurrencyTests
    {
        [TestMethod]
        public void TestCurrencyModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbCurrencysView qryRs, addRs = new(), modRs;
                CurrencyAddRq addRq = new();
                CurrencyModRq modRq = new();
                string addRqName = $"QPD";
                #endregion

                #region Query Test
                CurrencyQueryRq qryRq = new();
                qryRq.NameFilter = new() { Name = addRqName, MatchCriterion = "StartsWith" };
                qryRq.ActiveStatus = "All";
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = QB.ToView<QbCurrencysView>(QB.ExecuteQbRequest(qryRq));
                Regex statusCodes = new(@"^0$|^3250$");
                Assert.IsTrue(statusCodes.IsMatch(qryRs.StatusCode));
                if (qryRs.StatusCode == "3250") Assert.Inconclusive(qryRs.StatusMessage);
                #endregion

                #region Add Test
                if (qryRs.TotalCurrencys == 0)
                {
                    addRq.Name = addRqName;
                    addRq.IsActive = true;
                    addRq.CurrencyCode = addRqName;
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = QB.ToView<QbCurrencysView>(QB.ExecuteQbRequest(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");

                }
                #endregion

                #region Mod Test
                CurrencyRetDto acct = qryRs.TotalCurrencys == 0 ? addRs.Currencys[0] : qryRs.Currencys[0];
                modRq.ListID = acct.ListID;
                modRq.EditSequence = acct.EditSequence;
                modRq.Name = acct.Name;
                modRq.CurrencyFormat = new() { ThousandSeparator = "Comma" };
                Assert.IsTrue(modRq.IsEntityValid());

                modRs = QB.ToView<QbCurrencysView>(QB.ExecuteQbRequest(modRq));
                Assert.IsTrue(modRs.StatusCode == "0");
                #endregion
            }
            Thread.Sleep(2000);
        }
    }
}
