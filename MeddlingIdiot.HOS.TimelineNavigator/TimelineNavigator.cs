using System.Collections;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Segments;
using MeddlingIdiot.HOS.TimelineNavigator.Timelines;
using MeddlingIdiot.HOS.TimelineNavigator.Utilities;

namespace MeddlingIdiot.HOS.TimelineNavigator
{
    [Serializable]
    public sealed class TimelineNavigator : ITimelineNavigator
    {
        private readonly InMemoryLogger _logger;
        
        //These are all serializable fields
        private readonly StartOfDayTimelineOptions _startOfDayTimelineOptions;
        private readonly StartOfDayTimeline<StartOfDayMoment> _startOfDayTimeline;
        private readonly ITimeline<AnchorMoment> _anchorTimeline;
        private readonly ITimeline<JurisdictionMoment> _jurisdictionTimeline;
        private readonly ITimeline<DutyStatusChangeMoment> _dutyStatusChangeTimeline;
        private readonly ITimeline<GpsMoment> _gpsTimeline;
        private readonly ITimeline<EngineBusMoment> _engineBusTimeline;
        private readonly ITimeline<RestMoment> _restTimeline;
        private readonly ITimeline<ShiftExtensionMoment> _shiftExtensionTimeline;
        private readonly ITimeline<AgriculturalExceptionMoment> _agriculturalExceptionTimeline;
        private readonly ITimeline<AdverseConditionsMoment> _adverseConditionsTimeline;
        private readonly ITimeline<EventMoment> _eventTimeline;

        [NonSerialized] private readonly List<ITimeline> _timelines;

        [NonSerialized] private DutyStatusChangeMoment _currentDutyStatusChangeMoment = new DutyStatusChangeMoment();
        [NonSerialized] private GpsMoment _currentGpsMoment = new GpsMoment();
        [NonSerialized] private EngineBusMoment _currentEngineBusMoment = new EngineBusMoment();
        [NonSerialized] private RestMoment _currentRestMoment = new RestMoment(DateTime.MinValue, DateTime.MinValue, TimeSpan.Zero);

        [NonSerialized]
        private ShiftExtensionMoment _currentShiftExtensionMoment = new ShiftExtensionMoment(DateTime.MinValue);

        [NonSerialized] private AgriculturalExceptionMoment _currentAgriculturalExceptionMoment =
            new AgriculturalExceptionMoment(DateTime.MinValue);

        [NonSerialized]
        private AdverseConditionsMoment _currentAdverseConditionsMoment =
            new AdverseConditionsMoment(DateTime.MinValue);

        [NonSerialized] private EventMoment _currentEventMoment = new EventMoment(DateTime.MinValue);

        //[NonSerialized] private int _currentTimelineIndex = -1;

        public TimelineNavigator() : this(new StartOfDayTimelineOptions())
        {
            //Intentionally left blank Default is Midnight
        }

        public TimelineNavigator(StartOfDayTimelineOptions startOfDayTimelineOptions)
        {
            _logger = new InMemoryLogger();

            _startOfDayTimelineOptions = startOfDayTimelineOptions;

            _startOfDayTimeline = new StartOfDayTimeline<StartOfDayMoment>(_startOfDayTimelineOptions);
            _anchorTimeline = new Timeline<AnchorMoment>();
            _jurisdictionTimeline = new Timeline<JurisdictionMoment>();
            _dutyStatusChangeTimeline = new Timeline<DutyStatusChangeMoment>();
            _gpsTimeline = new Timeline<GpsMoment>();
            _engineBusTimeline = new Timeline<EngineBusMoment>();
            _restTimeline = new Timeline<RestMoment>();
            _shiftExtensionTimeline = new Timeline<ShiftExtensionMoment>();
            _agriculturalExceptionTimeline = new Timeline<AgriculturalExceptionMoment>();
            _adverseConditionsTimeline = new Timeline<AdverseConditionsMoment>();
            _eventTimeline = new Timeline<EventMoment>();

            _timelines = new List<ITimeline>();
            if (_startOfDayTimelineOptions.StopAtStartOfDay)
            {
                _timelines.Add(_startOfDayTimeline);
            }
            _timelines.Add((ITimeline)_anchorTimeline);
            _timelines.Add((ITimeline)_jurisdictionTimeline);
            _timelines.Add((ITimeline)_dutyStatusChangeTimeline);
            _timelines.Add((ITimeline)_gpsTimeline);
            _timelines.Add((ITimeline)_engineBusTimeline);
            _timelines.Add((ITimeline)_shiftExtensionTimeline);
            _timelines.Add((ITimeline)_agriculturalExceptionTimeline);
            _timelines.Add((ITimeline)_adverseConditionsTimeline);
            _timelines.Add((ITimeline)_eventTimeline);
        }

