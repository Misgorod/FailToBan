using System;
using System.IO;
using System.IO.Abstractions;

namespace FailToBan.Core
{
    public class ServiceSaver : IServiceSaver
    {
        private readonly IFileSystem fileSystem;
        private readonly string path;

        public ServiceSaver(string path, IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
            this.path = path;
        }

        public ServiceSaver(string path) : this(path, new FileSystem())
        { }

        public void Save(IService service)
        {
            var confPath = fileSystem.Path.Combine(path, $"{service.Name}.conf");
            if (fileSystem.File.Exists($"{confPath}.bak"))
            {
                fileSystem.File.Delete($"{confPath}.bak");
            }
            if (fileSystem.File.Exists(confPath))
            {
                fileSystem.File.Move(confPath, $"{confPath}.bak");
            }
            fileSystem.File.WriteAllText(confPath, service.ConfSetting?.ToString());

            var localPath = fileSystem.Path.Combine(path, $"{service.Name}.local");
            if (fileSystem.File.Exists($"{localPath}.bak"))
            {
                fileSystem.File.Delete($"{localPath}.bak");
            }
            if (fileSystem.File.Exists(localPath))
            {
                fileSystem.File.Move(localPath, $"{localPath}.bak");
            }
            fileSystem.File.WriteAllText(localPath, service.LocalSetting?.ToString());
        }

        public void Delete(IService service)
        {
            var confPath = fileSystem.Path.Combine(path, $"{service.Name}.conf");
            if (fileSystem.File.Exists(confPath))
            {
                fileSystem.File.Delete(confPath);
            }

            var localPath = fileSystem.Path.Combine(path, $"{service.Name}.local");
            if (fileSystem.File.Exists(localPath))
            {
                fileSystem.File.Delete(localPath);
            }
        }
    }
}