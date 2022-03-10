using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        public string GetPurchaseOrders() => QbObjectProcessor(new PurchaseOrderQueryRq(), Guid.NewGuid());
        public string GetPurchaseOrders(Guid requesterId) => QbObjectProcessor(new PurchaseOrderQueryRq(), requesterId);
        public string GetPurchaseOrders(PurchaseOrderQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetPurchaseOrders(PurchaseOrderQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        public string AddPurchaseOrder(PurchaseOrderAddRq po) => QbObjectProcessor(po, Guid.NewGuid());
        public string AddPurchaseOrder(PurchaseOrderAddRq po, Guid requesterId) => QbObjectProcessor(po, requesterId);
        #endregion Public Methods
    }
}