using MeddlingIdiot.HOS.Ruleset;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Utilities;

namespace MeddlingIdiot.HOS.RestTimelineBuilders
{
    internal class RestTimelinePairerUsaBus : IRestTimelinePairer
    {
        private readonly IRuleDefinition _ruleDefinition;
        private readonly ITimelineNavigator _navigator;
        private readonly ILogger _logger;
        
        //private RestMoment? _currentQualifiedRest;

        public RestTimelinePairerUsaBus(ILogger logger, IRuleDefinition ruleDefinition, ITimelineNavigator navigator)
        {
            _logger = logger;
            _navigator = navigator;
            _ruleDefinition = ruleDefinition;
        }

        public void PairSleeperSplits(CancellationToken cancellationToken = default)
        {
            _logger.Debug(LoggerCategories.Pairing, "--- Pair Sleeper Splits ---------------------------------");

            _navigator.JumpTo(DateTime.MinValue);
            _logger.Debug(LoggerCategories.Pairing, "JumpTo: " + _navigator.StartTimestamp);
            var lastPrimaryInsertedAt = DateTime.MinValue;
            do
            {

                if (_navigator.CurrentRestMoment.IsQualified)
                {
                    var qualifiedSplitRestMoment = _navigator.CurrentRestMoment;
                    if (_navigator.CurrentRestMoment.Timestamp > lastPrimaryInsertedAt)
                    {
                        _logger.Debug(LoggerCategories.Pairing, "Qualified Rest Found At:  " + _navigator.CurrentRestMoment.Timestamp);
                         FindNextPair(qualifiedSplitRestMoment);

                        lastPrimaryInsertedAt = _navigator.CurrentRestMoment.Timestamp;
                    }
                }
                _navigator.Next();
                _logger.Debug(LoggerCategories.Pairing, "Next(): " + _navigator.StartTimestamp + " duty: " + _navigator.DutyStatus);
            } while (!_navigator.IsEndOfTime() && !cancellationToken.IsCancellationRequested);

        }

        private void FindNextPair(RestMoment currentQualifiedRest)
        {

            var saveStart = _navigator.Start;

            _logger.Debug(LoggerCategories.Pairing, "Looking for Next Pair...");

            //We on last segment
            if (_navigator.FinishTimestamp == DateTime.MaxValue)
            {
                _logger.Debug(LoggerCategories.Pairing, "   No next segments. we are on the last one. (BOT)");
                return;
            }

            do
            {
                _navigator.JumpToNextRest();

                //No next sleeper segments
                if (_navigator.FinishTimestamp == DateTime.MaxValue)
                {
                    _logger.Debug(LoggerCategories.Pairing, "   No future segments. we are on the last one.");
                    _navigator.JumpTo(saveStart.Timestamp);
                    return;
                }

                //No secondary sleeper segments to pair with
                if ((_navigator.CurrentRestMoment.IsFullRest) || 
                    (_navigator.CurrentRestMoment.IsGlobalReset))
                {
                    _logger.Debug(LoggerCategories.Pairing, "   Next rest segment is a Primary or Full rest or Global reset and cannot pair.");
                    _navigator.JumpTo(saveStart.Timestamp);
                    return;
                }

                if ((_navigator.CurrentRestMoment.IsQualified) &&
                    ((currentQualifiedRest.Duration + _navigator.CurrentRestMoment.Duration) >= _ruleDefinition.MinFullRest)) 
                {
                    _logger.Debug(LoggerCategories.Pairing,
                        "    FOUND Paired Rest Found At:  " + _navigator.CurrentRestMoment.Timestamp);
                    var currentSplitMoment = _navigator.CurrentRestMoment;

                    _navigator.Upsert(new RestMoment(currentQualifiedRest.Timestamp,
                        currentQualifiedRest.Duration,
                        false, false,
                        true,
                        false,
                        true,
                        _navigator.CurrentRestMoment.DriverIdNumber,
                        _navigator.CurrentRestMoment.TruckNumber));

                    var pairedRestMoment = new RestMoment(currentSplitMoment.Timestamp,
                        currentSplitMoment.Duration,
                        false, false, true,
                        false,
                        true, //IsPaired
                        currentSplitMoment.DriverIdNumber,
                        currentSplitMoment.TruckNumber);

                    _navigator.Upsert(pairedRestMoment);
                    return;
                }
            } while (!_navigator.IsEndOfSleeperSplits());

            _navigator.JumpTo(saveStart.Timestamp);
        }

    }
}