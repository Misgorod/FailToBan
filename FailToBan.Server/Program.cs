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
            try
            {
                using (var server = new ServerPipe("VICFTB"))
                {
                    while (true)
                    {
                        Console.WriteLine("Waiting for connection");
                        var clientId = await server.WaitForConnectionAsync();
                        Console.WriteLine($"Got connection with id {clientId}");
                        var processTask = ProcessClientAsync(server, clientId).ContinueWith(task =>
                        {
                            Console.WriteLine($"Exception got for client with id: {clientId}");
                            var exception = task.Exception;
                            Console.WriteLine(exception?.Message);
                        }, TaskContinuationOptions.OnlyOnFaulted);
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

        private static async Task ProcessClientAsync(ServerPipe server, int clientId)
        {
            while (server.IsConnected(clientId))
            {
                var message = await server.ReadAsync(clientId);
                Console.WriteLine($"Got message from client with id: {clientId}\n" +
                                  $"Message: {message}");
                await server.WriteAsync("Server message", clientId);
            }
        }
    }
}
