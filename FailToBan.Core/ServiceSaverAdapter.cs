using System;
using System.IO.Abstractions;

namespace FailToBan.Core
{
    public class ServiceSaverAdapter : IServiceSaver
    {
        private readonly IServiceSaver[] savers;

        public ServiceSaverAdapter(params IServiceSaver[] savers)
        {
            this.savers = savers;
        }

        public void Save(IService service)
        {
            foreach (var saver in savers)
            {
                saver.Save(service);
            }
        }

        public void Delete(IService service)
        {
            foreach (var saver in savers)
            {
                saver.Delete(service);
            }
        }
    }
}