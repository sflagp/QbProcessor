using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace QbModels.QbProcessor.TEST
{
    [TestClass]
    public class CompanyTest
    {
        [TestMethod]
        public void TestClassModels()
        {
            using (RequestProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                CompanyRs qryRs;
                #endregion

                #region Query Test
                CompanyQueryRq qryRq = new();
                Assert.IsTrue(qryRq.IsEntityValid());

                string rs = QB.ExecuteQbRequest(qryRq);
                qryRs = new(rs);
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                Assert.IsTrue(string.IsNullOrEmpty(qryRs.ParseError));
                #endregion
            }
            Thread.Sleep(2000);
        }
    }
}
