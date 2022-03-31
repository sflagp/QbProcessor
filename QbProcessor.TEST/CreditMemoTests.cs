using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels;
using System;
using System.Threading;

namespace QbProcessor.TEST
{
    [TestClass]
    public class CreditMemoTests
    {
        [TestMethod]
        public void TestCreditMemoModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                #region Properties
                QbCreditMemosView qryRs, addRs = new(), modRs;
                CreditMemoAddRq addRq = new();
                CreditMemoModRq modRq = new();
                string addRqName = $"QbProcessor";
                string result;
                #endregion

                #region Query Test
                CreditMemoQueryRq qryRq = new();
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRq.RefNumberFilter = new() { RefNumber = addRqName, MatchCriterion = "StartsWith" };
                Assert.IsTrue(qryRq.IsEntityValid());

                result = QB.ExecuteQbRequest(qryRq);
                qryRs = QB.ToView<QbCreditMemosView>(result);
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                #endregion

                #region Add Test
                if (qryRs.TotalCreditMemos == 0)
                {
                    Random rdm = new();

                    AccountQueryRq accountsRq = new();
                    accountsRq.AccountType = "AccountsReceivable";
                    QbAccountsView accounts = QB.ToView<QbAccountsView>(QB.ExecuteQbRequest(accountsRq));
                    AccountRetDto account = accounts.Accounts[rdm.Next(0, accounts.Accounts.Count)];

                    CustomerQueryRq customerRq = new();
                    QbCustomersView customers = QB.ToView<QbCustomersView>(QB.ExecuteQbRequest(customerRq));
                    CustomerRetDto customer = customers.Customers[rdm.Next(0, customers.Customers.Count)];

                    ItemNonInventoryQueryRq itemsRq = new();
                    QbItemNonInventoryView items = QB.ToView<QbItemNonInventoryView>(QB.ExecuteQbRequest(itemsRq));
                    ItemNonInventoryRetDto item = items.ItemsNonInventory[rdm.Next(0, items.ItemsNonInventory.Count)];

                    addRq.Customer = new() { ListID = customer.ListID };
                    addRq.ARAccount = new() { ListID = account.ListID };
                    addRq.TxnDate = DateTime.Now;
                    addRq.RefNumber = addRqName;
                    addRq.CreditMemoLine = new();
                    addRq.CreditMemoLine.Add( new() 
                    { 
                        Item = new() { ListID = item.ListID },
                        Desc = $"QbProcessor.{addRq.GetType().Name} on {DateTime.Now}", 
                        Amount = 123.45M 
                    });
                    Assert.IsTrue(addRq.IsEntityValid());

                    result = QB.ExecuteQbRequest(addRq);
                    addRs = QB.ToView<QbCreditMemosView>(result);
                    Assert.IsTrue(addRs.StatusCode == "0");
                }
                #endregion

                #region Mod Test
                CreditMemoRetDto CreditMemo = qryRs.TotalCreditMemos == 0 ? addRs.CreditMemos[0] : qryRs.CreditMemos[0];
                modRq.TxnID = CreditMemo.TxnID;
                modRq.EditSequence = CreditMemo.EditSequence;
                modRq.TxnDate = CreditMemo.TxnDate;
                modRq.Customer = CreditMemo.Customer;
                modRq.Memo = $"QbProcessor.{modRq.GetType().Name} on {DateTime.Now}";
                Assert.IsTrue(modRq.IsEntityValid());

                modRq.TxnDate = default;
                result = QB.ExecuteQbRequest(modRq);
                modRs = QB.ToView<QbCreditMemosView>(result);
                Assert.IsTrue(modRs.StatusCode == "0");
                #endregion
            }
            Thread.Sleep(2000);
        }
    }
}
