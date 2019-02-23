using System.Collections.Generic;

namespace FailToBan.Core
{
    public class SettingContainer
    {
        private readonly Dictionary<(string, SettingType), ISetting> settings = new Dictionary<(string, SettingType), ISetting>();

        public bool AddSetting(string name, SettingType type, ISetting setting)
        {
            return settings.TryAdd((name, type), setting);
        }

        public ISetting GetSetting(string name, SettingType type)
        {
            return settings.TryGetValue((name, type), out var setting) ? setting : null;
        }
    }
}