using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpChat
{
    public class SocketServer
    {
        public static TcpListener Server = new TcpListener(IPAddress.Any, 80);
        
        private static List<Task> _socketsReedThread = new List<Task>();

        private byte[] _buffer = new byte[1024];
        private async Task ListenIncomingConnections()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    var client = Server.AcceptTcpClient();
                    var socketServerClient = client.GetStream();
                    var socketClient = SocketClient.Create(socketServerClient);
                    _socketsReedThread.Add(socketClient.StartReadThread());
                    socketClient.SendPacketIntoSocket("Enter your name: ");
                }
            });
        }

        public async Task Start()
        {
            Server.Start();
            var listenTread =  ListenIncomingConnections();
            await Task.WhenAny(listenTread);
            await Task.WhenAny(_socketsReedThread);
            throw new Exception("Some thread done");
        }

        public async Task Disconnect(SocketClient client)
        {
            _socketsReedThread.Remove(client.StartReadThread());
        }
    }
}