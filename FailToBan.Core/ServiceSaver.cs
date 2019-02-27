using System.IO;
using System.IO.Abstractions;

namespace FailToBan.Core
{
    public class ServiceSaver : IServiceSaver
    {
        private readonly IFileSystem fileSystem;

        public ServiceSaver(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public ServiceSaver() : this(new FileSystem())
        { }

        public void Save(string path, IService service)
        {
            var confPath = fileSystem.Path.Combine(path, $"{service.Name}.conf");
            if (fileSystem.File.Exists(confPath))
            {
                fileSystem.File.Move(confPath, $"{confPath}.bak");
            }
            fileSystem.File.WriteAllText(confPath, service.ConfSetting.ToString());

            var localPath = fileSystem.Path.Combine(path, $"{service.Name}.local");
            if (fileSystem.File.Exists(confPath))
            {
                fileSystem.File.Move(localPath, $"{localPath}.bak");
            }
            fileSystem.File.WriteAllText(localPath, service.LocalSetting.ToString());
        }
    }
}