using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FailToBan.Server
{
    public class Logger
    {
        private readonly string logPath;

        public Logger(string logPath = "/_Data/Logs/CLI/Main.log")
        {
            this.logPath = logPath;

            if (File.Exists(logPath))
                return;

            var directory = Path.GetDirectoryName(logPath);
            Directory.CreateDirectory(directory);
            File.Create(this.logPath).Close();
        }

        private string PrepareText(string text, From @from, LogType type)
        {
            var result = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            result += " " + GetText(@from);
            result += " " + GetText(type);
            result += " " + text;
            return result;
        }

        public void Log(string text, From @from, LogType type)
        {
            using (var writer =
                new StreamWriter(File.Open(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
            {
                writer.WriteLine(PrepareText(text, @from, type));
            }
        }

        public async Task LogAsync(string text, From @from, LogType type)
        {
            using (var writer =
                new StreamWriter(File.Open(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
            {
                await writer.WriteLineAsync(PrepareText(text, @from, type));
            }
        }

        private string GetText(From @from)
        {
            switch (@from)
            {
                case From.Server:
                    return "Сервер";
                case From.Client:
                    return "Клиент";
                case From.Unknown:
                    return "Неизвестно";
                default:
                    throw new ArgumentOutOfRangeException(nameof(@from), @from, null);
            }
        }

        private string GetText(LogType type)
        {
            switch (type)
            {
                case LogType.Debug:
                    return "Отладка";
                case LogType.Error:
                    return "Ошибка";
                case LogType.Message:
                    return "Сообщение";
                case LogType.Warning:
                    return "Предупреждение";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public enum From
        {
            Unknown,
            Server,
            Client
        }

        public enum LogType
        {
            Message,
            Warning,
            Error,
            Debug
        }
    }
}