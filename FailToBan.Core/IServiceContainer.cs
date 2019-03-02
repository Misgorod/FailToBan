using System.Collections.Generic;

namespace FailToBan.Core
{
    public interface IServiceContainer
    {
        Dictionary<string, IService> Jails { get; }
        Dictionary<string, IService> Actions { get; }
        Dictionary<string, IService> Filters { get; }
        IService GetDefault();
        void SetDefault(IService jail);
        IService GetJail(string name);
        void SetJail(IService jail);
        bool DeleteJail(string name);
        IService GetFilter(string name);
        void SetFilter(IService filter);
        bool DeleteFilter(string name);
        IService GetAction(string name);
        void SetAction(IService action);
        bool DeleteAction(string name);
    }
}