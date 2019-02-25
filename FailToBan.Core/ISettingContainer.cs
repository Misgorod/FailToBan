namespace FailToBan.Core
{
    public interface ISettingContainer
    {
        bool AddConf(string name, SettingType type, ISetting setting);
        bool AddLocal(string name, SettingType type, ISetting setting);
        (ISetting conf, ISetting local) GetSettings(string name, SettingType type);
    }
}