using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        public string GetAllItems() => QbObjectProcessor(new ItemQueryRq(), Guid.NewGuid());
        public string GetAllItems(Guid requesterId) => QbObjectProcessor(new ItemQueryRq(), requesterId);
        public string GetAllItems(ItemQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetAllItems(ItemQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        public string GetServiceItems(ItemServiceQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetServiceItems(ItemServiceQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        public string GetOtherChargeItems(ItemOtherChargeQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetOtherChargeItems(ItemOtherChargeQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);
        
        public string GetNonInventoryItems(ItemNonInventoryQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetNonInventoryItems(ItemNonInventoryQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);
        #endregion Public Methods
    }
}