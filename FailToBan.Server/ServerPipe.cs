using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace FailToBan.Server
{
    public class ServerPipe : IDisposable
    {
        private readonly string pipeName;
        private readonly ConcurrentDictionary<int, (NamedPipeServerStream serverStream, StreamWriter streamWriter, StreamReader streamReader)> clients;
        private int counter = 0;

        public ServerPipe(string pipeName)
        {
            this.pipeName = pipeName;
            clients = new ConcurrentDictionary<int, (NamedPipeServerStream, StreamWriter, StreamReader)>();
        }

        public async Task<int> WaitForConnectionAsync()
        {
            var pipeServerStream = new NamedPipeServerStream(pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            await pipeServerStream.WaitForConnectionAsync();
            var streamReader = new StreamReader(pipeServerStream);
            var streamWriter = new StreamWriter(pipeServerStream);

            clients.TryAdd(counter, (pipeServerStream, streamWriter, streamReader));
            return counter++;
        }

        public async Task<string> ReadAsync(int clientId)
        {
            var builder = new StringBuilder();
            var message = "";
            var (serverStream, _, streamReader) = clients[clientId];
            while (serverStream.IsConnected && (message = await streamReader.ReadLineAsync()) != "end")
            {
                builder.AppendLine(message);
            }
            Console.WriteLine($"SERVER PIPE GOT {builder.ToString().TrimEnd('\n')}");
            return builder.ToString().TrimEnd('\n');
        }

        public async Task WriteAsync(string message, int clientId)
        {
            bool valueGot = clients.TryGetValue(clientId, out var result);
            if (result.serverStream.IsConnected && valueGot)
            {
                await result.streamWriter.WriteLineAsync(message);
                await result.streamWriter.WriteLineAsync("end");
                await result.streamWriter.FlushAsync();
            }
            Console.WriteLine($"SERVER PIPE SENT {message}");
            Console.WriteLine($"SERVER STOP SENT");
        }

        public async Task WriteLastAsync(string message, int clientId)
        {
            bool valueGot = clients.TryGetValue(clientId, out var result);
            if (result.serverStream.IsConnected && valueGot)
            {
                await result.streamWriter.WriteLineAsync(message);
                await result.streamWriter.WriteLineAsync("exit");
                await result.streamWriter.FlushAsync();
            }
            Console.WriteLine($"SERVER PIPE SENT {message}");
            Console.WriteLine($"SERVER STOP SENT");
            Disconnect(clientId);
        }

        public bool IsConnected(int clientId)
        {
            return clients.ContainsKey(clientId) && clients[clientId].serverStream.IsConnected;
        }

        public bool Disconnect(int clientId)
        {
            return clients.TryRemove(clientId, out var streams);
        }

        public void Dispose()
        {
            foreach (var (serverStream, _, _) in clients.Values)
            {
                serverStream.Dispose();
            }

        }
    }
}