        public void Add(AnchorMoment moment)
        {
            _anchorTimeline.Add(moment);
            _startOfDayTimeline.Add(new StartOfDayMoment(moment.Timestamp, moment.DriverIdNumber, moment.TruckNumber));
        }
        
        public void Add(JurisdictionMoment moment)
        {
            _jurisdictionTimeline.Add(moment);
            _startOfDayTimeline.Add(new StartOfDayMoment(moment.Timestamp, moment.DriverIdNumber, moment.TruckNumber));
        }

        public void Add(DutyStatusChangeMoment moment)
        {
            _dutyStatusChangeTimeline.Add(moment);
            _startOfDayTimeline.Add(new StartOfDayMoment(moment.Timestamp, moment.DriverIdNumber, moment.TruckNumber));
        }

        public void Add(GpsMoment moment)
        {
            _gpsTimeline.Add(moment);
            _startOfDayTimeline.Add(new StartOfDayMoment(moment.Timestamp, moment.DriverIdNumber, moment.TruckNumber));
        }

        public void Add(EngineBusMoment moment)
        {
            _engineBusTimeline.Add(moment);
            _startOfDayTimeline.Add(new StartOfDayMoment(moment.Timestamp, moment.DriverIdNumber, moment.TruckNumber));
        }

        public void Add(RestMoment moment)
        {
            if (moment.Timestamp == DateTime.MinValue)
            {
                Upsert(moment);
                return;
            }

            _restTimeline.Add(moment);
            _startOfDayTimeline.Add(new StartOfDayMoment(moment.Timestamp, moment.DriverIdNumber, moment.TruckNumber));
        }

        public void Add(EventMoment moment)
        {
            if (moment.Timestamp == DateTime.MinValue)
            {
                Upsert(moment);
                return;
            }

            _eventTimeline.Add(moment);
            _startOfDayTimeline.Add(new StartOfDayMoment(moment.Timestamp, moment.DriverIdNumber, moment.TruckNumber));
        }

        public void Upsert(AnchorMoment moment)
        {
            _anchorTimeline.Upsert(moment);
            _startOfDayTimeline.Add(new StartOfDayMoment(moment.Timestamp, moment.DriverIdNumber, moment.TruckNumber));
            PublishData();
        }
        
        public void Upsert(JurisdictionMoment moment)
        {
            _jurisdictionTimeline.Upsert(moment);
            _startOfDayTimeline.Add(new StartOfDayMoment(moment.Timestamp, moment.DriverIdNumber, moment.TruckNumber));
            PublishData();
        }

        public void Upsert(DutyStatusChangeMoment moment)
        {
            _dutyStatusChangeTimeline.Upsert(moment);
            _startOfDayTimeline.Add(new StartOfDayMoment(moment.Timestamp, moment.DriverIdNumber, moment.TruckNumber));
            PublishData();
        }

        public void Upsert(GpsMoment moment)
        {
            _gpsTimeline.Upsert(moment);
            _startOfDayTimeline.Add(new StartOfDayMoment(moment.Timestamp, moment.DriverIdNumber, moment.TruckNumber));
            PublishData();
        }

