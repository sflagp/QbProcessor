using QbModels;
using System;

namespace QBProcessor
{
    public partial class QbProcessor
    {
        #region Public methods
        public string GetTimeTracking() => QbObjectProcessor(new TimeTrackingQueryRq(), Guid.NewGuid());
        public string GetTimeTracking(Guid requesterId) => QbObjectProcessor(new TimeTrackingQueryRq(), requesterId);
        public string GetTimeTracking(TimeTrackingQueryRq query) => QbObjectProcessor(query, Guid.NewGuid());
        public string GetTimeTracking(TimeTrackingQueryRq query, Guid requesterId) => QbObjectProcessor(query, requesterId);

        public string AddTimeTracking(TimeTrackingAddRq timeEntry) => QbObjectProcessor(timeEntry, Guid.NewGuid());
        public string AddTimeTracking(TimeTrackingAddRq timeEntry, Guid requesterId) => QbObjectProcessor(timeEntry, requesterId);
        #endregion
    }
}
