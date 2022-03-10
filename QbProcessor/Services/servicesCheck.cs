using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        public string GetChecks(CheckQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetChecks(CheckQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        public string AddCheck(CheckAddRq check) => QbObjectProcessor(check, Guid.NewGuid());
        public string AddCheck(CheckAddRq check, Guid requesterId) => QbObjectProcessor(check, requesterId);
        #endregion Public Methods
    }
}