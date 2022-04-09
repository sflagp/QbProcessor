using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        internal string GetCompany() => QbObjectProcessor(new CompanyQueryRq(), Guid.NewGuid());
        internal string GetCompany(Guid requesterId) => QbObjectProcessor(new CompanyQueryRq(), requesterId);
        #endregion Public Methods
    }
}