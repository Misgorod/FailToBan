using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace FailToBan.Core
{
    public class ServiceContainerBuilder : IServiceContainerBuilder
    {
        private readonly IFileSystem fileSystem;
        private readonly ISettingFactory settingFactory;
        private IService jail;
        private Dictionary<string, IService> jails;
        private Dictionary<string, IService> actions;
        private Dictionary<string, IService> filters;

        public ServiceContainerBuilder(IFileSystem fileSystem, ISettingFactory settingFactory)
        {
            this.fileSystem = fileSystem;
            this.settingFactory = settingFactory;
            this.jails = new Dictionary<string, IService>();
            this.actions = new Dictionary<string, IService>();
            this.filters = new Dictionary<string, IService>();
        }

        public IServiceContainerBuilder BuildDefault(string path)
        {
            var confFiles = fileSystem.Directory.GetFiles(path, "jail.conf");
            if (confFiles.Length != 1)
            {
                throw new VicException($"There is no jail.conf in {path}");
            }

            var jailConf = fileSystem.File.ReadAllText(confFiles[0]);
            var jailConfSetting = settingFactory.Build(jailConf);

            var localFiles = fileSystem.Directory.GetFiles(path, "jail.local");
            if (localFiles.Length != 1)
            {
                throw new VicException($"There is no jail.local in {path}");
            }

            var jailLocal = fileSystem.File.ReadAllText(localFiles[0]);
            var jailLocalSetting = settingFactory.Build(jailLocal);

            // TODO: Add service factory
            jail = new Service(jailConfSetting, jailLocalSetting, "jail");
            return this;
        }

        // TODO: Finish building of jails
        public IServiceContainerBuilder BuildJails(string path)
        {
            var confFiles = fileSystem.Directory.GetFiles(path, "?*.conf");
            return this;
        }
    }
}