using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public methods
        internal string GetTimeTracking() => QbObjectProcessor(new TimeTrackingQueryRq(), Guid.NewGuid());
        internal string GetTimeTracking(Guid requesterId) => QbObjectProcessor(new TimeTrackingQueryRq(), requesterId);
        internal string GetTimeTracking(TimeTrackingQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        internal string GetTimeTracking(TimeTrackingQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        internal string AddTimeTracking(TimeTrackingAddRq timeEntry) => QbObjectProcessor(timeEntry, Guid.NewGuid());
        internal string AddTimeTracking(TimeTrackingAddRq timeEntry, Guid requesterId) => QbObjectProcessor(timeEntry, requesterId);
        #endregion
    }
}
