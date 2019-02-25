using System.Text.RegularExpressions;

namespace FailToBan.Core
{
    public class Filter : Service
    {
        //private readonly ISettingContainer settingContainer;
        //private ISetting nextService;
        //private ISetting prevService;

        public Filter(ISetting confSetting, ISetting localSetting, string name/*, ISettingContainer settingContainer*/) : base(confSetting, localSetting, name)
        {
            //this.settingContainer = settingContainer;
        }

        //public override string GetRule(string section, RuleType type)
        //{
        //    var value = nextService?.GetSection(section)?.Get(type) ?? base.GetRule(section, type) ?? prevService?.GetSection(section)?.Get(type);
        //    return value;
        //}

        //public override void SetRule(string sectionName, RuleType type, string value)
        //{
        //    switch (type)
        //    {
        //        case RuleType.Before when Regex.IsMatch(value, @"^([\w\-]+)\.(?:(?:conf)|(?:local))$"):
        //        {
        //            var settingName = Regex.Match(value, @"^([\w\-]+)\.((?:conf)|(?:local))$").Groups[1].Value;
        //            var settingExtension = Regex.Match(value, @"^([\w\-]+)\.((?:conf)|(?:local))$").Groups[2].Value;
        //            var (conf, local) = settingContainer.GetSettings(settingName, SettingType.Filter);
        //            prevService = settingExtension == "local" ? local : conf;
        //            break;
        //        }
        //        case RuleType.After when Regex.IsMatch(value, @"^([\w\-]+)\.(?:(?:conf)|(?:local))$"):
        //        {
        //            var settingName = Regex.Match(value, @"^([\w\-]+)\.((?:conf)|(?:local))$").Groups[1].Value;
        //            var settingExtension = Regex.Match(value, @"^([\w\-]+)\.((?:conf)|(?:local))$").Groups[2].Value;
        //            var (conf, local) = settingContainer.GetSettings(settingName, SettingType.Filter);
        //            nextService = settingExtension == "local" ? local : conf;
        //            break;
        //        }
        //        default:
        //            base.SetRule(sectionName, type, value);
        //            break;
        //    }
        //}
    }
}