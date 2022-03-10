using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods

        public string GetBills(BillQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetBills(BillQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);
        public string GetBillPaymentChecks(BillPaymentCheckQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetBillPaymentChecks(BillPaymentCheckQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);
        public string AddBill(BillAddRq bill) => QbObjectProcessor(bill, Guid.NewGuid());
        public string AddBill(BillAddRq bill, Guid requesterId) => QbObjectProcessor(bill, requesterId);
        public string AddBillPaymentCheck(BillPaymentCheckAddRq bill) => QbObjectProcessor(bill, Guid.NewGuid());
        public string AddBillPaymentCheck(BillPaymentCheckAddRq bill, Guid requesterId) => QbObjectProcessor(bill, requesterId);

        #endregion Public Methods
    }
}