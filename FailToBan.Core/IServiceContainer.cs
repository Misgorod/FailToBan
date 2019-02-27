using System.Collections.Generic;

namespace FailToBan.Core
{
    public interface IServiceContainer
    {
        Dictionary<string, IService> Jails { get; }
        Dictionary<string, IService> Actions { get; }
        Dictionary<string, IService> Filters { get; }
        IService GetDefault();
        IService GetJail(string name);
        IService GetFilter(string name);
        IService GetAction(string name);
    }
}