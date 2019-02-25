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
            var message = "";
            while (pipeClientStream.IsConnected && (message = await clientReader.ReadLineAsync()) != "end")
            {
                builder.AppendLine(message);
            }

            return builder.ToString();
        }

        public async Task WriteAsync(string message)
        {
            if (pipeClientStream.IsConnected)
            {
                await clientWriter.WriteLineAsync(message);
                await clientWriter.WriteLineAsync("end");
                await clientWriter.FlushAsync();
            }
        }

        public void Dispose()
        {
            pipeClientStream.Dispose();
        }
    }
}