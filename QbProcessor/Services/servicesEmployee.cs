using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        public string GetEmployees() => QbObjectProcessor(new EmployeeQueryRq(), Guid.NewGuid());
        public string GetEmployees(Guid requesterId) => QbObjectProcessor(new EmployeeQueryRq(), requesterId);
        public string GetEmployees(EmployeeQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetEmployees(EmployeeQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        public string AddEmployee(EmployeeAddRq employee) => QbObjectProcessor(employee, Guid.NewGuid());
        public string AddEmployee(EmployeeAddRq employee, Guid requesterId) => QbObjectProcessor(employee, requesterId);
        #endregion Public Methods
    }
}