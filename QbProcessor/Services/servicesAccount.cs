using QbHelpers;
using QbModels;
using System;
using System.Xml;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public Methods
        public string GetAccounts() => QbObjectProcessor(new AccountQueryRq(), Guid.NewGuid());
        public string GetAccounts(Guid requesterId) => QbObjectProcessor(new AccountQueryRq(), requesterId);
        public string GetAccounts(AccountQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetAccounts(AccountQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        public string AddAccount(AccountAddRq account) => QbObjectProcessor(account, Guid.NewGuid());
        public string AddAccount(AccountAddRq account, Guid requesterId) => QbObjectProcessor(account, requesterId);
        #endregion Public Methods
    }
}