        public void Upsert(EngineBusMoment moment)
        {
            _engineBusTimeline.Upsert(moment);
            _startOfDayTimeline.Add(new StartOfDayMoment(moment.Timestamp, moment.DriverIdNumber, moment.TruckNumber));
            PublishData();
        }

        public void Upsert(RestMoment moment)
        {
            _restTimeline.Upsert(moment);
            _startOfDayTimeline.Add(new StartOfDayMoment(moment.Timestamp, moment.DriverIdNumber, moment.TruckNumber));
            PublishData();
        }

        public void Upsert(EventMoment moment)
        {
            _eventTimeline.Upsert(moment);
            _startOfDayTimeline.Add(new StartOfDayMoment(moment.Timestamp, moment.DriverIdNumber, moment.TruckNumber));
            PublishData();
        }

        public void Upsert(ShiftExtensionSegment segment)
        {
            _shiftExtensionTimeline.Upsert(new ShiftExtensionMoment(segment.StartTimestamp, segment.IsExtended,
                segment.DriverIdNumber, segment.TruckNumber));
            _startOfDayTimeline.Add(new StartOfDayMoment(segment.StartTimestamp, segment.DriverIdNumber,
                segment.TruckNumber));
            _shiftExtensionTimeline.Upsert(new ShiftExtensionMoment(segment.FinishTimestamp, !segment.IsExtended,
                segment.DriverIdNumber, segment.TruckNumber));
            _startOfDayTimeline.Add(new StartOfDayMoment(segment.StartTimestamp, segment.DriverIdNumber,
                segment.TruckNumber));
            PublishData();
        }

        public void Upsert(AgriculturalExceptionSegment segment)
        {
            _agriculturalExceptionTimeline.Upsert(new AgriculturalExceptionMoment(segment.StartTimestamp,
                segment.IsEnabled, segment.DriverIdNumber, segment.TruckNumber));
            _startOfDayTimeline.Add(new StartOfDayMoment(segment.StartTimestamp, segment.DriverIdNumber,
                segment.TruckNumber));
            _agriculturalExceptionTimeline.Upsert(new AgriculturalExceptionMoment(segment.FinishTimestamp,
                !segment.IsEnabled, segment.DriverIdNumber, segment.TruckNumber));
            _startOfDayTimeline.Add(new StartOfDayMoment(segment.StartTimestamp, segment.DriverIdNumber,
                segment.TruckNumber));
            PublishData();
        }

        public void Upsert(AdverseConditionsSegment segment)
        {
            _adverseConditionsTimeline.Upsert(new AdverseConditionsMoment(segment.StartTimestamp, segment.IsEnabled,
                segment.DriverIdNumber, segment.TruckNumber));
            _startOfDayTimeline.Add(new StartOfDayMoment(segment.StartTimestamp, segment.DriverIdNumber,
                segment.TruckNumber));
            _adverseConditionsTimeline.Upsert(new AdverseConditionsMoment(segment.FinishTimestamp, !segment.IsEnabled,
                segment.DriverIdNumber, segment.TruckNumber));
            _startOfDayTimeline.Add(new StartOfDayMoment(segment.StartTimestamp, segment.DriverIdNumber,
                segment.TruckNumber));
            PublishData();
        }

        public void Initialize()
        {
            foreach (var timeline in _timelines)
            {
                timeline.Initialize();
            }
        }

        private Moment GetClosestOnOrBefore(DateTime searchTimestamp)
        {
            Moment? latestMoment = null;
            foreach (var timeline in _timelines)
            {
                timeline.MoveOnOrBefore(searchTimestamp);
                if (latestMoment == null)
                {
                    latestMoment = timeline.BaseMoment;
                    continue;
                }

                if ((timeline.BaseMoment.Timestamp > latestMoment.Timestamp) &&
                    (timeline.BaseMoment.Timestamp > DateTime.MinValue))
                {
                    latestMoment = timeline.BaseMoment;
                }
            }

            //The following can't get coverage because we always add default _timelines to begin with.
            if (latestMoment == null)
                Start = new DutyStatusChangeMoment();
            return latestMoment!;
        }

