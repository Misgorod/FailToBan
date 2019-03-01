using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace FailToBan.Core
{
    public class ServiceContainerBuilder : IServiceContainerBuilder
    {
        private readonly IFileSystem fileSystem;
        private readonly IServiceFactory serviceFactory;
        private IService defaultJail;
        private readonly Dictionary<string, IService> jails;
        private readonly Dictionary<string, IService> actions;
        private readonly Dictionary<string, IService> filters;

        public ServiceContainerBuilder(IFileSystem fileSystem, IServiceFactory serviceFactory)
        {
            this.fileSystem = fileSystem;
            this.serviceFactory = serviceFactory;
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
            var jailConfText = ReadSettingFromPath(jailConfPath);
            var jailLocalText = ReadSettingFromPath(jailLocalPath);

            // TODO: AddToRule service factory
            defaultJail = serviceFactory.BuildService("jail", jailConfText, jailLocalText);
            return this;
        }

        private string ReadSettingFromPath(string path)
        {
            if (!fileSystem.File.Exists(path))
            {
                throw new VicException($"Setting at {path} doesn't exists");
            }

            var settingText = fileSystem.File.ReadAllText(path);
            return settingText;
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
                var confText = ReadSettingFromPath(confFile);
                if (builtJails.ContainsKey(fileName))
                {
                    var existingService = builtJails[fileName];
                    var newService = serviceFactory.BuildJail(fileName, existingService.ConfSetting, existingService.LocalSetting, defaultJail);
                    builtJails[fileName] = newService;
                }
                else
                {
                    builtJails.Add(fileName, serviceFactory.BuildJail(fileName, confText, null, defaultJail));

                }
            }

            var localFiles = fileSystem.Directory.GetFiles(path, "?*.local");
            foreach (var localFile in localFiles)
            {
                var fileName = fileSystem.Path.GetFileNameWithoutExtension(localFile);
                var localText = ReadSettingFromPath(localFile);
                if (builtJails.ContainsKey(fileName))
                {
                    var existingService = builtJails[fileName];
                    var newService = serviceFactory.BuildJail(fileName, existingService.ConfSetting, existingService.LocalSetting, defaultJail);
                    builtJails[fileName] = newService;
                }
                else
                {
                    builtJails.Add(fileName, serviceFactory.BuildJail(fileName, null, localText, defaultJail));

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
                var confText = ReadSettingFromPath(confFile);
                if (collection.ContainsKey(fileName))
                {
                    var existingService = collection[fileName];
                    var newService = serviceFactory.BuildService(fileName, existingService.ConfSetting, existingService.LocalSetting);
                    collection[fileName] = newService;
                }
                else
                {
                    collection.Add(fileName, serviceFactory.BuildService(fileName, confText, null));
                }
            }

            var localFiles = fileSystem.Directory.GetFiles(path, "?*.local");
            foreach (var localFile in localFiles)
            {
                var fileName = fileSystem.Path.GetFileNameWithoutExtension(localFile);
                var localText = ReadSettingFromPath(localFile);
                if (filters.ContainsKey(fileName))
                {
                    var existingService = collection[fileName];
                    var newService = serviceFactory.BuildService(fileName, existingService.ConfSetting, existingService.LocalSetting);
                    collection[fileName] = newService;
                }
                else
                {
                    collection.Add(fileName, serviceFactory.BuildService(fileName, null, localText));
                }
            }

            return collection;
        }
    }
}