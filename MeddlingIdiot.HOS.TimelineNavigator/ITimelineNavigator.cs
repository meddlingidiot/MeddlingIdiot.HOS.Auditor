using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Segments;
using MeddlingIdiot.HOS.TimelineNavigator.Utilities;

namespace MeddlingIdiot.HOS.TimelineNavigator
{
    public interface ITimelineNavigator : IEnumerable<Moment>
    {
        void Add(DutyStatusChangeMoment dutyChangeMoment);
        void Add(GpsMoment moment);
        void Add(EngineBusMoment moment);
        void Add(AnchorMoment moment);
        void Add(RestMoment moment);
        void Add(EventMoment moment);
        void Upsert(DutyStatusChangeMoment dutyChangeMoment);
        void Upsert(GpsMoment moment);
        void Upsert(EngineBusMoment moment);
        void Upsert(AnchorMoment moment);
        void Upsert(RestMoment moment);
        void Upsert(EventMoment moment); 
        void Upsert(ShiftExtensionSegment segment);
        void Upsert(AgriculturalExceptionSegment segment);
        void Upsert(AdverseConditionsSegment segment);

        void Initialize();

        void JumpTo(DateTime searchTimestamp);
        public void JumpToPriorRest(bool? paired = null);
        public void JumpToNextRest(bool? paired = null);
        public void JumpToPriorShiftExtension(bool? isExtended = null);
        public void JumpToNextShiftExtension(bool? isExtended = null);

        void Next();
        void Prior();

        bool IsBeginningOfTime();
        bool IsEndOfTime();
        bool IsEndOfSleeperSplits();
        bool IsEndOfShiftExtensions();

        Moment Start { get; }
        DateTime StartTimestamp { get; }
        Moment Finish { get; }
        DateTime FinishTimestamp { get; }
        TimeSpan Length { get; }

        DutyStatusChangeMoment CurrentDutyStatusChangeMoment { get; }
        DutyStatus DutyStatus { get; }
        decimal Odometer { get; }
        double Longitude { get; }
        double Latitude { get; }

        string? DriverIdNumber { get; }
        string? TruckNumber { get; }

        GpsMoment CurrentGpsMoment { get; }
        EngineBusMoment CurrentEngineBusMoment { get; }

        RestMoment CurrentRestMoment { get; }

        bool IsStartOfDay { get; }
        DateTime StartOfDay(DateTime timestamp);
        DateTime CurrentDay { get; }
        
        bool IsShiftExtended { get; }
        bool IsAgriculturalExceptionEnabled { get; }
        bool IsAdverseConditionsEnabled { get; }

        void DumpSplitRestTimeline(ILogger logger);
    }
}