        private Moment GetClosestAfter(DateTime searchTimestamp)
        {
            Moment earliestMoment = new DutyStatusChangeMoment(DateTime.MaxValue, DutyStatus.Unknown);
            var shortestDistanceBetweenMoments = TimeSpan.MaxValue;
            foreach (var timeline in _timelines)
            {
                timeline.MoveOnOrAfter(Start.Timestamp.AddSeconds(1));
                if (timeline.BaseMoment.Timestamp <= searchTimestamp) continue; //Only consider moments after the search timestamp
                var differenceBetweenCurrentAndEarliest =
                    timeline.BaseMoment.Timestamp - Start.Timestamp;
                if (earliestMoment.Timestamp == DateTime.MaxValue)
                {
                    shortestDistanceBetweenMoments = differenceBetweenCurrentAndEarliest;
                    earliestMoment = timeline.BaseMoment;
                    continue;
                }


                if ((differenceBetweenCurrentAndEarliest > TimeSpan.Zero) &&
                    (differenceBetweenCurrentAndEarliest) < shortestDistanceBetweenMoments)
                {
                    shortestDistanceBetweenMoments = differenceBetweenCurrentAndEarliest;
                    earliestMoment = timeline.BaseMoment;
                }
            }

            return earliestMoment;

        }

        private void DoJumpTo(DateTime searchTimestamp)
        {
            Start = (Moment)GetClosestOnOrBefore(searchTimestamp).Clone();
            Finish = (Moment)GetClosestAfter(Start.Timestamp).Clone();
            if (IsEndOfTime())
            {
                Finish = new DutyStatusChangeMoment(DateTime.MaxValue, DutyStatus.Unknown);
            }
        }

        public void JumpTo(DateTime searchTimestamp)
        {
            //_currentTimelineIndex = -1;
            DoJumpTo(searchTimestamp);
            PublishData();
        }

        //TODO: This should be tested.
        public void JumpToPriorRest(bool? paired = null)
        {
            if ((paired == null))
            {
                _restTimeline.Prior();
                JumpTo(_restTimeline.CurrentMoment.Timestamp);
                return;
            }

            do
            {
                _restTimeline.Prior();
            } while ((_restTimeline.CurrentMoment.IsPaired != paired) && (!_restTimeline.IsBeginningOfTime()));

            JumpTo(_restTimeline.CurrentMoment.Timestamp);
        }

        //TODO: This should be tested.
        public void JumpToNextRest(bool? paired = null)
        {
            if ((paired == null))
            {
                _restTimeline.Next();
                JumpTo(_restTimeline.CurrentMoment.Timestamp.AddSeconds(1));
                return;
            }

            do
            {
                _restTimeline.Next();
            } while ((_restTimeline.CurrentMoment.IsPaired != paired) && (!_restTimeline.IsEndOfTime()));

            JumpTo(_restTimeline.CurrentMoment.Timestamp);
        }

        //TODO: This should be tested.
        public void JumpToPriorShiftExtension(bool? isExtended = null)
        {
            if ((isExtended == null))
            {
                _shiftExtensionTimeline.Prior();
                JumpTo(_shiftExtensionTimeline.CurrentMoment.Timestamp);
                return;
            }

            do
            {
                _shiftExtensionTimeline.Prior();
            } while ((_shiftExtensionTimeline.CurrentMoment.IsExtended != isExtended) &&
                     (!_shiftExtensionTimeline.IsBeginningOfTime()));

            JumpTo(_shiftExtensionTimeline.CurrentMoment.Timestamp);
        }

        //TODO: This should be tested.
        public void JumpToNextShiftExtension(bool? isExtended = null)
        {
            if ((isExtended == null))
            {
                _shiftExtensionTimeline.Next();
                JumpTo(_shiftExtensionTimeline.CurrentMoment.Timestamp.AddSeconds(1));
                return;
            }

            do
            {
                _shiftExtensionTimeline.Next();
            } while ((_shiftExtensionTimeline.CurrentMoment.IsExtended != isExtended) &&
                     (!_shiftExtensionTimeline.IsEndOfTime()));

            JumpTo(_shiftExtensionTimeline.CurrentMoment.Timestamp);
        }

