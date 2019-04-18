namespace FailToBan.Core
{
    public interface IServiceSaver
    {
        void Save(IService service);
        void Delete(IService service);
    }
}