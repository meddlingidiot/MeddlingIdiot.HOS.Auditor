namespace MeddlingIdiot.HOS.Ruleset
{
    /// <summary>
    /// Maps a JurisdictionMoment.JurisdictionName to the rule definition that governs it.
    /// Names match the rule definition class names without the "RuleDefinition" suffix
    /// (case-insensitive). Unknown or null names return null so the caller falls back
    /// to the default jurisdiction.
    /// </summary>
    public static class JurisdictionRuleDefinitionFactory
    {
        private static readonly Dictionary<string, Func<IRuleDefinition>> Registry =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["Us60Hr"] = () => new Us60HrRuleDefinition(),
                ["Us70Hr"] = () => new Us70HrRuleDefinition(),
                ["UsBus60Hr"] = () => new UsBus60HrRuleDefinition(),
                ["UsBus70Hr"] = () => new UsBus70HrRuleDefinition(),
                ["UsOilfield60Hr"] = () => new UsOilfield60HrRuleDefinition(),
                ["UsOilfield70Hr"] = () => new UsOilfield70HrRuleDefinition(),
                ["UsSleeperPilot60Hr"] = () => new UsSleeperPilot60HrRuleDefinition(),
                ["UsSleeperPilot70Hr"] = () => new UsSleeperPilot70HrRuleDefinition(),
                ["CaliforniaIntrastate80Hr"] = () => new CaliforniaIntrastate80HrRuleDefinition(),
                ["FloridaIntrastate70Hr"] = () => new FloridaIntrastate70HrRuleDefinition(),
                ["FloridaIntrastate80Hr"] = () => new FloridaIntrastate80HrRuleDefinition(),
                ["TexasIntrastate70Hr"] = () => new TexasIntrastate70HrRuleDefinition(),
                ["CanadaCycle1"] = () => new CanadaCycle1RuleDefinition(),
                ["CanadaCycle2"] = () => new CanadaCycle2RuleDefinition(),
                ["CanadaAutoCycle"] = () => new CanadaAutoCycleRuleDefinition(),
                ["CanadaTeamCycle1"] = () => new CanadaTeamCycle1RuleDefinition(),
                ["CanadaTeamCycle2"] = () => new CanadaTeamCycle2RuleDefinition(),
                ["CanadaTeamAutoCycle"] = () => new CanadaTeamAutoCycleRuleDefinition(),
            };

        public static IRuleDefinition? Create(string? jurisdictionName)
        {
            if (string.IsNullOrWhiteSpace(jurisdictionName))
                return null;

            var key = jurisdictionName.Trim();
            if (key.EndsWith("RuleDefinition", StringComparison.OrdinalIgnoreCase))
                key = key.Substring(0, key.Length - "RuleDefinition".Length);

            return Registry.TryGetValue(key, out var factory) ? factory() : null;
        }

        public static bool IsKnown(string? jurisdictionName)
        {
            return Create(jurisdictionName) != null;
        }
    }
}
