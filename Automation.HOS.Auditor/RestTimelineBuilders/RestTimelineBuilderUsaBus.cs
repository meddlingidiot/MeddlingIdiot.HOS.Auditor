using Automation.HOS.Ruleset;
using Automation.HOS.TimelineNavigator;
using Automation.HOS.TimelineNavigator.Moments;
using Automation.HOS.TimelineNavigator.Utilities;

namespace Automation.HOS.RestTimelineBuilders
{
    internal class RestTimelineBuilderUsaBus : IRestTimelineBuilder
    {
        private readonly IRuleDefinition _ruleDefinition;
        private readonly ITimelineNavigator _navigator;
        private readonly ILogger _logger;

        public RestTimelineBuilderUsaBus(ILogger logger, IRuleDefinition ruleDefinition, ITimelineNavigator navigator)
        {
            _logger = logger;
            _navigator = navigator;
            _ruleDefinition = ruleDefinition;
        }

        public void BuildTimeline()
        {
//SETUP ------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------

            var splitRestOptions = new RestAccumulatorOptions(_ruleDefinition.SplitRestDutyStatuses, _ruleDefinition.MinSplitRest, _ruleDefinition.MinFullRest, ((startTimestamp, finishTimestamp, driverIdNumber,
                truckNumber) =>
            {
                //Create and insert splitRestMoment.
                _logger.Debug(LoggerCategories.RestBuilding, $"Add Moments at (start): {startTimestamp} and : {finishTimestamp}.");
                _navigator.Upsert(new RestMoment(startTimestamp, finishTimestamp - startTimestamp, false, false, true, true, false, driverIdNumber, truckNumber));
                _navigator.Upsert(new RestMoment(finishTimestamp, TimeSpan.Zero,false, false, false, false, false, driverIdNumber, truckNumber));
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
            var splitRestAccumulator = new RestAccumulator(_navigator, splitRestOptions, _logger);
            var fullResetAccumulator = new RestAccumulator(_navigator, fullRestOptions, _logger);
            var globalResetAccumulator = new RestAccumulator(_navigator, globalResetOptions, _logger);
            
            _navigator.JumpTo(DateTime.MinValue);
            _logger.Debug(LoggerCategories.RestBuilding, "JumpTo: " + _navigator.StartTimestamp);
            do
            {

                splitRestAccumulator.Accumulate(_navigator.StartTimestamp, _navigator.Length, _navigator.DutyStatus);
                fullResetAccumulator.Accumulate(_navigator.StartTimestamp, _navigator.Length, _navigator.DutyStatus);
                globalResetAccumulator.Accumulate(_navigator.StartTimestamp, _navigator.Length, _navigator.DutyStatus);

                _navigator.Next();
                _logger.Debug(LoggerCategories.RestBuilding, "Next(): " + _navigator.StartTimestamp + " duty: " + _navigator.DutyStatus);
            } while (!_navigator.IsEndOfTime());

        }
    }
}
