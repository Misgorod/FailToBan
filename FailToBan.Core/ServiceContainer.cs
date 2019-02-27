using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Pipes;
using System.Linq;

namespace FailToBan.Core
{
    public class ServiceContainer : IServiceContainer
    {
        private readonly IService jail;
        private readonly Dictionary<string, IService> jails;
        private readonly Dictionary<string, IService> actions;
        private readonly Dictionary<string, IService> filters;

        public Dictionary<string, IService> Jails => jails.Select(service => new KeyValuePair<string, IService>(service.Key, service.Value.Clone())).ToDictionary(pair => pair.Key, pair => pair.Value);
        public Dictionary<string, IService> Actions => actions.Select(service => new KeyValuePair<string, IService>(service.Key, service.Value.Clone())).ToDictionary(pair => pair.Key, pair => pair.Value);
        public Dictionary<string, IService> Filters => filters.Select(service => new KeyValuePair<string, IService>(service.Key, service.Value.Clone())).ToDictionary(pair => pair.Key, pair => pair.Value);

        public ServiceContainer(IService jail, Dictionary<string, IService> jails, Dictionary<string, IService> actions, Dictionary<string, IService> filters)
        {
            this.jail = jail;
            this.jails = jails;
            this.actions = actions;
            this.filters = filters;
        }

        public IService GetDefault()
        {
            return jail;
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