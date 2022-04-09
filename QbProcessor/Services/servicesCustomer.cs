using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        internal string GetCustomers() => QbObjectProcessor(new CustomerQueryRq(), Guid.NewGuid());
        internal string GetCustomers(Guid requesterId) => QbObjectProcessor(new CustomerQueryRq(), requesterId);
        internal string GetCustomers(CustomerQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetCustomers(CustomerQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        internal string AddCustomer(CustomerAddRq customer) => QbObjectProcessor(customer, Guid.NewGuid());
        internal string AddCustomer(CustomerAddRq customer, Guid requesterId) => QbObjectProcessor(customer, requesterId);
        #endregion Public Methods
    }
}