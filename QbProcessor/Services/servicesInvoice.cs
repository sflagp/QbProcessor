using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        internal string GetInvoices() => QbObjectProcessor(new InvoiceQueryRq(), Guid.NewGuid());
        internal string GetInvoices(Guid requesterId) => QbObjectProcessor(new InvoiceQueryRq(), requesterId);
        internal string GetInvoices(InvoiceQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetInvoices(InvoiceQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        internal string GetCreditMemos() => QbObjectProcessor(new CreditMemoQueryRq(), Guid.NewGuid());
        internal string GetCreditMemos(Guid requesterId) => QbObjectProcessor(new CreditMemoQueryRq(), requesterId);
        internal string GetCreditMemos(CreditMemoQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetCreditMemos(CreditMemoQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);
        #endregion Public Methods
    }
}