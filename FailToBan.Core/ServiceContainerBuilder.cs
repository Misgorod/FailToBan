using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;

namespace FailToBan.Core
{
    public class ServiceContainerBuilder : IServiceContainerBuilder
    {
        // TODO: Add building of filters
        private readonly IFileSystem fileSystem;
        private readonly ISettingFactory settingFactory;
        private IService defaultJail;
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
            var confFiles = fileSystem.Directory.GetFiles(path, "defaultJail.conf");
            if (confFiles.Length != 1)
            {
                throw new VicException($"There is no defaultJail.conf in {path}");
            }

            var jailConf = fileSystem.File.ReadAllText(confFiles[0]);
            var jailConfSetting = settingFactory.Build(jailConf);

            var localFiles = fileSystem.Directory.GetFiles(path, "defaultJail.local");
            if (localFiles.Length != 1)
            {
                throw new VicException($"There is no defaultJail.local in {path}");
            }

            var jailLocal = fileSystem.File.ReadAllText(localFiles[0]);
            var jailLocalSetting = settingFactory.Build(jailLocal);

            // TODO: Add service factory
            defaultJail = new Service(jailConfSetting, jailLocalSetting, "defaultJail");
            return this;
        }

        public IServiceContainerBuilder BuildJails(string path)
        {
            if (defaultJail == null)
            {
                throw new VicException("Building jails before creating defaultJail.conf");
            }
            var confFiles = fileSystem.Directory.GetFiles(path, "?*.conf");
            var localFiles = fileSystem.Directory.GetFiles(path, "?*.local");
            foreach (var confFile in confFiles)
            {
                if (!Regex.IsMatch(fileSystem.Path.GetFileName(confFile), @"^([\w\-_]+).conf$")) continue;
                var fileName = Regex.Match(confFile, @"^([\w\-_]+).conf$").Groups[1].Value;
                var confText = fileSystem.File.ReadAllText(confFile);
                var confSetting = settingFactory.Build(confText);
                var localName = localFiles.FirstOrDefault(x => fileSystem.Path.GetFileName(x) == $"{fileName}.local");
                if (localName != null)
                {
                    var localText = fileSystem.File.ReadAllText(localName);
                    var localSetting = settingFactory.Build(localText);
                    var jail = new Jail(confSetting, localSetting, fileName, defaultJail);
                    jails.Add(fileName, jail);
                }
                else
                {
                    var localSetting = settingFactory.Build();
                    var jail = new Jail(confSetting, localSetting, fileName, defaultJail);
                    jails.Add(fileName, jail);
                }
            }

            foreach (var localFile in localFiles)
            {
                if (!Regex.IsMatch(fileSystem.Path.GetFileName(localFile), @"^([\w\-_]+).local$")) continue;
                var fileName = Regex.Match(localFile, @"^([\w\-_]+).conf$").Groups[1].Value;
                if (!jails.ContainsKey(fileName))
                {
                    var localText = fileSystem.File.ReadAllText(localFile);
                    var localSetting = settingFactory.Build(localText);
                    var confSetting = settingFactory.Build();
                    var jail = new Jail(confSetting, localSetting, fileName, defaultJail);
                    jails.Add(fileName, jail);
                }
            }
            return this;
        }

        public IServiceContainerBuilder BuildActions(string path)
        {
            var confFiles = fileSystem.Directory.GetFiles(path, "?*.conf");
            var localFiles = fileSystem.Directory.GetFiles(path, "?*.local");
            foreach (var confFile in confFiles)
            {
                if (!Regex.IsMatch(fileSystem.Path.GetFileName(confFile), @"^([\w\-_]+).conf$")) continue;
                var fileName = Regex.Match(confFile, @"^([\w\-_]+).conf$").Groups[1].Value;
                var confSetting = settingFactory.Build(confFile);
                var localName = localFiles.FirstOrDefault(x => fileSystem.Path.GetFileName(x) == $"{fileName}.local");
                if (localName != null)
                {
                    var localSetting = settingFactory.Build(localName);
                    var action = new Service(confSetting, localSetting, fileName);
                    actions.Add(fileName, action);
                }
                else
                {
                    var localSetting = settingFactory.Build();
                    var action = new Service(confSetting, localSetting, fileName);
                    actions.Add(fileName, action);
                }
            }

            foreach (var localFile in localFiles)
            {
                if (!Regex.IsMatch(fileSystem.Path.GetFileName(localFile), @"^([\w\-_]+).local$")) continue;
                var fileName = Regex.Match(localFile, @"^([\w\-_]+).conf$").Groups[1].Value;
                if (!actions.ContainsKey(fileName))
                {
                    var localText = fileSystem.File.ReadAllText(localFile);
                    var localSetting = settingFactory.Build(localText);
                    var confSetting = settingFactory.Build();
                    var action = new Service(confSetting, localSetting, fileName);
                    actions.Add(fileName, action);
                }
            }
            return this;
        }
    }
}