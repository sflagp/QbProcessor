using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        internal string GetChecks(CheckQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetChecks(CheckQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        internal string AddCheck(CheckAddRq check) => QbObjectProcessor(check, Guid.NewGuid());
        internal string AddCheck(CheckAddRq check, Guid requesterId) => QbObjectProcessor(check, requesterId);
        #endregion Public Methods
    }
}