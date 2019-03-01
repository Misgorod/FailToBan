using System.IO;
using System.IO.Abstractions;

namespace FailToBan.Core
{
    public class ServiceSaver : IServiceSaver
    {
        private readonly IFileSystem fileSystem;
        public string Path { get; }

        public ServiceSaver(string path, IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
            this.Path = path;
        }

        public ServiceSaver(string path) : this(path, new FileSystem())
        { }

        public void Save(IService service)
        {
            var confPath = fileSystem.Path.Combine(Path, $"{service.Name}.conf");
            if (fileSystem.File.Exists(confPath))
            {
                fileSystem.File.Move(confPath, $"{confPath}.bak");
            }
            fileSystem.File.WriteAllText(confPath, service.ConfSetting.ToString());

            var localPath = fileSystem.Path.Combine(Path, $"{service.Name}.local");
            if (fileSystem.File.Exists(confPath))
            {
                fileSystem.File.Move(localPath, $"{localPath}.bak");
            }
            fileSystem.File.WriteAllText(localPath, service.LocalSetting.ToString());
        }
    }
}