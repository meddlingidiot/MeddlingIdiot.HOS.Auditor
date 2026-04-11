using Automation.HOS.TimelineNavigator.Utilities;

namespace Automation.HOS.Violations
{
    internal class ViolationGateway : IViolationGateway
    {
        private readonly ILogger _logger;
        private readonly List<Violation> _violations;

        public ViolationGateway(ILogger logger)
        {
            _logger = logger;
            _violations = new List<Violation>();
        }
        public void SaveViolation(Violation violation)
        {
            var found = false;
            foreach (var v in _violations)
            {
                if ((v.StartTimestamp == violation.StartTimestamp) && (v.Limit == violation.Limit))
                {
                    found = true;
                }
            }

            if (!found)
            {
                _logger.Debug(LoggerCategories.Violations, "Violation added: " + violation.ToString());
                _violations.Add(violation);
            }
            
        }

        public List<Violation> GetViolations()
        {
            return _violations;
        }

        public void Clear()
        {
            _violations.Clear();
        }
    }
}
