
namespace TcpChat
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new SocketServer();
            server.Start().Wait();
        }
    }
}