using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels;
using QbModels.ENUM;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QbModels.QbProcessor.TEST
{
    [TestClass]
    public class CustomerTests
    {
        [TestMethod]
        public void TestCustomerModels()
        {
            using (RequestProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                CustomerRs qryRs, addRs = new(""), modRs;
                CustomerAddRq addRq = new();
                CustomerModRq modRq = new();
                string addRqName = $"QbProcessor";
                #endregion

                #region Query Test
                CustomerQueryRq qryRq = new() { MaxReturned = 100, Iterator = "Start" };
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = new(QB.ExecuteQbRequest(qryRq));
                while(qryRs.RemainingCount > 0)
                {
                    qryRq.IteratorID = qryRs.IteratorID;
                    qryRq.Iterator = "Continue";
                    qryRs = new(QB.ExecuteQbRequest(qryRq));
                }
                Assert.AreEqual(0, qryRs.RemainingCount);

                qryRq = new();
                qryRq.NameFilter = new() { Name = addRqName, MatchCriterion = MatchCriterion.StartsWith };
                qryRq.ActiveStatus = "All";
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = new(QB.ExecuteQbRequest(qryRq));
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                Assert.IsTrue(string.IsNullOrEmpty(qryRs.ParseError));
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
                    addRq.PreferredDeliveryMethod = PreferredDeliveryMethod.Email;
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = new(QB.ExecuteQbRequest(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");
                    Assert.IsTrue(string.IsNullOrEmpty(addRs.ParseError));
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
                modRq.CreditCardInfo.ExpirationYear = 27;
                modRq.CreditCardInfo.NameOnCard = "Greg Prieto";
                modRq.CreditCardInfo.CreditCardAddress = "3648 Kapalua Way, Raleigh NC";
                modRq.CreditCardInfo.CreditCardPostalCode = "27610";
                modRq.CreditLimit = 10000M;
                Assert.IsTrue(modRq.IsEntityValid());

                modRs = new(QB.ExecuteQbRequest(modRq));
                Assert.IsTrue(modRs.StatusCode == "0");
                Assert.IsTrue(string.IsNullOrEmpty(modRs.ParseError));
                #endregion
            }
            Thread.Sleep(2000);
        }

        [TestMethod]
        public async Task TestCustomerModelsAsync()
        {
            using (RequestProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                CustomerRs qryRs, addRs = new(""), modRs;
                CustomerAddRq addRq = new();
                CustomerModRq modRq = new();
                string addRqName = $"QbProcessor";
                #endregion

                #region Query Test
                CustomerQueryRq qryRq = new() { MaxReturned = 100, Iterator = "Start" };
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = new(await QB.ExecuteQbRequestAsync(qryRq));
                while (qryRs.RemainingCount > 0)
                {
                    qryRq.IteratorID = qryRs.IteratorID;
                    qryRq.Iterator = "Continue";
                    qryRs = new(await QB.ExecuteQbRequestAsync(qryRq));
                }
                Assert.AreEqual(0, qryRs.RemainingCount);

                qryRq = new();
                qryRq.NameFilter = new() { Name = addRqName, MatchCriterion = MatchCriterion.StartsWith };
                qryRq.ActiveStatus = "All";
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = new(await QB.ExecuteQbRequestAsync(qryRq));
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                Assert.IsTrue(string.IsNullOrEmpty(qryRs.ParseError));
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
                    addRq.PreferredDeliveryMethod = PreferredDeliveryMethod.Email;
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = new(await QB.ExecuteQbRequestAsync(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");
                    Assert.IsTrue(string.IsNullOrEmpty(addRs.ParseError));
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
                modRq.CreditCardInfo.ExpirationYear = 27;
                modRq.CreditCardInfo.NameOnCard = "Greg Prieto";
                modRq.CreditCardInfo.CreditCardAddress = "3648 Kapalua Way, Raleigh NC";
                modRq.CreditCardInfo.CreditCardPostalCode = "27610";
                modRq.CreditLimit = 10000M;
                Assert.IsTrue(modRq.IsEntityValid());

                modRs = new(await QB.ExecuteQbRequestAsync(modRq));
                Assert.IsTrue(modRs.StatusCode == "0");
                Assert.IsTrue(string.IsNullOrEmpty(modRs.ParseError));
                #endregion
            }
            Thread.Sleep(2000);
        }

        [TestMethod]
        public void TestCustomerTypeModels()
        {
            using (RequestProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                CustomerTypeRs qryRs, addRs;
                CustomerTypeAddRq addRq = new();
                string addRqName = $"QbProcessor";
                #endregion

                #region Query Test
                CustomerTypeQueryRq qryRq = new();
                qryRq.NameFilter = new() { Name = addRqName, MatchCriterion = MatchCriterion.StartsWith };
                qryRq.ActiveStatus = "All";
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = new(QB.ExecuteQbRequest(qryRq));
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                Assert.IsTrue(string.IsNullOrEmpty(qryRs.ParseError));
                #endregion

                #region Add Test
                if (qryRs.TotalCustomerTypes == 0)
                {
                    addRq.Name = addRqName;
                    addRq.IsActive = true;
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = new(QB.ExecuteQbRequest(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");
                    Assert.IsTrue(string.IsNullOrEmpty(addRs.ParseError));
                }
                #endregion
            }
            Thread.Sleep(2000);
        }

        [TestMethod]
        public async Task TestCustomerTypeModelsAsync()
        {
            using (RequestProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                CustomerTypeRs qryRs, addRs;
                CustomerTypeAddRq addRq = new();
                string addRqName = $"QbProcessor";
                #endregion

                #region Query Test
                CustomerTypeQueryRq qryRq = new();
                qryRq.NameFilter = new() { Name = addRqName, MatchCriterion = MatchCriterion.StartsWith };
                qryRq.ActiveStatus = "All";
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = new(await QB.ExecuteQbRequestAsync(qryRq));
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                Assert.IsTrue(string.IsNullOrEmpty(qryRs.ParseError));
                #endregion

                #region Add Test
                if (qryRs.TotalCustomerTypes == 0)
                {
                    addRq.Name = addRqName;
                    addRq.IsActive = true;
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = new(await QB.ExecuteQbRequestAsync(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");
                    Assert.IsTrue(string.IsNullOrEmpty(addRs.ParseError));
                }
                #endregion
            }
            Thread.Sleep(2000);
        }

        [TestMethod]
        public void TestCustomerMsgModels()
        {
            using (RequestProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                CustomerMsgRs qryRs, addRs;
                CustomerMsgAddRq addRq = new();
                string addRqName = $"QbProcessor";
                #endregion

                #region Query Test
                CustomerMsgQueryRq qryRq = new();
                qryRq.NameFilter = new() { Name = addRqName, MatchCriterion = MatchCriterion.StartsWith };
                qryRq.ActiveStatus = "All";
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = new(QB.ExecuteQbRequest(qryRq));
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                Assert.IsTrue(string.IsNullOrEmpty(qryRs.ParseError));
                #endregion

                #region Add Test
                if (qryRs.TotalCustomerMsgs == 0)
                {
                    addRq.Name = addRqName;
                    addRq.IsActive = true;
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = new(QB.ExecuteQbRequest(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");
                    Assert.IsTrue(string.IsNullOrEmpty(addRs.ParseError));
                }
                #endregion
            }
            Thread.Sleep(2000);
        }

        [TestMethod]
        public async Task TestCustomerMsgModelsAsync()
        {
            using (RequestProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                CustomerMsgRs qryRs, addRs;
                CustomerMsgAddRq addRq = new();
                string addRqName = $"QbProcessor";
                #endregion

                #region Query Test
                CustomerMsgQueryRq qryRq = new();
                qryRq.NameFilter = new() { Name = addRqName, MatchCriterion = MatchCriterion.StartsWith };
                qryRq.ActiveStatus = "All";
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = new(await QB.ExecuteQbRequestAsync(qryRq));
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                Assert.IsTrue(string.IsNullOrEmpty(qryRs.ParseError));
                #endregion

                #region Add Test
                if (qryRs.TotalCustomerMsgs == 0)
                {
                    addRq.Name = addRqName;
                    addRq.IsActive = true;
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = new(await QB.ExecuteQbRequestAsync(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");
                    Assert.IsTrue(string.IsNullOrEmpty(addRs.ParseError));
                }
                #endregion
            }
            Thread.Sleep(2000);
        }
    }
}
