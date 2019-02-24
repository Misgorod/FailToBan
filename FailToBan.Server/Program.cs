using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace FailToBan.Server
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            int i = 0;
            while (true)
            {
                var serverStream = new NamedPipeServerStream("VICFTB", PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte);
                await serverStream.WaitForConnectionAsync();
                var task = Task.Run(() => RunAsync(serverStream, i++));
            }
        }

        private static async Task RunAsync(Stream serverStream, int clientId)
        {
            var message = "";
            using (var reader = new StreamReader(serverStream))
            while (message != "end" && !reader.EndOfStream)
            {
                message = await reader.ReadLineAsync();
                await ProcessAsync(message, clientId);
            }
        }

        private static async Task ProcessAsync(string message, int clientId)
        {
            await Console.Out.WriteLineAsync($"Message from {clientId}: {message}");
        }
    }
}
