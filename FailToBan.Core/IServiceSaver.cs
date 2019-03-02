namespace FailToBan.Core
{
    public interface IServiceSaver
    {
        string Path { get; }
        void Save(IService service);
        void Delete(IService service);
    }
}