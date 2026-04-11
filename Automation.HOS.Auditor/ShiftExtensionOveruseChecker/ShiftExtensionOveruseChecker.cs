using Automation.HOS.Ruleset;
using Automation.HOS.TimelineNavigator;
using Automation.HOS.TimelineNavigator.Extensions;
using Automation.HOS.TimelineNavigator.Moments;
using Automation.HOS.TimelineNavigator.Utilities;
using Automation.HOS.Violations;

namespace Automation.HOS.ShiftExtensionOveruseChecker
{
    internal class ShiftExtensionOveruseChecker
    {
        private readonly ITimelineNavigator _navigator;
        private readonly IRuleDefinition _ruleDefinition;
        private readonly IViolationGateway _violationGateway;
        private readonly ILogger _logger;

        public ShiftExtensionOveruseChecker(ITimelineNavigator navigator, IRuleDefinition ruleDefinition, IViolationGateway violationGateway, ILogger logger)
        {
            _navigator = navigator;
            _ruleDefinition = ruleDefinition;
            _violationGateway = violationGateway;
            _logger = logger;
        }

        public void MainLoop(Moment startOfAuditWindow, Moment endOfAuditWindow)
        {
            var latestShiftExtensionStart = DateTime.MaxValue;
            var latestShiftExtensionEnd = DateTime.MaxValue;

            // Check for shift extension overuse
            _logger.Debug(LoggerCategories.ShiftExtAudit, "SHIFT EXTENSION AUDIT");
            _logger.Debug(LoggerCategories.ShiftExtAudit, "---------------------");
            _navigator.JumpTo(startOfAuditWindow.Timestamp);
            _logger.Debug(LoggerCategories.ShiftExtAudit, $"StartAt: {_navigator.StartTimestamp}");
            _navigator.JumpToPriorShiftExtension(true);
            _logger.Debug(LoggerCategories.ShiftExtAudit, $"Find prior shift ext: {_navigator.StartTimestamp}");
            do
            {
                if (_navigator.IsShiftExtended)
                {
                    //They are always added in pairs so we can just grab them in pairs.
                    latestShiftExtensionStart = _navigator.StartTimestamp;
                    _navigator.JumpToNextShiftExtension();
                    latestShiftExtensionEnd = _navigator.StartTimestamp;
                }
                _navigator.JumpToNextShiftExtension(true);
                //we either jumped or didn't at the end leaving the size a zero.
                _logger.Debug(LoggerCategories.ShiftExtAudit, $"jump to next ext: {_navigator.StartTimestamp}");
                if (latestShiftExtensionStart != DateTime.MaxValue)
                {
                    _logger.Debug(LoggerCategories.ShiftExtAudit,
                        $" {_navigator.StartTimestamp.AbsoluteDifference(latestShiftExtensionEnd)} > {_ruleDefinition.MinShiftExtensionMaxUseSize}");

                    if (_navigator.StartTimestamp.AbsoluteDifference(latestShiftExtensionEnd) >
                        _ruleDefinition.MinShiftExtensionMaxUseSize)
                    {

                        _logger.Debug(LoggerCategories.ShiftExtAudit,
                            $"  ADDING VIOLATION 1: {latestShiftExtensionEnd}");
                        _violationGateway.SaveViolation(new Violation(
                            _navigator.DriverIdNumber,
                            _navigator.TruckNumber,
                            latestShiftExtensionEnd,
                            _navigator.StartTimestamp - latestShiftExtensionEnd,
                            latestShiftExtensionEnd,
                            _navigator.StartTimestamp - latestShiftExtensionEnd,
                            _ruleDefinition.MinShiftExtensionMaxUseSize,
                            TimeSpan.Zero,
                            "Shift extension overuse detected. Conflict at: " + _navigator.StartTimestamp));
                        _logger.Debug(LoggerCategories.ShiftExtAudit,
                            $"  ADDING VIOLATION 2: {_navigator.StartTimestamp}");
                        _violationGateway.SaveViolation(new Violation(
                            _navigator.DriverIdNumber,
                            _navigator.TruckNumber,
                            _navigator.StartTimestamp,
                            _navigator.StartTimestamp - latestShiftExtensionEnd,
                            _navigator.StartTimestamp,
                            _navigator.StartTimestamp - latestShiftExtensionEnd,
                            _ruleDefinition.MinShiftExtensionMaxUseSize,
                            TimeSpan.Zero, 
                            "Shift extension overuse detected. Conflict at: " + latestShiftExtensionEnd));
  
                    }
                }
            } while (!_navigator.IsEndOfShiftExtensions());
            _logger.Debug(LoggerCategories.ShiftExtAudit, $"End Of Shift Extensions: {_navigator.StartTimestamp}");

        }
    }
}
