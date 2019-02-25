namespace FailToBan.Core
{
    public interface ISetting
    {
        ISection GetSection(string name);

        bool AddSection(string name, ISection section);

        string ToString();

        ISetting Clone();
    }
}