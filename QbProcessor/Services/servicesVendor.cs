using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        internal string GetVendors() => QbObjectProcessor(new VendorQueryRq(), Guid.NewGuid());
        internal string GetVendors(Guid requesterId) => QbObjectProcessor(new VendorQueryRq(), requesterId);
        internal string GetVendors(VendorQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetVendors(VendorQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        internal string AddVendor(VendorAddRq vendor) => QbObjectProcessor(vendor, Guid.NewGuid());
        internal string AddVendor(VendorAddRq vendor, Guid requesterId) => QbObjectProcessor(vendor, requesterId);
        #endregion
    }
}
