using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels;
using QbModels.ENUM;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace QbModels.QbProcessor.TEST
{
    [TestClass]
    public class ReceivePaymentTests
    {
        [TestMethod]
        public void TestReceivePaymentModels()
        {
            using (RequestProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                ReceivePaymentRs qryRs, addRs = new(""), modRs;
                ReceivePaymentAddRq addRq = new();
                ReceivePaymentModRq modRq = new();
                string addRqName = $"QbProcessor";
                #endregion

                #region Query Test
                ReceivePaymentQueryRq qryRq = new();
                qryRq.RefNumberFilter = new() { RefNumber = addRqName, MatchCriterion = MatchCriterion.StartsWith };
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRs = new(QB.ExecuteQbRequest(qryRq));
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                Assert.IsTrue(string.IsNullOrEmpty(qryRs.ParseError));
                #endregion

                #region Add Test
                if (qryRs.TotalReceivePayments == 0)
                {
                    Random rdm = new();

                    AccountQueryRq bankRq = new() { AccountType = AccountType.Bank };
                    AccountRs banks = new(QB.ExecuteQbRequest(bankRq));
                    AccountRetDto bank = banks.Accounts[rdm.Next(0, banks.Accounts.Count)];

                    InvoiceQueryRq invoicesRq = new() { MaxReturned = 50, PaidStatus = PaidStatus.NotPaidOnly };
                    InvoiceRs invoices = new(QB.ExecuteQbRequest(invoicesRq));
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

                    addRs = new(QB.ExecuteQbRequest(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");
                    Assert.IsTrue(string.IsNullOrEmpty(addRs.ParseError));
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

                modRs = new(QB.ExecuteQbRequest(modRq));
                Assert.IsTrue(modRs.StatusCode == "0");
                Assert.IsTrue(string.IsNullOrEmpty(modRs.ParseError));
                #endregion
            }
            Thread.Sleep(2000);
        }

        [TestMethod]
        public void TestPaymentMethodModel()
        {
            using (RequestProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                PaymentMethodRs qryRs, addRs;
                PaymentMethodQueryRq qryRq;
                PaymentMethodAddRq addRq;

                string addRqName = "QbProcessor";
                string result;
                #endregion

                #region Query Test
                qryRq = new();
                Assert.IsTrue(qryRq.IsEntityValid());

                result = QB.ExecuteQbRequest(qryRq);
                qryRs = new(result);
                Regex statusCodes =  new(@"^0$|^3250$");
                Assert.IsTrue(statusCodes.IsMatch(qryRs.StatusCode));
                Assert.IsTrue(string.IsNullOrEmpty(qryRs.ParseError));
                if (qryRs.StatusCode == "3250") Assert.Inconclusive(qryRs.StatusMessage);
                #endregion

                #region Add Test
                if (qryRs.TotalPaymentMethods == 0)
                {
                    addRq = new()
                    {
                        Name = addRqName,
                        IsActive = true,
                        PaymentMethodType = PaymentMethodType.OtherCreditCard
                    };
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = new(QB.ExecuteQbRequest(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");
                    Assert.IsTrue(string.IsNullOrEmpty(addRs.ParseError));
                    Assert.IsTrue(addRs.TotalPaymentMethods > 0);
                }
                #endregion
            }
            Thread.Sleep(2000);
        }
    }
}
