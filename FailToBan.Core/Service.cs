namespace FailToBan.Core
{
    // TODO: AddToRule check of after and before properties
    public class Service : IService
    {
        public string Name { get; }

        public ISetting ConfSetting { get; set; }
        public ISetting LocalSetting { get; set; }

        public Service(ISetting confSetting, ISetting localSetting, string name)
        {
            this.ConfSetting = confSetting;
            this.LocalSetting = localSetting;
            Name = name;
        }

        public Service(string name) : this(null, null, name)
        { }

        public virtual string GetRule(string section, RuleType type)
        {
            var value = LocalSetting.GetSection(section)?.GetRule(type) ??
                        ConfSetting.GetSection(section)?.GetRule(type);
            return value;
        }

        public virtual void SetRule(string sectionName, RuleType type, string value)
        {
            var section = LocalSetting.GetSection(sectionName) ?? new Section();
            section.SetRule(type, value);
        }

        public virtual IService Clone()
        {
            return new Service(ConfSetting.Clone(), LocalSetting.Clone(), Name);
        }
    }
}