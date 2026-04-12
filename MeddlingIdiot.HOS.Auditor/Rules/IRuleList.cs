
using MeddlingIdiot.HOS.Violations;

namespace MeddlingIdiot.HOS.Rules;

internal interface IRuleList: IRule
{
    void AddRule(RuleBase rule);
    List<Violation> GetViolations(ThrowViolations firedAt);
}