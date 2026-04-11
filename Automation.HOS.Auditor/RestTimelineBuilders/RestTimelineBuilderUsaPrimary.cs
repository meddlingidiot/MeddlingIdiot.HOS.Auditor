using Automation.HOS.Ruleset;
using Automation.HOS.TimelineNavigator;
using Automation.HOS.TimelineNavigator.Moments;
using Automation.HOS.TimelineNavigator.Utilities;

namespace Automation.HOS.RestTimelineBuilders
{
    internal class RestTimelineBuilderUsaPrimary : IRestTimelineBuilder
    {
        private readonly IRuleDefinition _ruleDefinition;
        private readonly ITimelineNavigator _navigator;
        private readonly ILogger _logger;

        public RestTimelineBuilderUsaPrimary(ILogger logger, IRuleDefinition ruleDefinition, ITimelineNavigator navigator)
        {
            _logger = logger;
            _navigator = navigator;
            _ruleDefinition = ruleDefinition;
        }

        public void BuildTimeline()
        {
            // How do I know if a Primary Rest is part of a Full Rest?
            // every rest keeps track of splittable rest and total rest.
            // if total rest is less then FullRest, and
            // splittable rest is greater than MinPrimarySpitRestSize, then it is a primary rest.

            // 7+ and 2+ split
            // 7/3 consecutive sleep/passenger (How is this logged?)
            // 7+ sleeper can pair with 2+ off-duty or sleeper or unknown
            // If multiple pairings possible, the one with the fewest violations wins
//SETUP ------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------

            var primarySplitAdded = false;
            var primaryOptions = new RestAccumulatorOptions(_ruleDefinition.PrimaryRestDutyStatuses, _ruleDefinition.MinPrimarySplitRest, _ruleDefinition.MinFullRest, ((startTimestamp, finishTimestamp, driverIdNumber,
                truckNumber) =>
            {
                //Create and insert splitRestMoment.
                _logger.Debug(LoggerCategories.RestBuilding, $"Add Moments at (start): {startTimestamp} and : {finishTimestamp}.");
                _navigator.Upsert(new RestMoment(startTimestamp, finishTimestamp - startTimestamp, false, false, true, true, false, driverIdNumber, truckNumber));
                _navigator.Upsert(new RestMoment(finishTimestamp, TimeSpan.Zero,false, false, false, false, false, driverIdNumber, truckNumber));
                primarySplitAdded = true;

            }));
            var secondaryOptions = new RestAccumulatorOptions(_ruleDefinition.SecondaryRestDutyStatuses, _ruleDefinition.MinSecondarySplitRest, _ruleDefinition.MinFullRest, ((startTimestamp, finishTimestamp, driverIdNumber,
                truckNumber) =>
            {
                //Create and insert splitRestMoment.
                _logger.Debug(LoggerCategories.RestBuilding, "(secondary)Add Moment: " + startTimestamp);
                if (!primarySplitAdded)
                {
                    _navigator.Upsert(new RestMoment(startTimestamp, finishTimestamp - startTimestamp, false, false, true, false, false, driverIdNumber, truckNumber));
                    _navigator.Upsert(new RestMoment(finishTimestamp, TimeSpan.Zero, false, false, false,false, false, driverIdNumber, truckNumber));
                }
            }));
            var fullRestOptions = new RestAccumulatorOptions(_ruleDefinition.FullRestDutyStatuses, _ruleDefinition.MinFullRest, _ruleDefinition.GlobalReset,((startTimestamp, finishTimestamp, driverIdNumber,
                truckNumber) =>
            {
                _logger.Debug(LoggerCategories.RestBuilding, "(full rest) Add Moment: " + startTimestamp);
                _navigator.Upsert(new RestMoment(startTimestamp, finishTimestamp - startTimestamp, false, true, false, false, false, driverIdNumber, truckNumber));
                _navigator.Upsert(new RestMoment(finishTimestamp, TimeSpan.Zero, false, false, false, false, false, driverIdNumber, truckNumber));
            }));
            var globalResetOptions = new RestAccumulatorOptions(_ruleDefinition.GlobalResetDutyStatuses, _ruleDefinition.GlobalReset, TimeSpan.MaxValue, ((startTimestamp, finishTimestamp, driverIdNumber,
                truckNumber) =>
            {
                _logger.Debug(LoggerCategories.RestBuilding, "(global reset) Add Moment: " + startTimestamp);
                _navigator.Upsert(new RestMoment(startTimestamp, finishTimestamp - startTimestamp, true, true, false, false, false, driverIdNumber, truckNumber));
                _navigator.Upsert(new RestMoment(finishTimestamp, TimeSpan.Zero, false, false, false, false, false, driverIdNumber, truckNumber));
            }));

//MAIN LOOP-----------------------------------------------------------------------
//-------------------------------------------------------------------------------
            var primaryRestAccumulator = new RestAccumulator(_navigator, primaryOptions, _logger);
            var secondaryRestAccumulator = new RestAccumulator(_navigator, secondaryOptions, _logger);
            var fullResetAccumulator = new RestAccumulator(_navigator, fullRestOptions, _logger);
            var globalResetAccumulator = new RestAccumulator(_navigator, globalResetOptions, _logger);
            
            _navigator.JumpTo(DateTime.MinValue);
            _logger.Debug(LoggerCategories.RestBuilding, "JumpTo: " + _navigator.StartTimestamp);
            do
            {
                primarySplitAdded = false;

                primaryRestAccumulator.Accumulate(_navigator.StartTimestamp, _navigator.Length, _navigator.DutyStatus);
                secondaryRestAccumulator.Accumulate(_navigator.StartTimestamp, _navigator.Length, _navigator.DutyStatus);
                fullResetAccumulator.Accumulate(_navigator.StartTimestamp, _navigator.Length, _navigator.DutyStatus);
                globalResetAccumulator.Accumulate(_navigator.StartTimestamp, _navigator.Length, _navigator.DutyStatus);

                _navigator.Next();
                _logger.Debug(LoggerCategories.RestBuilding, "Next(): " + _navigator.StartTimestamp + " duty: " + _navigator.DutyStatus);
            } while (!_navigator.IsEndOfTime());

        }
    }
}
