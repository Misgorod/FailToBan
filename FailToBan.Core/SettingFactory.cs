using System;
using System.Text.RegularExpressions;

namespace FailToBan.Core
{
    public class SettingFactory : ISettingFactory
    {
        private readonly Regex sectionRegex;
        private readonly Regex ruleRegex;
        private readonly Regex continuationRegex;

        public SettingFactory(Regex sectionRegex, Regex ruleRegex, Regex continuationRegex)
        {
            this.sectionRegex = sectionRegex;
            this.ruleRegex = ruleRegex;
            this.continuationRegex = continuationRegex;
        }

        public ISetting Build(string configuration)
        {
            if (configuration == null) return null;
            var setting = new Setting();
            // TODO: Create section factory
            Section section = null;
            var rule = RuleType.Null;
            string ruleValue = null;
            foreach (var s in configuration.Split("\n"))
            {
                var sectionName = sectionRegex.IsMatch(s) ? sectionRegex.Match(s).Groups[1].Value : null;
                if (sectionName != null)
                {
                    section = new Section();
                    setting.AddSection(sectionName, section);
                    continue;
                }

                var ruleName = ruleRegex.IsMatch(s) ? ruleRegex.Match(s).Groups[1].Value : null;
                if (ruleName != null && section != null && RuleTypeExtension.TryParse(ruleName, out rule))
                {
                    ruleValue = ruleRegex.Match(s).Groups[2].Value;
                    section.SetRule(rule, ruleValue);
                    continue;
                }

                var continuation = continuationRegex.IsMatch(s) ? continuationRegex.Match(s).Groups[1].Value : null;
                if (continuation != null && ruleValue != null)
                {
                    section.AddToRule(rule, continuation);
                }
            }

            return setting;
        }

        public ISetting Build()
        {
            return new Setting();
        }
    }
}