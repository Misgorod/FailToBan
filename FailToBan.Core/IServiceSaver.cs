namespace FailToBan.Core
{
    public interface IServiceSaver
    {
        string Path { get; }
        void Save(IService service);
    }
}