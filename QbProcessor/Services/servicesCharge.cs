using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        internal string GetCharges() => QbObjectProcessor(new ChargeQueryRq(), Guid.NewGuid());
        internal string GetCharges(Guid requesterId) => QbObjectProcessor(new ChargeQueryRq(), requesterId);
        internal string GetCharges(ChargeQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetCharges(ChargeQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        internal string AddCharge(ChargeAddRq charge) => QbObjectProcessor(charge, Guid.NewGuid());
        internal string AddCharge(ChargeAddRq charge, Guid requesterId) => QbObjectProcessor(charge, requesterId);
        #endregion Public Methods
    }
}