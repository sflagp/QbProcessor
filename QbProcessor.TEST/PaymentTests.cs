using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace QbProcessor.TEST
{
    [TestClass]
    public class ReceivePaymentTests
    {
        [TestMethod]
        public void TestReceivePaymentModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbReceivePaymentsView qryRs, addRs = new(), modRs;
                ReceivePaymentAddRq addRq = new();
                ReceivePaymentModRq modRq = new();
                string addRqName = $"QbProcessor";
                #endregion

                #region Query Test
                ReceivePaymentQueryRq qryRq = new();
                qryRq.RefNumberFilter = new() { RefNumber = addRqName, MatchCriterion = "StartsWith" };
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = QB.ToView<QbReceivePaymentsView>(QB.ExecuteQbRequest(qryRq));
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                #endregion

                #region Add Test
                if (qryRs.TotalReceivePayments == 0)
                {
                    Random rdm = new();

                    AccountQueryRq bankRq = new() { AccountType = "Bank" };
                    QbAccountsView banks = QB.ToView<QbAccountsView>(QB.ExecuteQbRequest(bankRq));
                    AccountRetDto bank = banks.Accounts[rdm.Next(0, banks.Accounts.Count)];

                    InvoiceQueryRq invoicesRq = new() { MaxReturned = 50, PaidStatus = "NotPaidOnly" };
                    QbInvoicesView invoices = QB.ToView<QbInvoicesView>(QB.ExecuteQbRequest(invoicesRq));
                    InvoiceRetDto invoice = invoices.Invoices?[rdm.Next(0, invoices.Invoices.Count)];

                    addRq.RefNumber = addRqName;
                    addRq.Customer = new() { ListID = invoice.Customer.ListID };
                    addRq.DepositToAccount = new() { ListID = bank.ListID };
                    addRq.TxnDate = DateTime.Now;
                    addRq.TotalAmount = invoice.BalanceRemaining;
                    addRq.AppliedToTxn = new();
                    addRq.AppliedToTxn.Add(new()
                    {
                        TxnID = invoice.TxnID,
                        PaymentAmount = invoice.BalanceRemaining
                    });
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = QB.ToView<QbReceivePaymentsView>(QB.ExecuteQbRequest(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");
                }
                #endregion

                #region Mod Test
                ReceivePaymentRetDto receivePayment = qryRs.TotalReceivePayments == 0 ? addRs.ReceivePayments[0] : qryRs.ReceivePayments[0];
                modRq.TxnID = receivePayment.TxnID;
                modRq.EditSequence = receivePayment.EditSequence;
                modRq.Customer = new() { ListID = receivePayment.Customer.ListID };
                modRq.TotalAmount = receivePayment.TotalAmount;
                modRq.Memo = $"QbProcessor.{modRq.GetType().Name} on {DateTime.Now}";
                Assert.IsTrue(modRq.IsEntityValid());

                modRs = QB.ToView<QbReceivePaymentsView>(QB.ExecuteQbRequest(modRq));
                Assert.IsTrue(modRs.StatusCode == "0");
                #endregion
            }
            Thread.Sleep(2000);
        }

        [TestMethod]
        public void TestPaymentMethodModel()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbPaymentMethodsView qryRs, addRs;
                PaymentMethodQueryRq qryRq;
                PaymentMethodAddRq addRq;

                string addRqName = "QbProcessor";
                string result;
                #endregion

                #region Query Test
                qryRq = new();
                Assert.IsTrue(qryRq.IsEntityValid());

                result = QB.ExecuteQbRequest(qryRq);
                qryRs = QB.ToView<QbPaymentMethodsView>(result);
                Regex statusCodes =  new(@"\b0\b|\b3250\b");
                Assert.IsTrue(statusCodes.IsMatch(qryRs.StatusCode));
                if (qryRs.StatusCode == "3250") Assert.Inconclusive(qryRs.StatusMessage);
                #endregion

                #region Add Test
                if (qryRs.TotalPaymentMethods == 0)
                {
                    addRq = new()
                    {
                        Name = addRqName,
                        IsActive = true,
                        PaymentMethodType = "OtherCreditCard"
                    };
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = QB.ToView<QbPaymentMethodsView>(QB.ExecuteQbRequest(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");
                    Assert.IsTrue(addRs.TotalPaymentMethods > 0);
                }
                #endregion
            }
            Thread.Sleep(2000);
        }
    }
}
