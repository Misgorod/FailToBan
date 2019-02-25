namespace FailToBan.Core
{
    public interface IService
    {
        string Name { get; }
        string GetRule(string section, RuleType type);
        void SetRule(string sectionName, RuleType type, string value);
        (ISetting conf, ISetting local) GetSettings();
    }
}