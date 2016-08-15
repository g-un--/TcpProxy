using Akka.IO;

namespace TcpProxy
{
    internal class Messages
    {
        public class SendMessage
        {
            private SendMessage() { }

            public static SendMessage Create(ByteString data)
            {
                return new SendMessage { Data = data };
            }

            public ByteString Data { get; private set; }
        }
    }
}
