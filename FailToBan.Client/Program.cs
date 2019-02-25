using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Http;
using System.Threading.Tasks;

namespace FailToBan.Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                using (var clientPipe = new ClientPipe("VICFTB"))
                {
                    await clientPipe.ConnectAsync();

                    while (true)
                    {
                        string request = await Console.In.ReadLineAsync();
                        await clientPipe.WriteAsync(request);
                        string response = await clientPipe.ReadAsync();
                        await Console.Out.WriteLineAsync($"Server response: {response}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}
