using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbHelpers;
using QbModels;
using System;
using System.Threading;

namespace QbProcessor.TEST
{
    [TestClass]
    public class TimeTrackingTests
    {
        [TestMethod]
        public void TestTimeTrackingModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbTimeTrackingView qryRs, addRs = new(), modRs;
                TimeTrackingAddRq addRq = new();
                TimeTrackingModRq modRq = new();
                EmployeeRetDto emp;
                string addRqName = $"QbProcessor";
                #endregion

                #region Query Test
                EmployeeQueryRq empRq = new() { NameFilter = new() { Name = "QbProcessor", MatchCriterion = "StartsWith" } };
                QbEmployeesView emps = QB.ToView<QbEmployeesView>(QB.ExecuteQbRequest(empRq));
                if (emps.Employees.Count == 0) Assert.Inconclusive("QbProcessor employee not found.");
                emp = emps.Employees[0];

                TimeTrackingQueryRq qryRq = new();
                qryRq.TimeTrackingEntityFilter = new() { ListID = emp.ListID };
                qryRq.TxnDateRangeFilter = new() { FromTxnDate = DateTime.Today.AddDays(-2), ToTxnDate = DateTime.Today };
                Assert.IsTrue(qryRq.IsEntityValid());

                string strRs = QB.ExecuteQbRequest(qryRq);
                qryRs = QB.ToView<QbTimeTrackingView>(strRs);
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                #endregion

                #region Add Test
                if (qryRs.TotalTimeTracking == 0)
                {
                    Random rdm = new();

                    CustomerQueryRq customerRq = new();
                    QbCustomersView customers = QB.ToView<QbCustomersView>(QB.ExecuteQbRequest(customerRq));
                    CustomerRetDto customer = customers.Customers[rdm.Next(0, customers.Customers.Count)];

                    ItemNonInventoryQueryRq itemRq = new();
                    QbItemNonInventoryView items = QB.ToView<QbItemNonInventoryView>(QB.ExecuteQbRequest(itemRq));
                    ItemNonInventoryRetDto item = items.ItemsNonInventory[rdm.Next(0, items.ItemsNonInventory.Count)];

                    addRq.TxnDate = DateTime.Now;
                    addRq.Entity = new() { ListID = emp.ListID };
                    addRq.Customer = new() { ListID = customer.ListID };
                    addRq.ItemService = new() { ListID = item.ListID };
                    addRq.Duration = 1.11M;
                    Assert.IsTrue(addRq.IsEntityValid());

                    addRs = QB.ToView<QbTimeTrackingView>(QB.ExecuteQbRequest(addRq));
                    Assert.IsTrue(addRs.StatusCode == "0");
                    Assert.IsTrue(addRs.TotalTimeTracking > 0);
                }
                #endregion

                #region Mod Test
                TimeTrackingRetDto timeTracking = qryRs.TotalTimeTracking > 0 ? qryRs.TimeTracking[0] : addRs.TimeTracking[0];
                modRq.TxnID = timeTracking.TxnID;
                modRq.EditSequence = timeTracking.EditSequence;
                modRq.Entity = timeTracking.Entity;
                modRq.Duration = timeTracking.Duration.FromQbTime() + 0.05M;
                modRq.TxnDate = DateTime.Now;
                modRq.Notes = $"{addRqName} modified on {DateTime.Now} by {modRq.GetType().Name}";
                modRq.BillableStatus = "Billable";
                Assert.IsTrue(modRq.IsEntityValid());

                modRs = QB.ToView<QbTimeTrackingView>(QB.ExecuteQbRequest(modRq));
                Assert.IsTrue(modRs.StatusCode == "0");
                #endregion
            }
            Thread.Sleep(2000);
        }
    }
}
