using System.Collections.Generic;

namespace FailToBan.Core
{
    public class SettingContainer : ISettingContainer
    {
        private readonly Dictionary<(string name, SettingType type), ISetting> confSettings = new Dictionary<(string name, SettingType type), ISetting>();
        private readonly Dictionary<(string name, SettingType type), ISetting> localSettings = new Dictionary<(string name, SettingType type), ISetting>();

        public bool AddConf(string name, SettingType type, ISetting setting)
        {
            return confSettings.TryAdd((name, type), setting);
        }

        public bool AddLocal(string name, SettingType type, ISetting setting)
        {
            return localSettings.TryAdd((name, type), setting);
        }

        public (ISetting conf, ISetting local) GetSettings(string name, SettingType type)
        {
            var conf = confSettings.TryGetValue((name, type), out var confSetting) ? confSetting : null;
            var local = localSettings.TryGetValue((name, type), out var localSetting) ? localSetting : null;
            return (conf, local);
        }
    }
}