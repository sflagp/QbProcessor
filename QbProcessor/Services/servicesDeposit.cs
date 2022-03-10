using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        public string GetDeposits() => QbObjectProcessor(new DepositQueryRq(), Guid.NewGuid());
        public string GetDeposits(Guid requesterId) => QbObjectProcessor(new DepositQueryRq(), requesterId);
        public string GetDeposits(DepositQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetDeposits(DepositQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        public string AddDeposit(DepositAddRq deposit) => QbObjectProcessor(deposit, Guid.NewGuid());
        public string AddDeposit(DepositAddRq deposit, Guid requesterId) => QbObjectProcessor(deposit, requesterId);
        #endregion Public Methods
    }
}