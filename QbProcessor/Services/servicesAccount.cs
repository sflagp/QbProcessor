using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        internal string GetAccounts() => QbObjectProcessor(new AccountQueryRq(), Guid.NewGuid());
        internal string GetAccounts(Guid requesterId) => QbObjectProcessor(new AccountQueryRq(), requesterId);
        internal string GetAccounts(AccountQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetAccounts(AccountQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        internal string AddAccount(AccountAddRq account) => QbObjectProcessor(account, Guid.NewGuid());
        internal string AddAccount(AccountAddRq account, Guid requesterId) => QbObjectProcessor(account, requesterId);
        #endregion Public Methods
    }
}