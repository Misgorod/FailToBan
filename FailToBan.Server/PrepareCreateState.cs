using System.IO;
using System.Text;
using FailToBan.Core;

namespace FailToBan.Server
{
    public class PrepareCreateState : ShellState
    {
        public PrepareCreateState(Shell shell) : base(shell)
        { }

        public override string Handle(string[] values)
        {
            if (values.Length == 0)
            {
                return "Неверное количество аргументов";
            }

            if (values[0] == "--List")
            {
                var confRules = shell.ServiceContainer
                    .GetDefault()
                    .ConfSetting
                    .GetSection("DEFAULT")
                    .Rules;
                var localRules = shell.ServiceContainer
                    .GetDefault()
                    .LocalSetting
                    .GetSection("DEFAULT")
                    .Rules;
                foreach (var (type, value) in confRules)
                {
                    if (!localRules.ContainsKey(type))
                    {
                        localRules.Add(type, value);
                    }
                }

                var builder = new StringBuilder();
                builder.AppendLine("Список правил, установленных по умолчанию:");
                foreach (var (key, value) in localRules)
                {
                    builder.AppendLine($"{key} : {value}");
                }
                return builder.ToString();
            }

            var serviceName = values[0];
            var service = shell.ServiceContainer.GetJail(serviceName);

            if (service != null)
            {
                return "Сервис с таким именем уже существует \nДля изменения существующего сервиса используйте команду edit";
            }

            shell.CurrentJail = new Setting(Path.Combine(settingContainer.JailsPath, currentFullName), Setting.SettingType.jail, false);
            step++;
            return $"Создание сервиса";
        }
    }
}