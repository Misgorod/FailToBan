using System.Collections.Generic;

namespace FailToBan.Core
{
    public class ServiceContainer : IServiceContainer
    {
        private readonly IService jail;
        private readonly Dictionary<string, IService> jails;
        private readonly Dictionary<string, IService> actions;
        private readonly Dictionary<string, IService> filters;

        public ServiceContainer(IService jail, Dictionary<string, IService> jails, Dictionary<string, IService> actions, Dictionary<string, IService> filters)
        {
            this.jail = jail;
            this.jails = jails;
            this.actions = actions;
            this.filters = filters;
        }

        public IService GetJail(string name)
        {
            return jails.GetValueOrDefault(name, null);
        }

        public IService GetFilter(string name)
        {
            return filters.GetValueOrDefault(name, null);
        }

        public IService GetAction(string name)
        {
            return actions.GetValueOrDefault(name, null);
        }
    }
}