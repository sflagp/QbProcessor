using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public methods
        public string GetJournalEntries() => QbObjectProcessor(new JournalEntryQueryRq(), Guid.NewGuid());
        public string GetJournalEntries(Guid requesterId) => QbObjectProcessor(new JournalEntryQueryRq(), requesterId);
        public string GetJournalEntries(JournalEntryQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetJournalEntries(JournalEntryQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        public string AddJournalEntry(JournalEntryAddRq journalEntry) => QbObjectProcessor(journalEntry, Guid.NewGuid());
        public string AddJournalEntry(JournalEntryAddRq journalEntry, Guid requesterId) => QbObjectProcessor(journalEntry, requesterId);
        #endregion
    }
}
