using Automation.HOS.TimelineNavigator;
using Automation.HOS.TimelineNavigator.Moments;
using Automation.HOS.TimelineNavigator.Utilities;

namespace Automation.HOS
{
    public class GpsStreamToDutyStatusTimeline : IGpsStreamToDutyStatusTimeline
    {
        private const double AuditDrivingDistanceThreshold = 0.02;
        private readonly ILogger _logger;

        public GpsStreamToDutyStatusTimeline(ILogger logger)
        {
            _logger = logger;
        }

        public ITimelineNavigator ConvertGpsTimelineToDutyStatusTimeline(ITimelineNavigator gpsNavigator)
        {
            gpsNavigator = gpsNavigator ?? throw new ArgumentNullException(nameof(gpsNavigator));

            var dutyStatusNavigator = gpsNavigator;//.DeepClone();
            
            gpsNavigator.JumpTo(DateTime.MinValue); 
            
            //Can't do DateTime.MinValue so Next() once.
            gpsNavigator.Next();
            do
            {
                var currentGpsMoment = gpsNavigator.CurrentGpsMoment;
                var currentEngineBusMoment = gpsNavigator.CurrentEngineBusMoment;
                gpsNavigator.Next();
                var nextGpsMoment = gpsNavigator.CurrentGpsMoment;
                var nextEngineBusMoment = gpsNavigator.CurrentEngineBusMoment;
                var (distance, speed) = currentGpsMoment.DistanceTo(nextGpsMoment);
                var (odoDistance, odoSpeed) = currentEngineBusMoment.DistanceTo(nextEngineBusMoment);
                _logger.Debug(LoggerCategories.GpsToDutyStatus, $"Distance: {distance} speed: {speed} cur.Odo: {currentEngineBusMoment.Odometer} odoDistance: {odoDistance} odoSpeed: {odoSpeed} next.Odo: {nextEngineBusMoment.Odometer}");
                if (distance > AuditDrivingDistanceThreshold)
                {
                    dutyStatusNavigator.Add(new DutyStatusChangeMoment(
                        currentGpsMoment.Timestamp,
                        DutyStatus.Driving,
                        currentGpsMoment.DriverIdNumber, 
                        currentGpsMoment.TruckNumber));
                }
                else
                {
                    dutyStatusNavigator.Add(new DutyStatusChangeMoment(
                        currentGpsMoment.Timestamp,
                        DutyStatus.Sleeper,
                        currentGpsMoment.DriverIdNumber,
                        currentGpsMoment.TruckNumber));
                }

            } while (!gpsNavigator.IsEndOfTime());

            return dutyStatusNavigator;
        }
    }
}
