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
        protected IServiceSaver jailSaver;
        protected IServiceSaver filterSaver;

        public string Message { get; protected set; }

        public ShellState(Shell shell, IServiceContainer serviceContainer, IService currentJail, IService currentFilter, IServiceFactory serviceFactory, IServiceSaver jailSaver, IServiceSaver filterSaver)
        {
            this.shell = shell;
            this.serviceContainer = serviceContainer;
            this.currentJail = currentJail;
            this.currentFilter = currentFilter;
            this.serviceFactory = serviceFactory;
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

        private string PrintCurrentRules()
        {
            var rules = Commands.ManageJail(serviceContainer, currentJail.Name);
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