using System.Collections.Generic;

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
            return BuildService(name, conf, local);
        }

        public IService BuildService(string name, string confText, string localText)
        {
            var conf = settingFactory.Build(confText);
            var local = settingFactory.Build(localText);
            return BuildService(name, conf, local);
        }

        public IService BuildService(string name, ISetting localSetting)
        {
            var conf = settingFactory.Build();
            return BuildService(name, conf, localSetting);
        }

        public IService BuildService(string name, ISetting confSetting, ISetting localSetting)
        {
            return new Service(name, localSetting)
            {
                ConfSetting = confSetting,
            };
        }

        public IService BuildJail(string name, string confText, string localText, IService defaultService)
        {
            var conf = settingFactory.Build(confText);
            var local = settingFactory.Build(localText);
            return BuildJail(name, conf, local, defaultService);
        }


        public IService BuildJail(string name, IService defaultService)
        {
            var conf = settingFactory.Build();
            var local = settingFactory.Build();
            return BuildJail(name, conf, local, defaultService);
        }

        public IService BuildJail(string name, ISetting localSetting, IService defaultService)
        {
            var conf = settingFactory.Build();
            return BuildJail(name, conf, localSetting, defaultService);
        }

        public IService BuildJail(string name, ISetting confSetting, ISetting localSetting, IService defaultService)
        {
            return new Jail(name, localSetting, defaultService)
            {
                ConfSetting = confSetting,
            };
        }
    }
}