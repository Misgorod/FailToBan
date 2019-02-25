namespace FailToBan.Core
{
    public interface IServiceContainer
    {
        IService GetJail(string name);
        IService GetFilter(string name);
        IService GetAction(string name);
    }
}