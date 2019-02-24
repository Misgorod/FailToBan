using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace FailToBan.Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                var clientStream = new NamedPipeClientStream(".", "VICFTB", PipeDirection.InOut);
                await clientStream.ConnectAsync();
                var message = "";
                using (var writer = new StreamWriter(clientStream))
                    while (message != "end")
                    {
                        message = await Console.In.ReadLineAsync();
                        await writer.WriteLineAsync(message);
                        await writer.FlushAsync();
                        await clientStream.FlushAsync();
                    }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.ReadLine();
        }
    }
}
