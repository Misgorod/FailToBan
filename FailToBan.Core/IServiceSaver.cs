namespace FailToBan.Core
{
    public interface IServiceSaver
    {
        void Save(string path, IService service);
    }
}