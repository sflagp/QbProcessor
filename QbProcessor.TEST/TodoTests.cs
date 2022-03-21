using Microsoft.VisualStudio.TestTools.UnitTesting;
using QbModels;
using System;
using System.Threading;

namespace QbProcessor.TEST
{
    [TestClass]
    public class TodoTests
    {
        [TestMethod]
        public void TestTodoModels()
        {
            using (QBProcessor.QbProcessor QB = new())
            {
                #region Properties
                if (QB == null)
                {
                    throw new Exception("Quickbooks not loaded or error connecting to Quickbooks.");
                }

                QbTodosView qryRs, addRs = new(), modRs;
                TodoAddRq addRq = new();
                TodoModRq modRq = new();
                Random rdm = new();
                string addRqName = $"QbProcessor";
                string result;
                #endregion

                #region Query Test
                TodoQueryRq qryRq = new();
                Assert.IsTrue(qryRq.IsEntityValid());

                qryRq.DoneStatus = "NotDoneOnly";
                Assert.IsTrue(qryRq.IsEntityValid());

                result = QB.ExecuteQbRequest(qryRq);
                qryRs = QB.ToView<QbTodosView>(result);
                if (qryRs.StatusCode == "3231") Assert.Inconclusive(qryRs.StatusMessage);
                Assert.IsTrue(qryRs.StatusSeverity == "Info");
                #endregion

                #region Add Test
                if (qryRs.TotalTodos == 0)
                {
                    CustomerQueryRq custRq = new();
                    QbCustomersView customers = QB.ToView<QbCustomersView>(QB.ExecuteQbRequest(custRq));
                    CustomerRetDto cust = customers.Customers[rdm.Next(0, customers.Customers.Count)];

                    addRq.Notes = $"{addRqName}.{addRq.GetType().Name}";
                    addRq.Customer = new() { ListID = cust.ListID };
                    addRq.Notes = $"Requested by {addRqName} on {DateTime.Now}";
                    addRq.IsActive = true;
                    Assert.IsTrue(addRq.IsEntityValid());

                    result = QB.ExecuteQbRequest(addRq);
                    addRs = QB.ToView<QbTodosView>(result);
                    if (addRs.StatusCode == "3250") Assert.Inconclusive(addRs.StatusMessage);
                    Assert.IsTrue(addRs.StatusCode == "0");
                }
                #endregion

                #region Mod Test
                TodoRetDto Todo = qryRs.TotalTodos == 0 ? addRs.Todos[0] : qryRs.Todos[0];

                EmployeeQueryRq empRq = new();
                QbEmployeesView employees = QB.ToView<QbEmployeesView>(QB.ExecuteQbRequest(empRq));
                EmployeeRetDto emp = employees.Employees[rdm.Next(0, employees.Employees.Count)];

                modRq.ListID = Todo.ListID;
                modRq.EditSequence = Todo.EditSequence;
                modRq.Employee = new() { ListID = emp.ListID };
                modRq.Notes = $"Completed by {addRqName}.{modRq.GetType().Name} on {DateTime.Now}";
                modRq.IsDone = true;
                Assert.IsTrue(modRq.IsEntityValid());

                result = QB.ExecuteQbRequest(modRq);
                modRs = QB.ToView<QbTodosView>(result);
                Assert.IsTrue(modRs.StatusCode == "0");
                #endregion
            }
            Thread.Sleep(2000);
        }
    }
}