        // Walks forward from the current position until <paramref name="predicate"/> is satisfied and
        // returns the StartTimestamp of that moment, without disturbing the caller's current position.
        // If the end of time is reached without a match, the last StartTimestamp seen is returned.
        public DateTime PeekAhead(Func<ITimelineNavigator, bool> predicate)
        {
            var savedTimestamp = StartTimestamp;
            try
            {
                while (!IsEndOfTime())
                {
                    Next();
                    if (predicate(this))
                        return StartTimestamp;
                }

                return StartTimestamp;
            }
            finally
            {
                JumpTo(savedTimestamp);
            }
        }

        public void Next()
        {
            if (Finish.Timestamp == DateTime.MaxValue)
                return;
            Start = Finish;
            Finish = (Moment)GetClosestAfter(Start.Timestamp).Clone();
            if (IsEndOfTime())
                Finish = new DutyStatusChangeMoment(DateTime.MaxValue, DutyStatus.Unknown);

            PublishData();
        }

        public void Prior()
        {
            if (Start.Timestamp == DateTime.MinValue)
                return;
            Finish = Start;
            Start = (Moment)GetClosestOnOrBefore(Start.Timestamp.AddSeconds(-1)).Clone();
            PublishData();

        }

        public bool IsBeginningOfTime()
        {
            return Start.Timestamp == DateTime.MinValue;
        }

        public bool IsEndOfTime()
        {
            var result = GetClosestAfter(Start.Timestamp);
            return result.Timestamp == DateTime.MaxValue;
        }

        public bool IsEndOfSleeperSplits()
        {
            return _restTimeline.IsEndOfTime();
        }

        public bool IsEndOfShiftExtensions()
        {
            return _shiftExtensionTimeline.IsEndOfTime();
        }

        private void PublishData()
        {
            StartTimestamp = Start.Timestamp;
            FinishTimestamp = Finish.Timestamp;
            DriverIdNumber = Start.DriverIdNumber;
            TruckNumber = Start.TruckNumber;
            
            _dutyStatusChangeTimeline.MoveOnOrBefore(Start.Timestamp);
            _currentDutyStatusChangeMoment = (DutyStatusChangeMoment)_dutyStatusChangeTimeline.CurrentMoment.Clone();
            
            _gpsTimeline.MoveOnOrBefore(Start.Timestamp);
            _currentGpsMoment = (GpsMoment)_gpsTimeline.CurrentMoment.Clone();
            
            _engineBusTimeline.MoveOnOrBefore(Start.Timestamp);
            _currentEngineBusMoment = (EngineBusMoment)_engineBusTimeline.CurrentMoment.Clone();

            _restTimeline.MoveOnOrBefore(Start.Timestamp);
            _currentRestMoment = (RestMoment)_restTimeline.CurrentMoment.Clone();

            _shiftExtensionTimeline.MoveOnOrBefore(Start.Timestamp);
            _currentShiftExtensionMoment = (ShiftExtensionMoment)_shiftExtensionTimeline.CurrentMoment.Clone();
            
            _agriculturalExceptionTimeline.MoveOnOrBefore(Start.Timestamp);
            _currentAgriculturalExceptionMoment = (AgriculturalExceptionMoment)_agriculturalExceptionTimeline.CurrentMoment.Clone();
            
            _adverseConditionsTimeline.MoveOnOrBefore(Start.Timestamp);
            _currentAdverseConditionsMoment = (AdverseConditionsMoment)_adverseConditionsTimeline.CurrentMoment.Clone();
            
            _eventTimeline.MoveOnOrBefore(Start.Timestamp);
            _currentEventMoment = (EventMoment)_eventTimeline.CurrentMoment.Clone();
        }

