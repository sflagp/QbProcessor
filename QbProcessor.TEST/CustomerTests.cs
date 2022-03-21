using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels;
using System;
using System.Threading;

namespace QbProcessor.TEST
{
    [TestClass]
    public class CustomerTests
    {
        [TestMethod]
        public void TestCustomerModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbCustomersView qryRs, addRs = new(), modRs;
                CustomerAddRq addRq = new();
                CustomerModRq modRq = new();
                string addRqName = $"QbProcessor";
                #endregion

                #region Query Test
                CustomerQueryRq qryRq = new();
                qryRq.NameFilter = new() { Name = addRqName, MatchCriterion = "StartsWith" };
                qryRq.ActiveStatus = "All";
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = QB.ToView<QbCustomersView>(QB.ExecuteQbRequest(qryRq));
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                #endregion

                #region Add Test
                if (qryRs.TotalCustomers == 0)
                {
                    addRq.Name = addRqName;
                    addRq.IsActive = true;
                    addRq.BillAddress = new()
                    {
                        Addr1 = "3648 Kapalua Way",
                        City = "Raleigh",
                        State = "NC",
                        PostalCode = "27610"
                    };
                    addRq.Phone = "305-775-4754";
                    addRq.Notes = addRq.GetType().Name;
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = QB.ToView<QbCustomersView>(QB.ExecuteQbRequest(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");

                }
                #endregion

                #region Mod Test
                CustomerRetDto acct = qryRs.TotalCustomers == 0 ? addRs.Customers[0] : qryRs.Customers[0];
                modRq.ListID = acct.ListID;
                modRq.EditSequence = acct.EditSequence;
                modRq.FirstName = "Greg";
                modRq.LastName = "Prieto";
                modRq.Notes = $"{modRq.GetType().Name} on {DateTime.Now}";
                modRq.IsActive = true;
                Assert.IsTrue(modRq.IsEntityValid());

                modRq.CreditCardInfo = new();
                modRq.CreditCardInfo.CreditCardNumber = "xxxx-xxxx-xxxx-9651";
                modRq.CreditCardInfo.ExpirationMonth = 11;
                modRq.CreditCardInfo.ExpirationYear = 22;
                modRq.CreditCardInfo.NameOnCard = "Greg Prieto";
                modRq.CreditCardInfo.CreditCardAddress = "3648 Kapalua Way, Raleigh NC";
                modRq.CreditCardInfo.CreditCardPostalCode = "27610";
                modRq.CreditLimit = 10000M;
                Assert.IsTrue(modRq.IsEntityValid());

                modRs = QB.ToView<QbCustomersView>(QB.ExecuteQbRequest(modRq));
                Assert.IsTrue(modRs.StatusCode == "0");
                #endregion
            }
            Thread.Sleep(2000);
        }

        [TestMethod]
        public void TestCustomerTypeModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbCustomerTypesView qryRs, addRs;
                CustomerTypeAddRq addRq = new();
                string addRqName = $"QbProcessor";
                #endregion

                #region Query Test
                CustomerTypeQueryRq qryRq = new();
                qryRq.NameFilter = new() { Name = addRqName, MatchCriterion = "StartsWith" };
                qryRq.ActiveStatus = "All";
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = QB.ToView<QbCustomerTypesView>(QB.ExecuteQbRequest(qryRq));
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                #endregion

                #region Add Test
                if (qryRs.TotalCustomerTypes == 0)
                {
                    addRq.Name = addRqName;
                    addRq.IsActive = true;
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = QB.ToView<QbCustomerTypesView>(QB.ExecuteQbRequest(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");
                }
                #endregion
            }
            Thread.Sleep(2000);
        }

        [TestMethod]
        public void TestCustomerMsgModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbCustomerMsgsView qryRs, addRs;
                CustomerMsgAddRq addRq = new();
                string addRqName = $"QbProcessor";
                #endregion

                #region Query Test
                CustomerMsgQueryRq qryRq = new();
                qryRq.NameFilter = new() { Name = addRqName, MatchCriterion = "StartsWith" };
                qryRq.ActiveStatus = "All";
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = QB.ToView<QbCustomerMsgsView>(QB.ExecuteQbRequest(qryRq));
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                #endregion

                #region Add Test
                if (qryRs.TotalCustomerMsgs == 0)
                {
                    addRq.Name = addRqName;
                    addRq.IsActive = true;
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = QB.ToView<QbCustomerMsgsView>(QB.ExecuteQbRequest(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");
                }
                #endregion
            }
            Thread.Sleep(2000);
        }
    }
}
