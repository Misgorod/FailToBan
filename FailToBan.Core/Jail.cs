namespace FailToBan.Core
{
    public class Jail : Service
    {
        private readonly IService jailSetting;

        public Jail(string name, ISetting localSetting, IService jailSetting) : base(name, localSetting)
        {
            this.jailSetting = jailSetting;
        }

        public override string GetRule(string section, RuleType type)
        {
            var value = base.GetRule(section, type) ?? 
                        jailSetting.GetRule(section, type) ??
                        jailSetting.GetRule("DEFAULT", type);

            return value;
        }

        public override IService Clone()
        {
            return new Jail(Name, LocalSetting.Clone(), jailSetting)
            {
                ConfSetting = ConfSetting?.Clone(),
            };
        }
    }
}