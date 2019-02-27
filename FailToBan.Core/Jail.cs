namespace FailToBan.Core
{
    public class Jail : Service
    {
        private readonly IService jailSetting;

        public Jail(ISetting confSetting, ISetting localSetting, string name, IService jailSetting) : base(confSetting, localSetting, name)
        {
            this.jailSetting = jailSetting;
        }

        public Jail(string name, IService jailSetting) : this(null, null, name, jailSetting)
        { }

        public override string GetRule(string section, RuleType type)
        {
            var value = LocalSetting?.GetSection(section)?.GetRule(type) ?? 
                        ConfSetting?.GetSection(section)?.GetRule(type) ?? 
                        jailSetting?.GetRule(section, type) ??
                        jailSetting?.GetRule("DEFAULT", type);

            return value;
        }

        public override IService Clone()
        {
            return new Jail(ConfSetting.Clone(), LocalSetting.Clone(), Name, jailSetting.Clone());
        }
    }
}