using Akka.Actor;
using Akka.IO;
using System.Net;

namespace TcpProxy
{
    internal class TcpClient : ReceiveActor, IWithUnboundedStash
    {
        private readonly IActorRef manager = Context.System.Tcp();

        public IStash Stash { get; set; }

        public TcpClient(EndPoint endPoint)
        {
            manager.Tell(new Tcp.Connect(endPoint));

            Receive<Tcp.Connected>(message =>
            {
                Sender.Tell(new Tcp.Register(Self));
                Stash.UnstashAll();
                Become(() => Connected(Sender));
            });

            Receive<Tcp.Received>(_ => Stash.Stash());
            Receive<Tcp.ErrorClosed>(message => Context.Stop(Self));
            Receive<Tcp.CommandFailed>(message => Context.Stop(Self));
        }

        public void Connected(IActorRef connection)
        {
            Receive<Tcp.Received>(message =>
            {
                Context.Parent.Tell(Messages.SendMessage.Create(message.Data));
            });
            Receive<Messages.SendMessage>(message =>
            {
                connection.Tell(Tcp.Write.Create(message.Data));
            });

            Receive<Tcp.PeerClosed>(message => Context.Stop(Self));
            Receive<Tcp.CommandFailed>(message => Context.Stop(Self));
            Receive<Terminated>(terminated => Context.Stop(Self));
        }
    }
}
