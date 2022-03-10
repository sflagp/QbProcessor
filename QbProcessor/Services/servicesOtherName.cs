using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        public string GetOtherName() => QbObjectProcessor(new OtherNameQueryRq(), Guid.NewGuid());
        public string GetOtherName(Guid requesterId) => QbObjectProcessor(new OtherNameQueryRq(), requesterId);
        public string GetOtherName(OtherNameQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetOtherName(OtherNameQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        public string AddOtherName(OtherNameAddRq otherName) => QbObjectProcessor(otherName, Guid.NewGuid());
        public string AddOtherName(OtherNameAddRq otherName, Guid requesterId) => QbObjectProcessor(otherName, requesterId);
        #endregion Public Methods
    }
}