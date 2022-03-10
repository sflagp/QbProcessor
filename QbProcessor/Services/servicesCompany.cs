using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        public string GetCompany() => QbObjectProcessor(new CompanyQueryRq(), Guid.NewGuid());
        public string GetCompany(Guid requesterId) => QbObjectProcessor(new CompanyQueryRq(), requesterId);
        #endregion Public Methods
    }
}