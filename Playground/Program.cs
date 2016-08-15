using Akka.Actor;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TcpProxy;

namespace TcpProxyPlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            var localhost = IPAddress.Parse("127.0.0.1");
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 9002);
            var proxyEndPoint = new IPEndPoint(IPAddress.Loopback, 9001);

            var testMessage = "Hello world!";

            Task.Run(() =>
            {
                var testListener = new TcpListener(serverEndPoint);
                testListener.Start();
                var remoteClient = testListener.AcceptTcpClient();
                remoteClient.Client.Send(Encoding.UTF8.GetBytes(testMessage));
            });

            var system = ActorSystem.Create("TcpProxy");
            var tcpProxyServer = system.ActorOf(Props.Create(() => 
                new TcpProxyServer(proxyEndPoint, serverEndPoint)), "TcpProxyServer");

            var testClient = new TcpClient();
            testClient.Connect(proxyEndPoint);
            byte[] buffer = new byte[12];
            testClient.Client.Receive(buffer);

            Console.WriteLine(Encoding.UTF8.GetString(buffer));
        }
    }
}
