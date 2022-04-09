using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        internal string GetEmployees() => QbObjectProcessor(new EmployeeQueryRq(), Guid.NewGuid());
        internal string GetEmployees(Guid requesterId) => QbObjectProcessor(new EmployeeQueryRq(), requesterId);
        internal string GetEmployees(EmployeeQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetEmployees(EmployeeQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        internal string AddEmployee(EmployeeAddRq employee) => QbObjectProcessor(employee, Guid.NewGuid());
        internal string AddEmployee(EmployeeAddRq employee, Guid requesterId) => QbObjectProcessor(employee, requesterId);
        #endregion Public Methods
    }
}