using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QbProcessor.TEST
{
    [TestClass]
    public class JournalEntryTests
    {
        [TestMethod]
        public void TestJournalEntryModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbJournalEntriesView qryRs, addRs = new(), modRs;
                JournalEntryAddRq addRq = new();
                JournalEntryModRq modRq = new();
                string addRqName = $"QbProcessor";
                #endregion

                #region Query Test
                JournalEntryQueryRq qryRq = new() { IncludeLineItems = true };
                qryRq.RefNumberFilter = new() { RefNumber = addRqName, MatchCriterion = "StartsWith" };
                Assert.IsTrue(qryRq.IsEntityValid());

                string strRs = QB.ExecuteQbRequest(qryRq);
                qryRs = QB.ToView<QbJournalEntriesView>(strRs);
                Assert.IsTrue(qryRs.StatusSeverity == "Info");

                if (qryRs.TotalJournalEntries > 0) Assert.Inconclusive("Journal Entries already exist.  Cannot test.");
                #endregion

                #region Add Test
                if (qryRs.TotalJournalEntries == 0)
                {
                    Random rdm = new();

                    AccountQueryRq accountsRq = new();
                    QbAccountsView accounts = QB.ToView<QbAccountsView>(QB.ExecuteQbRequest(accountsRq));
                    List<AccountRetDto> expenses = 
                        accounts.Accounts.Where(a => a.AccountType.Contains("Expense") && a.Balance > 0).ToList();
                    List<AccountRetDto> assets = accounts.Accounts.Where(a => a.AccountType.Contains("Asset")).ToList();
                    AccountRetDto expense = expenses[rdm.Next(0, expenses.Count)];
                    AccountRetDto asset = assets[rdm.Next(0, assets.Count)];

                    CustomerQueryRq customerRq = new();
                    QbCustomersView customers = QB.ToView<QbCustomersView>(QB.ExecuteQbRequest(customerRq));
                    CustomerRetDto customer = customers.Customers[rdm.Next(0, customers.Customers.Count)];

                    VendorQueryRq vendorRq = new();
                    QbVendorsView vendors = QB.ToView<QbVendorsView>(QB.ExecuteQbRequest(vendorRq));
                    VendorRetDto vendor = vendors.Vendors[rdm.Next(0, vendors.Vendors.Count)];

                    addRq.TxnDate = DateTime.Now;
                    addRq.RefNumber = addRqName;
                    addRq.DebitLine = new()
                    {
                        Account = new() { ListID = expense.ListID },
                        Amount = 123.45M,
                        Entity = new() { ListID = vendor.ListID }
                    };
                    addRq.CreditLine = new()
                    {
                        Account = new() { ListID = asset.ListID },
                        Amount = 123.45M,
                        Entity = new() { ListID = customer.ListID }
                    };
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = QB.ToView<QbJournalEntriesView>(QB.ExecuteQbRequest(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");
                    Assert.IsTrue(addRs.TotalJournalEntries > 0);
                }
                #endregion

                #region Mod Test
                JournalEntryRetDto journalEntry = addRs.JournalEntries[0];
                modRq.TxnID = journalEntry.TxnID;
                modRq.EditSequence = journalEntry.EditSequence;
                modRq.JournalLineMod = new();
                modRq.JournalLineMod.Add(new()
                {
                    TxnLineID = journalEntry.DebitLine.TxnLineID,
                    JournalLineType = "Debit",
                    Amount = journalEntry.DebitLine.Amount,
                    Account = journalEntry.DebitLine.Account,
                    Entity = journalEntry.DebitLine.Entity,
                    Memo = $"Debit line QbProcessor.{modRq.GetType().Name} on {DateTime.Now}"
                });
                modRq.JournalLineMod.Add(new()
                {
                    TxnLineID = journalEntry.CreditLine.TxnLineID,
                    JournalLineType = "Credit",
                    Amount = journalEntry.CreditLine.Amount,
                    Account = journalEntry.CreditLine.Account,
                    Entity = journalEntry.CreditLine.Entity,
                    Memo = $"Credit line QbProcessor.{modRq.GetType().Name} on {DateTime.Now}"
                });
                Assert.IsTrue(modRq.IsEntityValid());

                modRs = QB.ToView<QbJournalEntriesView>(QB.ExecuteQbRequest(modRq));
                Assert.IsTrue(modRs.StatusCode == "0");
                #endregion
            }
            Thread.Sleep(2000);
        }
    }
}
