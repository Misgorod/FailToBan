using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FailToBan.Server
{
    public class Logger
    {
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

        private string pathToLog;
        private StreamWriter writer;

        public Logger(string LogPath)
        {
            pathToLog = LogPath;
            if (!File.Exists(pathToLog))
            {
                List<string> PathList = pathToLog.Split('/').ToList();
                for (int i = 2; i < PathList.Count; i++)
                {
                    string TempPath = string.Join('/', PathList.GetRange(0, i).ToArray());
                    if (!Directory.Exists(TempPath))
                    {
                        Directory.CreateDirectory(TempPath);
                    }
                }
                FileStream Stream = File.Create(pathToLog);
                Stream.Close();
            }
        }

        private string PrepareText(string Text, From From, LogType Type)
        {
            string Result = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Result += " " + FromToText(From);
            Result += " " + TypeToText(Type);
            Result += " " + Text;
            return Result;
        }

        public async void Log(string Text, From From, LogType Type)
        {
            await LogAsync(Text, From, Type);
        }

        public async Task LogAsync(string Text, From From, LogType Type)
        {
            using (writer = new StreamWriter(File.Open(pathToLog, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)))
            {
                await writer.WriteLineAsync(PrepareText(Text, From, Type));
                try
                {
                    writer.Flush();
                }
                catch (ObjectDisposedException)
                { }
            }
        }

        private string FromToText(From From)
        {
            switch (From)
            {
                case From.Server:
                    return "Server";
                case From.Client:
                    return "Client";
            }
            return "Unknown";
        }

        private string TypeToText(LogType Type)
        {
            switch (Type)
            {
                case LogType.Debug:
                    return "Отладка";
                case LogType.Error:
                    return "Ошибка";
                case LogType.Message:
                    return "Сообщение";
                case LogType.Warning:
                    return "Предупреждение";
            }
            return "Unknown";
        }
    }
}