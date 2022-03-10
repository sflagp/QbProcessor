using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        public string GetInvoices() => QbObjectProcessor(new InvoiceQueryRq(), Guid.NewGuid());
        public string GetInvoices(Guid requesterId) => QbObjectProcessor(new InvoiceQueryRq(), requesterId);
        public string GetInvoices(InvoiceQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetInvoices(InvoiceQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        public string GetCreditMemos() => QbObjectProcessor(new CreditMemoQueryRq(), Guid.NewGuid());
        public string GetCreditMemos(Guid requesterId) => QbObjectProcessor(new CreditMemoQueryRq(), requesterId);
        public string GetCreditMemos(CreditMemoQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetCreditMemos(CreditMemoQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);
        #endregion Public Methods
    }
}