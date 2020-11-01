using System;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace TcpChat
{
    public class SocketClient
    {
        private const int BufferSize = 1024;

        private byte[] _buffer = new byte[1024];

        public string ClientName = "";
        
        public NetworkStream ClientStreamContext { get; set; }
        
        public string SocketId { get; set; }
        
        
        public void SendPacketIntoSocket(string message)
        {
            var messageInBytes = Encoding.ASCII.GetBytes(message);
            ClientStreamContext.Write(messageInBytes, 0, messageInBytes.Length);
        }
        
        public static SocketClient Create(NetworkStream clientStreamContext)
        {
            return new SocketClient
            {
                ClientStreamContext = clientStreamContext,
                SocketId = Guid.NewGuid().ToString(),
            };
        }
        
        public void ReadThread()
        {
            while (true)
            {
                var readBytesCount = ClientStreamContext.Read(_buffer, 0, _buffer.Length);
                if (readBytesCount > 0)
                {
                    ChatStore.HandleNewMessage(_buffer, this, readBytesCount);
                    Array.Clear(_buffer, 0, _buffer.Length);
                }
            }
        }

        public async Task StartReadThread()
        {
            await Task.Run(ReadThread);
        }
    }
}