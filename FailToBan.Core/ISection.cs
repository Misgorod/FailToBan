namespace FailToBan.Core
{
    public interface ISection
    {
        ISection SetRule(RuleType rule, string value);

        ISection AddToRule(RuleType rule, string value);

        ISection Clone();

        string GetRule(RuleType rule);

        string ToString(string name);
    }
}