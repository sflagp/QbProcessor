using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace QbProcessor.TEST
{
    [TestClass]
    public class CreditCardChargeTests
    {
        [TestMethod]
        public void TestCreditCardChargeModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbCreditCardChargesView qryRs, addRs = new(), modRs;
                CreditCardChargeAddRq addRq = new();
                CreditCardChargeModRq modRq = new();
                string addRqName = $"QbProcessor";
                string result;
                #endregion

                #region Query Test
                CreditCardChargeQueryRq qryRq = new();
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRq.RefNumberFilter = new() { RefNumber = addRqName, MatchCriterion = "StartsWith" };
                Assert.IsTrue(qryRq.IsEntityValid());

                result = QB.ExecuteQbRequest(qryRq);
                qryRs = QB.ToView<QbCreditCardChargesView>(result);
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                #endregion

                #region Add Test
                if (qryRs.TotalCreditCardCharges == 0)
                {
                    Random rdm = new();

                    AccountQueryRq accountsRq = new();
                    accountsRq.AccountType = "CreditCard";
                    QbAccountsView accounts = QB.ToView<QbAccountsView>(QB.ExecuteQbRequest(accountsRq));
                    AccountRetDto account = accounts.Accounts[rdm.Next(0, accounts.Accounts.Count)];

                    ItemQueryRq itemsRq = new();
                    QbItemsView items = QB.ToView<QbItemsView>(QB.ExecuteQbRequest(itemsRq));
                    ItemOtherChargeRetDto item = items.OtherChargeItems[rdm.Next(0, items.PaymentItems.Count)];

                    VendorQueryRq vendorRq = new();
                    QbVendorsView vendors = QB.ToView<QbVendorsView>(QB.ExecuteQbRequest(vendorRq));
                    VendorRetDto vendor = vendors.Vendors[rdm.Next(0, vendors.Vendors.Count)];

                    CustomerQueryRq customerRq = new();
                    QbCustomersView customers = QB.ToView<QbCustomersView>(QB.ExecuteQbRequest(customerRq));
                    CustomerRetDto customer = customers.Customers[rdm.Next(0, customers.Customers.Count)];

                    addRq.Account = new() { ListID = account.ListID };
                    addRq.PayeeEntity = new() { ListID = vendor.ListID };
                    addRq.TxnDate = DateTime.Now;
                    addRq.RefNumber = addRqName;
                    addRq.ItemLine = new();
                    addRq.ItemLine.Item = new() { ListID = item.ListID };
                    addRq.ItemLine.Amount = 123.45M;
                    Assert.IsTrue(addRq.IsEntityValid());

                    result = QB.ExecuteQbRequest(addRq);
                    addRs = QB.ToView<QbCreditCardChargesView>(result);
                    Assert.IsTrue(addRs.StatusCode == "0");
                }
                #endregion

                #region Mod Test
                CreditCardChargeRetDto creditCardCharge = qryRs.TotalCreditCardCharges == 0 ? addRs.CreditCardCharges[0] : qryRs.CreditCardCharges[0];
                modRq.TxnID = creditCardCharge.TxnID;
                modRq.EditSequence = creditCardCharge.EditSequence;
                modRq.Account = creditCardCharge.Account;
                modRq.TxnDate = creditCardCharge.TxnDate;
                modRq.Memo = $"QbProcessor.{addRq.GetType().Name} on {DateTime.Now}";
                Assert.IsTrue(modRq.IsEntityValid());

                modRq.TxnDate = default;
                result = QB.ExecuteQbRequest(modRq);
                modRs = QB.ToView<QbCreditCardChargesView>(result);
                Assert.IsTrue(modRs.StatusCode == "0");
                #endregion
            }
            Thread.Sleep(2000);
        }
    }

    [TestClass]
    public class CreditCardCreditTests
    {
        [TestMethod]
        public void TestCreditCardCreditModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbCreditCardCreditsView qryRs, addRs = new(), modRs;
                CreditCardCreditAddRq addRq = new();
                CreditCardCreditModRq modRq = new();
                string addRqName = $"QbProcessor";
                string result;
                #endregion

                #region Query Test
                CreditCardCreditQueryRq qryRq = new();
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRq.RefNumberFilter = new() { RefNumber = addRqName, MatchCriterion = "StartsWith" };
                Assert.IsTrue(qryRq.IsEntityValid());

                result = QB.ExecuteQbRequest(qryRq);
                qryRs = QB.ToView<QbCreditCardCreditsView>(result);
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                #endregion

                #region Add Test
                if (qryRs.TotalCreditCardCredits == 0)
                {
                    Random rdm = new();

                    AccountQueryRq accountsRq = new();
                    accountsRq.AccountType = "CreditCard";
                    QbAccountsView accounts = QB.ToView<QbAccountsView>(QB.ExecuteQbRequest(accountsRq));
                    AccountRetDto account = accounts.Accounts[rdm.Next(0, accounts.Accounts.Count)];

                    ItemQueryRq itemsRq = new();
                    QbItemsView items = QB.ToView<QbItemsView>(QB.ExecuteQbRequest(itemsRq));
                    ItemOtherChargeRetDto item = items.OtherChargeItems[rdm.Next(0, items.PaymentItems.Count)];

                    VendorQueryRq vendorRq = new();
                    QbVendorsView vendors = QB.ToView<QbVendorsView>(QB.ExecuteQbRequest(vendorRq));
                    VendorRetDto vendor = vendors.Vendors[rdm.Next(0, vendors.Vendors.Count)];

                    CustomerQueryRq customerRq = new();
                    QbCustomersView customers = QB.ToView<QbCustomersView>(QB.ExecuteQbRequest(customerRq));
                    CustomerRetDto customer = customers.Customers[rdm.Next(0, customers.Customers.Count)];

                    addRq.Account = new() { ListID = account.ListID };
                    addRq.PayeeEntity = new() { ListID = vendor.ListID };
                    addRq.TxnDate = DateTime.Now;
                    addRq.RefNumber = addRqName;
                    addRq.ItemLine = new();
                    addRq.ItemLine.Item = new() { ListID = item.ListID };
                    addRq.ItemLine.Amount = 123.45M;
                    Assert.IsTrue(addRq.IsEntityValid());

                    result = QB.ExecuteQbRequest(addRq);
                    addRs = QB.ToView<QbCreditCardCreditsView>(result);
                    Assert.IsTrue(addRs.StatusCode == "0");
                }
                #endregion

                #region Mod Test
                CreditCardCreditRetDto creditCardCredit = qryRs.TotalCreditCardCredits == 0 ? addRs.CreditCardCredits[0] : qryRs.CreditCardCredits[0];
                modRq.TxnID = creditCardCredit.TxnID;
                modRq.EditSequence = creditCardCredit.EditSequence;
                modRq.Account = creditCardCredit.Account;
                modRq.TxnDate = creditCardCredit.TxnDate;
                modRq.PayeeEntity = creditCardCredit.PayeeEntity;
                modRq.Memo = $"QbProcessor.{modRq.GetType().Name} on {DateTime.Now}";
                Assert.IsTrue(modRq.IsEntityValid());

                modRq.TxnDate = default;
                result = QB.ExecuteQbRequest(modRq);
                modRs = QB.ToView<QbCreditCardCreditsView>(result);
                Assert.IsTrue(modRs.StatusCode == "0");
                #endregion
            }
            Thread.Sleep(2000);
        }
    }

    [TestClass]
    public class BillPaymentCreditCardTests
    {
        [TestMethod]
        public void TestBillPaymentCreditCardModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbBillPaymentCreditCardsView qryRs, addRs;
                BillPaymentCreditCardAddRq addRq = new();
                string addRqName = $"QbProcessor";
                #endregion

                #region Query Test
                BillPaymentCreditCardQueryRq qryRq = new();
                qryRq.RefNumberFilter = new() { RefNumber = addRqName, MatchCriterion = "StartsWith" };
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = QB.ToView<QbBillPaymentCreditCardsView>(QB.ExecuteQbRequest(qryRq));
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                #endregion

                #region Add Test
                if (qryRs.TotalBillPaymentCreditCards == 0)
                {
                    Random rdm = new();

                    AccountQueryRq accountsRq = new();
                    QbAccountsView accounts = QB.ToView<QbAccountsView>(QB.ExecuteQbRequest(accountsRq));
                    AccountRetDto account = accounts.Accounts.FirstOrDefault(a => a.AccountType == "AccountsPayable");
                    AccountRetDto card = accounts.Accounts.FirstOrDefault(a => a.AccountType == "CreditCard");

                    BillQueryRq billsRq = new();
                    QbBillsView bills = QB.ToView<QbBillsView>(QB.ExecuteQbRequest(billsRq));
                    BillRetDto bill = bills.Bills[rdm.Next(0, bills.Bills.Count)];

                    VendorQueryRq vendorRq = new();
                    QbVendorsView vendors = QB.ToView<QbVendorsView>(QB.ExecuteQbRequest(vendorRq));
                    VendorRetDto vendor = vendors.Vendors[rdm.Next(0, vendors.Vendors.Count)];

                    addRq.PayeeEntity = new() { ListID = vendor.ListID };
                    addRq.APAccount = new() { ListID = account.ListID };
                    addRq.CreditCardAccount = new() { ListID = card.ListID };
                    addRq.TxnDate = DateTime.Now;
                    addRq.RefNumber = addRqName;
                    addRq.AppliedToTxnAdd = new();
                    addRq.AppliedToTxnAdd.Add(new AppliedToTxnAddDto() { TxnID = bill.TxnID, PaymentAmount = 1M });
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = QB.ToView<QbBillPaymentCreditCardsView>(QB.ExecuteQbRequest(addRq));
                    Regex responses = new(@"^0\b|^3120\b|^3250\b");
                    Assert.IsTrue(responses.IsMatch(addRs.StatusCode));
                }
                #endregion
            }
            Thread.Sleep(2000);
        }
    }

    [TestClass]
    public class ARRefundCreditCardTests
    {
        [TestMethod]
        public void TestARRefundCreditCardModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbARRefundCreditCardsView qryRs, addRs;
                ARRefundCreditCardAddRq addRq = new();
                string addRqName = $"QbProcessor";
                #endregion

                #region Query Test
                ARRefundCreditCardQueryRq qryRq = new();
                qryRq.RefNumberFilter = new() { RefNumber = addRqName, MatchCriterion = "StartsWith" };
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = QB.ToView<QbARRefundCreditCardsView>(QB.ExecuteQbRequest(qryRq));
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                #endregion

                #region Add Test
                if (qryRs.TotalARRefundCreditCards == 0)
                {
                    Random rdm = new();

                    AccountQueryRq accountsRq = new();
                    QbAccountsView accounts = QB.ToView<QbAccountsView>(QB.ExecuteQbRequest(accountsRq));
                    AccountRetDto account = accounts.Accounts.FirstOrDefault(a => a.AccountType == "AccountsReceivable");
                    AccountRetDto card = accounts.Accounts.FirstOrDefault(a => a.AccountType == "CreditCard");

                    InvoiceQueryRq invoicesRq = new() { MaxReturned = 50 };
                    QbInvoicesView invoices = QB.ToView<QbInvoicesView>(QB.ExecuteQbRequest(invoicesRq));
                    InvoiceRetDto invoice = invoices.Invoices?[rdm.Next(0, invoices.Invoices.Count)];

                    CustomerQueryRq customerRq = new();
                    QbCustomersView customers = QB.ToView<QbCustomersView>(QB.ExecuteQbRequest(customerRq));
                    CustomerRetDto customer = customers.Customers[rdm.Next(0, customers.Customers.Count)];

                    addRq.Customer = new() { ListID = customer.ListID };
                    addRq.ARAccount = new() { ListID = account.ListID };
                    addRq.RefundFromAccount = new() { ListID = card.ListID };
                    addRq.TxnDate = DateTime.Now;
                    addRq.RefNumber = addRqName;
                    addRq.RefundAppliedToTxnAdd = new();
                    addRq.RefundAppliedToTxnAdd.Add(new() { TxnID = invoice.TxnID, RefundAmount = invoice.AppliedAmount });
                    Assert.IsTrue(addRq.IsEntityValid());

                    //addRs = QB.ToView<QbARRefundCreditCardsView>(QB.ExecuteQbRequest(addRq));
                    //Regex responses = new(@"^0\b|^3120\b|^3250\b");
                    //Assert.IsTrue(responses.IsMatch(addRs.StatusCode));
                }
                #endregion
            }
            Thread.Sleep(2000);
        }
    }
}
