using System.Collections.Generic;

namespace FailToBan.Core
{
    public interface ISection
    {
        Dictionary<RuleType, string> Rules { get; }
        ISection SetRule(RuleType rule, string value);
        bool AddToRule(RuleType rule, string value);
        ISection Clone();
        string GetRule(RuleType rule);
        string ToString(string name);
        string GetUnknown(string rule);
        bool AddToUnknow(string rule, string value);
    }
}