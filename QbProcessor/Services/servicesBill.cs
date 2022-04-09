using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods

        internal string GetBills(BillQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetBills(BillQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);
        internal string GetBillPaymentChecks(BillPaymentCheckQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetBillPaymentChecks(BillPaymentCheckQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);
        internal string AddBill(BillAddRq bill) => QbObjectProcessor(bill, Guid.NewGuid());
        internal string AddBill(BillAddRq bill, Guid requesterId) => QbObjectProcessor(bill, requesterId);
        internal string AddBillPaymentCheck(BillPaymentCheckAddRq bill) => QbObjectProcessor(bill, Guid.NewGuid());
        internal string AddBillPaymentCheck(BillPaymentCheckAddRq bill, Guid requesterId) => QbObjectProcessor(bill, requesterId);

        #endregion Public Methods
    }
}