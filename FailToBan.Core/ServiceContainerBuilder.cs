using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace FailToBan.Core
{
    public class ServiceContainerBuilder : IServiceContainerBuilder
    {
        private readonly IFileSystem fileSystem;
        private readonly ISettingFactory settingFactory;
        private IService defaultJail;
        private readonly Dictionary<string, IService> jails;
        private readonly Dictionary<string, IService> actions;
        private readonly Dictionary<string, IService> filters;

        public ServiceContainerBuilder(IFileSystem fileSystem, ISettingFactory settingFactory)
        {
            this.fileSystem = fileSystem;
            this.settingFactory = settingFactory;
            jails = new Dictionary<string, IService>();
            actions = new Dictionary<string, IService>();
            filters = new Dictionary<string, IService>();
        }

        public IServiceContainer Build()
        {
            return new ServiceContainer(defaultJail, jails, actions, filters);
        }

        public IServiceContainerBuilder BuildDefault(string path)
        {
            var jailConfPath = fileSystem.Path.Combine(path, "jail.conf");
            var jailLocalPath = fileSystem.Path.Combine(path, "jail.local");
            var jailConfSetting = BuildSettingFromPath(jailConfPath);
            var jailLocalSetting = BuildSettingFromPath(jailLocalPath);

            // TODO: AddToRule service factory
            defaultJail = new Service(jailConfSetting, jailLocalSetting, "jail");
            return this;
        }

        private ISetting BuildSettingFromPath(string path)
        {
            if (!fileSystem.File.Exists(path))
            {
                throw new VicException($"Setting at {path} doesn't exists");
            }

            var settingText = fileSystem.File.ReadAllText(path);
            var setting = settingFactory.Build(settingText);
            return setting;
        }

        public IServiceContainerBuilder BuildJails(string path)
        {
            if (defaultJail == null)
            {
                throw new VicException("Building jails before creating jail.conf");
            }

            var builtJails = new Dictionary<string, IService>();
            var confFiles = fileSystem.Directory.GetFiles(path, "?*.conf");
            foreach (var confFile in confFiles)
            {
                var fileName = fileSystem.Path.GetFileNameWithoutExtension(confFile);
                var confSetting = BuildSettingFromPath(confFile);
                if (builtJails.ContainsKey(fileName))
                {
                    builtJails[fileName].ConfSetting = confSetting;
                }
                else
                {
                    builtJails.Add(fileName, new Jail(fileName, defaultJail)
                    {
                        ConfSetting = confSetting
                    });
                }
            }

            var localFiles = fileSystem.Directory.GetFiles(path, "?*.local");
            foreach (var localFile in localFiles)
            {
                var fileName = fileSystem.Path.GetFileNameWithoutExtension(localFile);
                var localSetting = BuildSettingFromPath(localFile);
                if (builtJails.ContainsKey(fileName))
                {
                    builtJails[fileName].LocalSetting = localSetting;
                }
                else
                {
                    builtJails.Add(fileName, new Jail(fileName, defaultJail)
                    {
                        LocalSetting = localSetting
                    });
                }
            }

            builtJails.ToList().ForEach(jail => jails[jail.Key] = jail.Value);

            return this;
        }

        public IServiceContainerBuilder BuildActions(string path)
        {
            var builtActions = BuildCollection(path);
            builtActions.ToList().ForEach(action => actions[action.Key] = action.Value);

            return this;
        }

        public IServiceContainerBuilder BuildFilters(string path)
        {
            var builtFilters = BuildCollection(path);
            builtFilters.ToList().ForEach(filter => filters[filter.Key] = filter.Value);

            return this;
        }

        private Dictionary<string, IService> BuildCollection(string path)
        {
            var collection = new Dictionary<string, IService>();
            var confFiles = fileSystem.Directory.GetFiles(path, "?*.conf");
            foreach (var confFile in confFiles)
            {
                var fileName = fileSystem.Path.GetFileNameWithoutExtension(confFile);
                var confSetting = BuildSettingFromPath(confFile);
                if (collection.ContainsKey(fileName))
                {
                    collection[fileName].ConfSetting = confSetting;
                }
                else
                {
                    collection.Add(fileName, new Service(confSetting, settingFactory.Build(), fileName));
                }
            }

            var localFiles = fileSystem.Directory.GetFiles(path, "?*.local");
            foreach (var localFile in localFiles)
            {
                var fileName = fileSystem.Path.GetFileNameWithoutExtension(localFile);
                var localSetting = BuildSettingFromPath(localFile);
                if (filters.ContainsKey(fileName))
                {
                    collection[fileName].LocalSetting = localSetting;
                }
                else
                {
                    collection.Add(fileName, new Service(localSetting, fileName));
                }
            }

            return collection;
        }
    }
}