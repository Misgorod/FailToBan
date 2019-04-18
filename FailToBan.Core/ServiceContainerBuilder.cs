using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
            //Console.WriteLine("JAILS\n");
            //foreach (var (name, jail) in result.Jails)
            //{
            //    Console.WriteLine($"{name} = {jail.Name}");
            //}
            //Console.WriteLine("ACTIONS\n");
            //foreach (var (name, jail) in result.Actions)
            //{
            //    Console.WriteLine($"{name} = {jail.Name}");
            //}
            //Console.WriteLine("FILTERS\n");
            //foreach (var (name, jail) in result.Jails)
            //{
            //    Console.WriteLine($"{name} = {jail.Name}");
            //}
            return result;
        }

        public IServiceContainerBuilder BuildDefault(string path)
        {
            var jailConfPath = fileSystem.Path.Combine(path, "jail.conf");
            var jailLocalPath = fileSystem.Path.Combine(path, "jail.local");
            if (!fileSystem.File.Exists(jailLocalPath)) fileSystem.File.Create(jailLocalPath).Dispose();
            var jailConfText = ReadSettingFromPath(jailConfPath);
            var jailLocalText = ReadSettingFromPath(jailLocalPath);

            // TODO: AddToRule service factory
            var defaultJail = serviceFactory.BuildService("jail", jailConfText, jailLocalText);
            serviceContainer.SetDefault(defaultJail);
            return this;
        }

        private string ReadSettingFromPath(string path)
        {
            var result = "";
            using (var reader = new StreamReader(fileSystem.File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None)))
            {
                result = reader.ReadToEnd();
            }

            return result;
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
                        var localSetting = settingFactory.Build();
                        jail = serviceFactory.BuildJail(fileName, confSetting, localSetting, defaultJail);
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
                        jail = serviceFactory.BuildJail(fileName, localSetting, defaultJail);
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
                    //Console.WriteLine(fileName);
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
                    var filter = serviceContainer.GetFilter(fileName);
                    if (filter != null)
                    {
                        filter.LocalSetting = localSetting;
                    }
                    else
                    {
                        filter = serviceFactory.BuildService(fileName);
                        filter.LocalSetting = localSetting;
                        serviceContainer.SetFilter(filter);
                    }
                }
            }

            return this;
        }
    }
}