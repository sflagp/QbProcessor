using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        public string GetVendors() => QbObjectProcessor(new VendorQueryRq(), Guid.NewGuid());
        public string GetVendors(Guid requesterId) => QbObjectProcessor(new VendorQueryRq(), requesterId);
        public string GetVendors(VendorQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetVendors(VendorQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        public string AddVendor(VendorAddRq vendor) => QbObjectProcessor(vendor, Guid.NewGuid());
        public string AddVendor(VendorAddRq vendor, Guid requesterId) => QbObjectProcessor(vendor, requesterId);
        #endregion
    }
}
