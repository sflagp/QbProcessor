using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        internal string GetDeposits() => QbObjectProcessor(new DepositQueryRq(), Guid.NewGuid());
        internal string GetDeposits(Guid requesterId) => QbObjectProcessor(new DepositQueryRq(), requesterId);
        internal string GetDeposits(DepositQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetDeposits(DepositQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        internal string AddDeposit(DepositAddRq deposit) => QbObjectProcessor(deposit, Guid.NewGuid());
        internal string AddDeposit(DepositAddRq deposit, Guid requesterId) => QbObjectProcessor(deposit, requesterId);
        #endregion Public Methods
    }
}