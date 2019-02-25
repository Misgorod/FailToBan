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

            return builder.ToString();
        }

        public async Task WriteAsync(string message, int clientId)
        {
            var (serverStream, streamWriter, _) = clients[clientId];
            if (serverStream.IsConnected)
            {
                await streamWriter.WriteLineAsync(message);
                await streamWriter.WriteLineAsync("end");
                await streamWriter.FlushAsync();
            }
        }

        public bool IsConnected(int clientId)
        {
            return clients[clientId].serverStream.IsConnected;
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
