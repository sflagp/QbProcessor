using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        internal string GetPurchaseOrders() => QbObjectProcessor(new PurchaseOrderQueryRq(), Guid.NewGuid());
        internal string GetPurchaseOrders(Guid requesterId) => QbObjectProcessor(new PurchaseOrderQueryRq(), requesterId);
        internal string GetPurchaseOrders(PurchaseOrderQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetPurchaseOrders(PurchaseOrderQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        internal string AddPurchaseOrder(PurchaseOrderAddRq po) => QbObjectProcessor(po, Guid.NewGuid());
        internal string AddPurchaseOrder(PurchaseOrderAddRq po, Guid requesterId) => QbObjectProcessor(po, requesterId);
        #endregion Public Methods
    }
}