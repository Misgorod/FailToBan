namespace FailToBan.Core
{
    public interface ISection
    {
        ISection Set(RuleType rule, string value);

        ISection Add(RuleType rule, string value);

        ISection Clone();

        string Get(RuleType rule);

        string ToString(string name);
    }
}