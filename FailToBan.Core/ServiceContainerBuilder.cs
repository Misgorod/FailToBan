using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace FailToBan.Core
{
    public class ServiceContainerBuilder : IServiceContainerBuilder
    {
        private readonly IFileSystem fileSystem;
        private readonly IServiceFactory serviceFactory;
        private readonly ISettingFactory settingFactory;
        private IServiceContainer serviceContainer;

        public ServiceContainerBuilder(IFileSystem fileSystem, IServiceFactory serviceFactory, ISettingFactory settingFactory)
        {
            this.fileSystem = fileSystem;
            this.serviceFactory = serviceFactory;
            this.settingFactory = settingFactory;
            this.serviceContainer = new ServiceContainer();
        }

        public ServiceContainerBuilder(IServiceFactory serviceFactory, ISettingFactory settingFactory) : this(new FileSystem(), serviceFactory, settingFactory)
        { }

        public IServiceContainer Build()
        {
            var result = serviceContainer;
            serviceContainer = new ServiceContainer();
            return result;
        }

        public IServiceContainerBuilder BuildDefault(string path)
        {
            var jailConfPath = fileSystem.Path.Combine(path, "jail.conf");
            var jailLocalPath = fileSystem.Path.Combine(path, "jail.local");
            var jailConfText = ReadSettingFromPath(jailConfPath);
            var jailLocalText = ReadSettingFromPath(jailLocalPath);

            // TODO: AddToRule service factory
            var defaultJail = serviceFactory.BuildService("jail", jailConfText, jailLocalText);
            serviceContainer.SetDefault(defaultJail);
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
            var defaultJail = serviceContainer.GetDefault();
            if (defaultJail == null)
            {
                throw new VicException("Building jails before creating jail.conf");
            }

            var files = fileSystem.Directory.GetFiles(path);
            foreach (var file in files)
            {
                var fileName = fileSystem.Path.GetFileNameWithoutExtension(file);
                var fileNameWithExt = fileSystem.Path.GetFileName(file);
                if (Regex.IsMatch(fileNameWithExt, @"[\w\d\-_]+.conf$"))
                {
                    var confText = ReadSettingFromPath(file);
                    var confSetting = settingFactory.Build(confText);
                    var jail = serviceContainer.GetJail(fileName);
                    if (jail != null)
                    {
                        jail.ConfSetting = confSetting;
                    }
                    else
                    {
                        jail = serviceFactory.BuildJail(fileName, defaultJail);
                        jail.ConfSetting = confSetting;
                        serviceContainer.SetJail(jail);
                    }
                }
                else if (Regex.IsMatch(fileNameWithExt, @"[\w\d\-_]+.local$"))
                {
                    var localText = ReadSettingFromPath(file);
                    var localSetting = settingFactory.Build(localText);
                    var jail = serviceContainer.GetJail(fileName);
                    if (jail != null)
                    {
                        jail.LocalSetting = localSetting;
                    }
                    else
                    {
                        jail = serviceFactory.BuildJail(fileName, defaultJail);
                        jail.LocalSetting = localSetting;
                        serviceContainer.SetJail(jail);
                    }
                }
            }

            return this;
        }

        public IServiceContainerBuilder BuildActions(string path)
        {
            var files = fileSystem.Directory.GetFiles(path);
            foreach (var file in files)
            {
                var fileName = fileSystem.Path.GetFileNameWithoutExtension(file);
                var fileNameWithExt = fileSystem.Path.GetFileName(file);
                if (Regex.IsMatch(fileNameWithExt, @"[\w\d\-_]+.conf$"))
                {
                    var confText = ReadSettingFromPath(file);
                    var confSetting = settingFactory.Build(confText);
                    var action = serviceContainer.GetAction(fileName);
                    if (action != null)
                    {
                        action.ConfSetting = confSetting;
                    }
                    else
                    {
                        action = serviceFactory.BuildService(fileName);
                        action.ConfSetting = confSetting;
                        serviceContainer.SetAction(action);
                    }
                }
                else if (Regex.IsMatch(fileNameWithExt, @"[\w\d\-_]+.local$"))
                {
                    var localText = ReadSettingFromPath(file);
                    var localSetting = settingFactory.Build(localText);
                    var action = serviceContainer.GetAction(fileName);
                    if (action != null)
                    {
                        action.LocalSetting = localSetting;
                    }
                    else
                    {
                        action = serviceFactory.BuildService(fileName);
                        action.LocalSetting = localSetting;
                        serviceContainer.SetAction(action);
                    }
                }
            }

            return this;
        }

        public IServiceContainerBuilder BuildFilters(string path)
        {
            var files = fileSystem.Directory.GetFiles(path);
            foreach (var file in files)
            {
                var fileName = fileSystem.Path.GetFileNameWithoutExtension(file);
                var fileNameWithExt = fileSystem.Path.GetFileName(file);
                if (Regex.IsMatch(fileNameWithExt, @"[\w\d\-_]+.conf$"))
                {
                    var confText = ReadSettingFromPath(file);
                    var confSetting = settingFactory.Build(confText);
                    var filter = serviceContainer.GetFilter(fileName);
                    if (filter != null)
                    {
                        filter.ConfSetting = confSetting;
                    }
                    else
                    {
                        filter = serviceFactory.BuildService(fileName);
                        filter.ConfSetting = confSetting;
                        serviceContainer.SetFilter(filter);
                    }
                }
                else if (Regex.IsMatch(fileNameWithExt, @"[\w\d\-_]+.local$"))
                {
                    var localText = ReadSettingFromPath(file);
                    var localSetting = settingFactory.Build(localText);
                    var action = serviceContainer.GetFilter(fileName);
                    if (action != null)
                    {
                        action.LocalSetting = localSetting;
                    }
                    else
                    {
                        action = serviceFactory.BuildService(fileName);
                        action.LocalSetting = localSetting;
                        serviceContainer.SetFilter(action);
                    }
                }
            }

            return this;
        }
    }
}