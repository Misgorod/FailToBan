namespace FailToBan.Core
{
    public interface IServiceContainerBuilder
    {
        IServiceContainer Build();
        IServiceContainerBuilder BuildDefault(string path);
        IServiceContainerBuilder BuildJails(string path);
        IServiceContainerBuilder BuildActions(string path);
        IServiceContainerBuilder BuildFilters(string path);
    }
}