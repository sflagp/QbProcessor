using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public methods
        internal string GetJournalEntries() => QbObjectProcessor(new JournalEntryQueryRq(), Guid.NewGuid());
        internal string GetJournalEntries(Guid requesterId) => QbObjectProcessor(new JournalEntryQueryRq(), requesterId);
        internal string GetJournalEntries(JournalEntryQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetJournalEntries(JournalEntryQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        internal string AddJournalEntry(JournalEntryAddRq journalEntry) => QbObjectProcessor(journalEntry, Guid.NewGuid());
        internal string AddJournalEntry(JournalEntryAddRq journalEntry, Guid requesterId) => QbObjectProcessor(journalEntry, requesterId);
        #endregion
    }
}
