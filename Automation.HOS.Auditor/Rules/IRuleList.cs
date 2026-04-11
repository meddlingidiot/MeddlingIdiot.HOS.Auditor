
using Automation.HOS.Violations;

namespace Automation.HOS.Rules;

internal interface IRuleList: IRule
{
    void AddRule(RuleBase rule);
    List<Violation> GetViolations(ThrowViolations firedAt);
}