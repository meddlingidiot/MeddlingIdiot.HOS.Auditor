using MeddlingIdiot.HOS.Ruleset;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Utilities;

namespace MeddlingIdiot.HOS.RestTimelineBuilders
{
    internal class RestTimelinePairerUsaPrimary : IRestTimelinePairer
    {
        private readonly IRuleDefinition _ruleDefinition;
        private readonly ITimelineNavigator _navigator;
        private readonly ILogger _logger;

        public RestTimelinePairerUsaPrimary(ILogger logger, IRuleDefinition ruleDefinition, ITimelineNavigator navigator)
        {
            _logger = logger;
            _navigator = navigator;
            _ruleDefinition = ruleDefinition;
        }

        public void PairSleeperSplits(CancellationToken cancellationToken = default)
        {
            _logger.Debug(LoggerCategories.Pairing, "--- Pair Sleeper Splits ---------------------------------");

            // 7+ and 2+ split
            // 7/3 consecutive sleep/passenger (How is this logged?)
            // 7+ sleeper can pair with 2+ off-duty or sleeper or unknown
            // If multiple pairings possible, the one with the fewest violations wins
            _navigator.JumpTo(DateTime.MinValue);
            _logger.Debug(LoggerCategories.Pairing, "JumpTo: " + _navigator.StartTimestamp);
            var lastPrimaryInsertedAt = DateTime.MinValue;
            do
            {

                if (_navigator.CurrentRestMoment.IsPrimary)
                {
                    var primaryRestMoment = _navigator.CurrentRestMoment;
                    if (_navigator.CurrentRestMoment.Timestamp > lastPrimaryInsertedAt)
                    {
                        _logger.Debug(LoggerCategories.Pairing, "Primary Rest Found At:  " + _navigator.CurrentRestMoment.Timestamp);
                        _navigator.Upsert(new RestMoment(_navigator.CurrentRestMoment.Timestamp,
                            _navigator.CurrentRestMoment.Duration,
                            false, false,
                            true,
                            _navigator.CurrentRestMoment.IsPrimary,
                            true,
                            _navigator.CurrentRestMoment.DriverIdNumber,
                            _navigator.CurrentRestMoment.TruckNumber));
                        FindPriorPair(primaryRestMoment);
                        FindNextPair(primaryRestMoment);

                        lastPrimaryInsertedAt = _navigator.CurrentRestMoment.Timestamp;
                    }
                }
                _navigator.Next();
                _logger.Debug(LoggerCategories.Pairing, "Next(): " + _navigator.StartTimestamp + " duty: " + _navigator.DutyStatus);
            } while (!_navigator.IsEndOfTime() && !cancellationToken.IsCancellationRequested);

        }

        public void FindPriorPair(RestMoment currentPrimaryRest)
        {
            var saveStart = _navigator.Start;

            _logger.Debug(LoggerCategories.Pairing, "Looking for Prior Pair...");

            //We on first sleeper segment
            if (_navigator.CurrentRestMoment.Timestamp == DateTime.MinValue)
            {
                _logger.Debug(LoggerCategories.Pairing, "   No prior segments. we are on the first one. (BOT)");
                return;
            }

            do
            {
                _navigator.JumpToPriorRest();

                //No prior sleeper segments
                if (_navigator.CurrentRestMoment.Timestamp == DateTime.MinValue)
                {
                    _logger.Debug(LoggerCategories.Pairing, "   No prior segments. we are on the first one.");
                    _navigator.JumpTo(saveStart.Timestamp);
                    return;
                }

                //No secondary sleeper segments to pair with
                if ((_navigator.CurrentRestMoment.IsPrimary) || 
                    (_navigator.CurrentRestMoment.IsFullRest) || 
                    (_navigator.CurrentRestMoment.IsGlobalReset))
                {
                    _logger.Debug(LoggerCategories.Pairing, "   Prior rest segment is a Primary or Full Rest or Global Reset and cannot pair.");
                    _navigator.JumpTo(saveStart.Timestamp);
                    return;
                }

                if ((_navigator.CurrentRestMoment.IsQualified) &&
                    ((currentPrimaryRest.Duration + _navigator.CurrentRestMoment.Duration) >= _ruleDefinition.MinFullRest))
                {
                    _logger.Debug(LoggerCategories.Pairing,
                        "    FOUND Secondary Rest Found At:  " + _navigator.CurrentRestMoment.Timestamp);
                    var currentSplitMoment = _navigator.CurrentRestMoment;
                    var splitRestMoment = new RestMoment(currentSplitMoment.Timestamp,
                        currentSplitMoment.Duration,
                        false, false,
                        true, //IsQualified
                        false, //IsPrimary
                        true, //IsPaired
                        currentSplitMoment.DriverIdNumber,
                        currentSplitMoment.TruckNumber);

                    _navigator.Upsert(splitRestMoment);
                }

                _logger.Debug(LoggerCategories.Pairing, "Prior(): " + _navigator.StartTimestamp + " duty: " + _navigator.DutyStatus);
            } while (!_navigator.IsBeginningOfTime());

            _navigator.JumpTo(saveStart.Timestamp);
        }

        private void FindNextPair(RestMoment currentPrimaryRest)
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
                if ((_navigator.CurrentRestMoment.IsPrimary) || 
                    (_navigator.CurrentRestMoment.IsFullRest) || 
                    (_navigator.CurrentRestMoment.IsGlobalReset))
                {
                    _logger.Debug(LoggerCategories.Pairing, "   Next rest segment is a Primary or Full rest or Global reset and cannot pair.");
                    _navigator.JumpTo(saveStart.Timestamp);
                    return;
                }

                if ((_navigator.CurrentRestMoment.IsQualified) &&
                    ((currentPrimaryRest.Duration + _navigator.CurrentRestMoment.Duration) >= _ruleDefinition.MinFullRest)) 
                {
                    _logger.Debug(LoggerCategories.Pairing,
                        "    FOUND Secondary Rest Found At:  " + _navigator.CurrentRestMoment.Timestamp);
                    var currentSplitMoment = _navigator.CurrentRestMoment;
                    var pairedRestMoment = new RestMoment(currentSplitMoment.Timestamp,
                        currentSplitMoment.Duration,
                        false, false, true,
                        currentSplitMoment.IsPrimary,
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