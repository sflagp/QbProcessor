using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        internal string GetAllItems() => QbObjectProcessor(new ItemQueryRq(), Guid.NewGuid());
        internal string GetAllItems(Guid requesterId) => QbObjectProcessor(new ItemQueryRq(), requesterId);
        internal string GetAllItems(ItemQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetAllItems(ItemQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        internal string GetServiceItems(ItemServiceQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetServiceItems(ItemServiceQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        internal string GetOtherChargeItems(ItemOtherChargeQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetOtherChargeItems(ItemOtherChargeQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);
        
        internal string GetNonInventoryItems(ItemNonInventoryQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetNonInventoryItems(ItemNonInventoryQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);
        #endregion Public Methods
    }
}