using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        internal string GetOtherName() => QbObjectProcessor(new OtherNameQueryRq(), Guid.NewGuid());
        internal string GetOtherName(Guid requesterId) => QbObjectProcessor(new OtherNameQueryRq(), requesterId);
        internal string GetOtherName(OtherNameQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetOtherName(OtherNameQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        internal string AddOtherName(OtherNameAddRq otherName) => QbObjectProcessor(otherName, Guid.NewGuid());
        internal string AddOtherName(OtherNameAddRq otherName, Guid requesterId) => QbObjectProcessor(otherName, requesterId);
        #endregion Public Methods
    }
}