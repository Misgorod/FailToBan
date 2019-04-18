using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Http;
using System.Threading.Tasks;

namespace FailToBan.Client
{
    internal class Program
    {
        private static bool run;
        private static Logger logger;

        private static async Task LogAsync(string text, Logger.From @from, Logger.LogType type)
        {
            await logger.LogAsync(text, @from, type);
            Console.WriteLine(text);
        }

        public static async Task Main(string[] args)
        {
            logger = new Logger();
            try
            {
                using (var clientPipe = new ClientPipe("VICFTB"))
                {
                    await LogAsync("Инициализация Pipe коннектора", Logger.From.Client, Logger.LogType.Debug);
                    await clientPipe.ConnectAsync();

                    switch (string.Join(" ", args))
                    {
                        case "create":
                        case "edit":
                            run = true;
                            break;

                        default:
                            run = false;
                            break;
                    }

                    await clientPipe.WriteAsync(string.Join(" ", args));
                    var response = await clientPipe.ReadAsync();
                    await LogAsync($"Ответ сервера:\n{response}", Logger.From.Client, Logger.LogType.Debug);
                    //Console.WriteLine(response);
                    while (run)
                    {
                        var request = await Console.In.ReadLineAsync();
                        await LogAsync($"Ввод пользователя: {request}", Logger.From.Client, Logger.LogType.Debug);
                        await clientPipe.WriteAsync(request);

                        if (request == "q")
                        {
                            break;
                        }

                        response = await clientPipe.ReadAsync();
                        await LogAsync($"Ответ сервера:\n{response}", Logger.From.Client, Logger.LogType.Debug);
                        //Console.WriteLine(response);
                    }
                }
            }
            catch (Exception e)
            {
                await LogAsync(e.Message, Logger.From.Client, Logger.LogType.Error);
                await LogAsync(e.StackTrace, Logger.From.Client, Logger.LogType.Error);
            }
        }
    }
}
