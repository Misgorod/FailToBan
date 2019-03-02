namespace FailToBan.Core
{
    public class ServiceFactory : IServiceFactory
    {
        private readonly ISettingFactory settingFactory;

        public ServiceFactory(ISettingFactory settingFactory)
        {
            this.settingFactory = settingFactory;
        }

        public IService BuildService(string name)
        {
            var conf = settingFactory.Build();
            var local = settingFactory.Build();
            return new Service(conf, local, name);
        }

        public IService BuildService(string name, string confText, string localText)
        {
            var conf = settingFactory.Build(confText);
            var local = settingFactory.Build(localText);
            return new Service(conf, local, name);
        }

        public IService BuildService(string name, ISetting confSetting, ISetting localSetting)
        {
            return new Service(confSetting, localSetting, name);
        }

        public IService BuildJail(string name, string confText, string localText, IService defaultService)
        {
            var conf = settingFactory.Build(confText);
            var local = settingFactory.Build(localText);
            return new Jail(conf, local, name, defaultService);
        }

        public IService BuildJail(string name, ISetting confSetting, ISetting localSetting, IService defaultService)
        {
            return new Jail(confSetting, localSetting, name, defaultService);
        }

        public IService BuildJail(string name, IService defaultService)
        {
            return new Jail(name, defaultService);
        }
    }
}