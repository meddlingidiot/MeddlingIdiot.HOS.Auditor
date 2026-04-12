using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.Violations;

namespace MeddlingIdiot.HOS.Rules
{
    internal class RuleList : IRuleList
    {
        private readonly List<RuleBase> _rules;

        public RuleList()
        {
            _rules = new List<RuleBase>();
        }

        public void AddRule(RuleBase rule)
        {
            _rules.Add(rule);
        }

        public void Accumulate(DateTime startTimestamp, TimeSpan toAccumulate, DutyStatus dutyStatus)
        {
            foreach (var rule in _rules)
            {
                rule.Accumulate(startTimestamp, toAccumulate, dutyStatus);
            }
        }

        public void GlobalReset()
        {
            foreach (var rule in _rules)
            {
                rule.GlobalReset();
            }
        }

        public void Reset()
        {
            foreach (var rule in _rules)
            {
                rule.Reset();
            }
        }

        public List<Violation> GetViolations(ThrowViolations firedAt)
        {
            var violations = new List<Violation>();
            foreach (var rule in _rules)
            {
                if (rule.ThrowViolations.Contains(firedAt))
                {
                    var violation = rule.GetViolation();
                    if (violation != null)
                    {
                        violations.Add(violation);
                    }
                }
            }
            return violations;
        }

    } 
}
