namespace FailToBan.Core
{
    public interface ISection
    {
        ISection Set(RuleType rule, string value);

        string Get(RuleType rule);

        string ToString(string name);
    }
}