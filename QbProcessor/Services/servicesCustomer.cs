using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        public string GetCustomers() => QbObjectProcessor(new CustomerQueryRq(), Guid.NewGuid());
        public string GetCustomers(Guid requesterId) => QbObjectProcessor(new CustomerQueryRq(), requesterId);
        public string GetCustomers(CustomerQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetCustomers(CustomerQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        public string AddCustomer(CustomerAddRq customer) => QbObjectProcessor(customer, Guid.NewGuid());
        public string AddCustomer(CustomerAddRq customer, Guid requesterId) => QbObjectProcessor(customer, requesterId);
        #endregion Public Methods
    }
}