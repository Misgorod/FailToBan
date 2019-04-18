namespace FailToBan.Core
{
    public interface IServiceFactory
    {
        IService BuildService(string name);
        IService BuildService(string name, string confText, string localText);
        IService BuildService(string name, ISetting localSetting);
        IService BuildService(string name, ISetting confSetting, ISetting localSetting);
        IService BuildJail(string name, string confText, string localText, IService defaultService);
        IService BuildJail(string name, IService defaultService);
        IService BuildJail(string name, ISetting localSetting, IService defaultService);
        IService BuildJail(string name, ISetting confSetting, ISetting localSetting, IService defaultService);
    }
}