using RiptideNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLRiptide.Util.MessageUtil
{
    class MessageBuffer
    {
        private readonly List<Message> messageBuffer;

        private readonly int maxMessageBufferSize;

        public MessageBuffer(int maxMessageBufferSize)
        {
            this.maxMessageBufferSize = maxMessageBufferSize;

            messageBuffer = new List<Message>();
        }

        public void AddMessageToBuffer(Message message)
        {
            messageBuffer.Add(message);
        }

        public Message PopMessageFromBuffer()
        {
            Message message = null;

            if (messageBuffer.Count >= maxMessageBufferSize)
            {
                message = messageBuffer[0];

                messageBuffer.RemoveAt(0);
            }

            return message;
        }
    }
}
