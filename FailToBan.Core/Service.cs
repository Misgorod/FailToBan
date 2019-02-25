namespace FailToBan.Core
{
    public class Service : IService
    {
        public string Name { get; }

        protected readonly ISetting confSetting;
        protected readonly ISetting localSetting;

        public Service(ISetting confSetting, ISetting localSetting, string name)
        {
            this.confSetting = confSetting;
            this.localSetting = localSetting;
            Name = name;
        }

        public virtual string GetRule(string section, RuleType type)
        {
            var value = localSetting.GetSection(section)?.Get(type) ??
                        confSetting.GetSection(section)?.Get(type);
            return value;
        }

        public virtual void SetRule(string sectionName, RuleType type, string value)
        {
            var section = localSetting.GetSection(sectionName) ?? new Section();
            section.Set(type, value);
        }

        public (ISetting conf, ISetting local) GetSettings()
        {
            return (confSetting, localSetting);
        }
    }
}