        public Moment Start { get; private set; } = new DutyStatusChangeMoment();
        public DateTime StartTimestamp { get; private set; } = DateTime.MinValue;
        public Moment Finish { get; private set; } = new DutyStatusChangeMoment(DateTime.MaxValue, DutyStatus.Unknown); 
        public DateTime FinishTimestamp { get; private set; } = DateTime.MaxValue;
        public TimeSpan Length => FinishTimestamp - StartTimestamp;

        public DutyStatusChangeMoment CurrentDutyStatusChangeMoment => _currentDutyStatusChangeMoment;
        public DutyStatus DutyStatus => _currentDutyStatusChangeMoment.CurrentDutyStatus;
        public decimal Odometer => _currentEngineBusMoment.Odometer;
        public double Longitude => _currentGpsMoment.Longitude;
        public double Latitude => _currentGpsMoment.Latitude;

        public string? DriverIdNumber { get; private set; }
        public string? TruckNumber { get; private set; }

        public GpsMoment CurrentGpsMoment => _currentGpsMoment;
        public EngineBusMoment CurrentEngineBusMoment => _currentEngineBusMoment;

        public RestMoment CurrentRestMoment => _currentRestMoment;

        public bool IsStartOfDay => _startOfDayTimeline.IsStartOfDay(StartTimestamp);

        public DateTime StartOfDay(DateTime timestamp)
        {
            return _startOfDayTimeline.StartOfDay(timestamp);
        }
        public DateTime CurrentDay => StartOfDay(StartTimestamp);

        public bool IsShiftExtended => _currentShiftExtensionMoment.IsExtended;
        public bool IsAgriculturalExceptionEnabled => _currentAgriculturalExceptionMoment.IsEnabled;
        public bool IsAdverseConditionsEnabled => _currentAdverseConditionsMoment.IsEnabled;

        public void DumpSplitRestTimeline(ILogger logger)
        {
            _restTimeline.MoveOnOrBefore(DateTime.MinValue);
            do
            {
                logger.Debug(LoggerCategories.Pairing, "SplitRestMoment: " + _restTimeline.CurrentMoment);
                _restTimeline.Next();
            } while (!_restTimeline.IsEndOfTime());

        }

        public List<RestMoment> GetRestTimelineMoments()
        {
            List<RestMoment> restMoments = new List<RestMoment>();
            _restTimeline.MoveOnOrBefore(DateTime.MinValue);
            _restTimeline.Next(); // skip sentinel at DateTime.MinValue
            while (_restTimeline.CurrentMoment.Timestamp > DateTime.MinValue)
            {
                restMoments.Add((RestMoment)_restTimeline.CurrentMoment.Clone());
                if (_restTimeline.IsEndOfTime())
                    break;
                _restTimeline.Next();
            }

            return restMoments;
        }

        public void ClearRestTimeline()
        {
            _restTimeline.Clear();
            _currentRestMoment = new RestMoment(DateTime.MinValue, DateTime.MinValue, TimeSpan.Zero);
        }

        public List<JurisdictionMoment> GetJurisdictionMoments()
        {
            List<JurisdictionMoment> jurisdictionMoments = new List<JurisdictionMoment>();
            _jurisdictionTimeline.MoveOnOrBefore(DateTime.MinValue);
            _jurisdictionTimeline.Next(); // skip sentinel at DateTime.MinValue
            while (_jurisdictionTimeline.CurrentMoment.Timestamp > DateTime.MinValue)
            {
                jurisdictionMoments.Add((JurisdictionMoment)_jurisdictionTimeline.CurrentMoment.Clone());
                if (_jurisdictionTimeline.IsEndOfTime())
                    break;
                _jurisdictionTimeline.Next();
            }

            return jurisdictionMoments;
        }

        public IEnumerator<Moment> GetEnumerator()
        {
            Initialize();
            JumpTo(DateTime.MinValue);

            while (!IsEndOfTime())
            {
                _logger.Debug("enumerator","GetEnumerator: " + Start);
                yield return Start;
                Next();
            }

            // Return the final moment at end of time
            yield return Start;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

     }
}
