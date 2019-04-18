using System.Collections.Generic;
using System.Text;
using FailToBan.Core;

namespace FailToBan.Server.Shells
{
    public abstract class ShellState
    {
        protected Shell shell;
        protected IServiceContainer serviceContainer;
        protected IService currentJail;
        protected IService currentFilter;
        protected IServiceFactory serviceFactory;
        protected ISettingFactory settingFactory;
        protected IServiceSaver jailSaver;
        protected IServiceSaver filterSaver;

        public string Message { get; protected set; }

        public ShellState(Shell shell, IServiceContainer serviceContainer, IServiceFactory serviceFactory, ISettingFactory settingFactory, IServiceSaver jailSaver, IServiceSaver filterSaver)
        {
            this.shell = shell;
            this.serviceContainer = serviceContainer;
            this.serviceFactory = serviceFactory;
            this.settingFactory = settingFactory;
            this.jailSaver = jailSaver;
            this.filterSaver = filterSaver;
        }

        public ShellState(ShellState state)
        {
            this.shell = state.shell;
            this.serviceContainer = state.serviceContainer;
            this.currentJail = state.currentJail;
            this.currentFilter = state.currentFilter;
            this.serviceFactory = state.serviceFactory;
            this.settingFactory = state.settingFactory;
            this.jailSaver = state.jailSaver;
            this.filterSaver = state.filterSaver;
        }


        public abstract string Handle(string[] values);

        protected string PrintDefaultRules()
        {
            var confRules = serviceContainer
                .GetDefault()
                .ConfSetting
                .GetSection("DEFAULT")
                .Rules;
            var localRules = serviceContainer
                .GetDefault()
                .LocalSetting
                .GetSection("DEFAULT")
                .Rules;

            foreach (var (type, value) in confRules)
            {
                if (!localRules.ContainsKey(type))
                {
                    localRules.Add(type, value);
                }
            }

            var builder = new StringBuilder();
            builder.AppendLine("Список правил, установленных по умолчанию:");
            foreach (var (key, value) in localRules)
            {
                builder.AppendLine($"{key} : {value}");
            }
            return builder.ToString();
        }

        protected string PrintCurrentRules()
        {
            var rules = new Dictionary<string, string>();
            var confSections = currentJail.ConfSetting?.Sections;
            if (confSections != null)
            {
                foreach (var (name, section) in confSections)
                {
                    foreach (var (rule, value) in section.Rules)
                    {
                        rules[rule.ToString()] = value;
                    }
                }
            }

            var localSections = currentJail.LocalSetting?.Sections;
            if (localSections != null)
            {
                foreach (var (name, section) in localSections)
                {
                    foreach (var (rule, value) in section.Rules)
                    {
                        rules[rule.ToString()] = value;
                    }
                }
            }
            var builder = new StringBuilder();
            builder.AppendLine("Текущие правила:\n");
            foreach (var (rule, value) in rules)
            {
                builder.AppendLine($"{rule} : {value}");
            }

            return builder.ToString();
        }
    }
}