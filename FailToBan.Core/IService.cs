namespace FailToBan.Core
{
    public interface IService
    {
        string Name { get; }
        ISetting ConfSetting { get; set; }
        ISetting LocalSetting { get; set; }
        string GetRule(string section, RuleType type);
        void SetRule(string sectionName, RuleType type, string value);
        IService Clone();
    }
}