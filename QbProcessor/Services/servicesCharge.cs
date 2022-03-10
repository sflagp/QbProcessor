using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        public string GetCharges() => QbObjectProcessor(new ChargeQueryRq(), Guid.NewGuid());
        public string GetCharges(Guid requesterId) => QbObjectProcessor(new ChargeQueryRq(), requesterId);
        public string GetCharges(ChargeQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetCharges(ChargeQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        public string AddCharge(ChargeAddRq charge) => QbObjectProcessor(charge, Guid.NewGuid());
        public string AddCharge(ChargeAddRq charge, Guid requesterId) => QbObjectProcessor(charge, requesterId);
        #endregion Public Methods
    }
}