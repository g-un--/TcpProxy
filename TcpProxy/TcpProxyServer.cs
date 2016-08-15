using Akka.Actor;
using Akka.IO;
using System;
using System.Net;

namespace TcpProxy
{
    public class TcpProxyServer : ReceiveActor
    {
        private readonly IActorRef manager = Context.System.Tcp();

        public TcpProxyServer(EndPoint listeningEndPoint, EndPoint linkEndPoint)
        {
            manager.Tell(new Tcp.Bind(Self, listeningEndPoint));

            Receive<Tcp.Bound>(message => Console.WriteLine($"Listening on: {message.LocalAddress}"));
            Receive<Tcp.Connected>(message =>
            {
                Console.WriteLine($"Client connected: {message.RemoteAddress}");
                var linkActor = Context.ActorOf(Props.Create(() => new TcpLink(message.RemoteAddress, Sender, linkEndPoint)));
                Sender.Tell(new Tcp.Register(linkActor));
            });

            Receive<Terminated>(message => Context.Stop(Self));
            Receive<Tcp.CommandFailed>(message => Context.Stop(Self));
        }
    }
}
