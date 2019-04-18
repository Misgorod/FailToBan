using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Pipes;
using System.Linq;

namespace FailToBan.Core
{
    public class ServiceContainer : IServiceContainer
    {
        private IService jail;
        private readonly Dictionary<string, IService> jails;
        private readonly Dictionary<string, IService> actions;
        private readonly Dictionary<string, IService> filters;

        public Dictionary<string, IService> Jails => jails.Select(service => new KeyValuePair<string, IService>(service.Key, service.Value.Clone())).ToDictionary(pair => pair.Key, pair => pair.Value);
        public Dictionary<string, IService> Actions => actions.Select(service => new KeyValuePair<string, IService>(service.Key, service.Value.Clone())).ToDictionary(pair => pair.Key, pair => pair.Value);
        public Dictionary<string, IService> Filters => filters.Select(service => new KeyValuePair<string, IService>(service.Key, service.Value.Clone())).ToDictionary(pair => pair.Key, pair => pair.Value);

        //public ServiceContainer(IService jail, Dictionary<string, IService> jails, Dictionary<string, IService> actions, Dictionary<string, IService> filters)
        //{
        //    this.jail = jail;
        //    this.jails = jails;
        //    this.actions = actions;
        //    this.filters = filters;
        //}

        public ServiceContainer()
        {
            this.jail = null;
            this.jails = new Dictionary<string, IService>();
            this.actions = new Dictionary<string, IService>();
            this.filters = new Dictionary<string, IService>();
        }

        public IService GetDefault()
        {
            return jail;
        }

        public void SetDefault(IService jail)
        {
            this.jail = jail;
        }

        public IService GetJail(string name)
        {
            return jails.GetValueOrDefault(name, null);
        }

        public void SetJail(IService jail)
        {
            jails[jail.Name] = jail;
        }

        public bool DeleteJail(string name)
        {
            return jails.Remove(name);
        }

        public IService GetFilter(string name)
        {
            return filters.GetValueOrDefault(name, null);
        }

        public void SetFilter(IService filter)
        {
            filters[filter.Name] = filter;
        }

        public bool DeleteFilter(string name)
        {
            return filters.Remove(name);
        }

        public IService GetAction(string name)
        {
            return actions.GetValueOrDefault(name, null);
        }

        public void SetAction(IService action)
        {
            actions[action.Name] = action;
        }

        public bool DeleteAction(string name)
        {
            return actions.Remove(name);
        }

    }
}