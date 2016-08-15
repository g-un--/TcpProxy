using Akka.Actor;
using Akka.IO;
using System.Net;

namespace TcpProxy
{
    internal class TcpLink : ReceiveActor
    {
        public TcpLink(EndPoint senderAddress, IActorRef connection, EndPoint linkAddress)
        {
            var tcpClient = Context.Watch(Context.ActorOf(Props.Create(() => new TcpClient(linkAddress))));

            Receive<Tcp.Received>(message =>
            {
                tcpClient.Tell(Messages.SendMessage.Create(message.Data));
            });
            Receive<Messages.SendMessage>(message =>
            {
                connection.Tell(Tcp.Write.Create(message.Data));
            });

            Receive<Tcp.ConnectionClosed>(closed => Context.Stop(Self));
            Receive<Terminated>(terminated => Context.Stop(Self));
        }
    }
}
