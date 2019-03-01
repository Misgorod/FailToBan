using System;
using System.IO;
using System.IO.Pipes;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FailToBan.Client
{
    public class ClientPipe : IDisposable
    {
        private readonly NamedPipeClientStream pipeClientStream;
        private readonly StreamWriter clientWriter;
        private readonly StreamReader clientReader;

        public ClientPipe(string pipeName)
        {
            pipeClientStream = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            clientReader = new StreamReader(pipeClientStream);
            clientWriter = new StreamWriter(pipeClientStream);
        }

        public async Task ConnectAsync()
        {
            await pipeClientStream.ConnectAsync();
        }

        public async Task<string> ReadAsync()
        {
            var builder = new StringBuilder();
            var message = await clientReader.ReadLineAsync();
            while (pipeClientStream.IsConnected && message != "end" && message != "exit")
            {
                builder.AppendLine(message);
                message = await clientReader.ReadLineAsync();
            }

            if (message == "exit")
            {
                Dispose();
            }
            Console.WriteLine($"CLIENT PIPE GOT {builder.ToString().TrimEnd('\n')}");
            return builder.ToString().TrimEnd('\n');
        }

        public async Task WriteAsync(string message)
        {
            if (pipeClientStream.IsConnected)
            {
                await clientWriter.WriteLineAsync(message);
                await clientWriter.WriteLineAsync("end");
                await clientWriter.FlushAsync();
            }

            Console.WriteLine($"CLIENT PIPE SENT {message}");
            Console.WriteLine($"CLIENT STOP SENT");
        }

        public void Dispose()
        {
            pipeClientStream.Dispose();
        }
    }
}