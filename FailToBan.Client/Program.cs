using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Http;
using System.Threading.Tasks;

namespace FailToBan.Client
{
    internal class Program
    {
        private bool run = false;

        private void LogData(string text, Logger.From @from, Logger.LogType type) => Program.LogData(text, @from, type);

        private async Task Main(string[] args)
        {
            try
            {
                using (var clientPipe = new ClientPipe("VICFTB"))
                {
                    LogData("Инициализация Pipe коннектора", Logger.From.Client, Logger.LogType.Debug);
                    LogData($"Аргументы запуска \"{string.Join(" ", args)}\"", Logger.From.Client, Logger.LogType.Debug);
                    await clientPipe.ConnectAsync();

                    switch (string.Join(" ", args))
                    {
                        case "create":
                            run = true;
                            break;

                        case "edit":
                            run = true;
                            break;
                    }

                    await clientPipe.WriteAsync(string.Join(" ", args));
                    LogData($"Ввод пользователя: {string.Join(" ", args)}", Logger.From.Client, Logger.LogType.Debug);
                    var response = await clientPipe.ReadAsync();
                    LogData($"Ответ сервера: {response}", Logger.From.Client, Logger.LogType.Message);

                    while (run)
                    {
                        var request = await Console.In.ReadLineAsync();
                        LogData($"Ввод пользователя: {request}", Logger.From.Client, Logger.LogType.Debug);

                        if (request == "q")
                        {
                            break;
                        }

                        await clientPipe.WriteAsync(request);
                        response = await clientPipe.ReadAsync();
                        LogData($"Ответ сервера: {response}", Logger.From.Client, Logger.LogType.Message);
                    }
                }
            }
            catch (Exception e)
            {
                LogData(e.Message, Logger.From.Client, Logger.LogType.Error);
                LogData(e.StackTrace, Logger.From.Client, Logger.LogType.Error);
            }
        }
    }
}
