using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace QbProcessor.TEST
{
    [TestClass]
    public class Z_CleanupTests
    {
        [TestMethod]
        //[Ignore("Do not cleanup transactions")]
        public void TestCleanupModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                TransactionQueryRq qryRq;
                QbTransactionsView qryRs;
                Regex validCodes = new(@"^0$|^1$");
                Regex validDelCodes = new(@"^0$|^3160$");
                #endregion

                #region Query test
                qryRq = new();
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRq.TransactionModifiedDateRangeFilter = new() 
                { 
                    FromModifiedDate = DateTime.Today.AddDays(-7), 
                    ToModifiedDate = DateTime.Today 
                };
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = new(QB.ExecuteQbRequest(qryRq));
                Assert.IsTrue(validCodes.IsMatch(qryRs.StatusCode));

                if (qryRs.TotalTransactions == 0) return;
                #endregion

                #region Cleanup transactions
                Assert.AreNotEqual(0, qryRs.TotalTransactions);
                foreach(TransactionRetDto t in qryRs.Transactions)
                {
                    TxnDelRq delRq = new() { TxnDelType = t.TxnType, TxnID = t.TxnID };
                    Assert.IsTrue(delRq.IsEntityValid());

                    string result = QB.ExecuteQbRequest(delRq);
                    QbTxnDelView delRs = new(result);
                    Assert.IsTrue(validDelCodes.IsMatch(delRs.StatusCode));
                }
                #endregion
            }
            Thread.Sleep(2000);
        }
    }
}
