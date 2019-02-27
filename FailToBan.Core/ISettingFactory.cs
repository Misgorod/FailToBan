using System.IO;

namespace FailToBan.Core
{
    public interface ISettingFactory
    {
        ISetting Build();
        ISetting Build(string configuration);
    }
}