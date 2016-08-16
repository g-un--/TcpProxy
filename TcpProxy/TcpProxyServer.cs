using Akka.Actor;
using Akka.Event;
using Akka.IO;
using System;
using System.Net;

namespace TcpProxy
{
    public class TcpProxyServer : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Logging.GetLogger(Context);
        private readonly IActorRef manager = Context.System.Tcp();

        public TcpProxyServer(EndPoint listeningEndPoint, EndPoint linkEndPoint)
        {
            manager.Tell(new Tcp.Bind(Self, listeningEndPoint));

            Receive<Tcp.Bound>(message => _log.Debug($"Proxy listening on: {message.LocalAddress}"));
            Receive<Tcp.Connected>(message =>
            {
                _log.Debug($"Client connected: {message.RemoteAddress}");
                var linkActor = Context.ActorOf(Props.Create(() => new TcpLink(message.RemoteAddress, Sender, linkEndPoint)));
                Sender.Tell(new Tcp.Register(linkActor));
            });

            Receive<Terminated>(message => Context.Stop(Self));
            Receive<Tcp.CommandFailed>(message => Context.Stop(Self));
        }
    }
}
