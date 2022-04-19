using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HLRiptide.Util.MessageUtil
{
    class MessageBuffer
    {
        private readonly Dictionary<uint, Message> tickMessageBuffer;

        private readonly int maxMessageBufferSize;

        public MessageBuffer(int maxMessageBufferSize)
        {
            this.maxMessageBufferSize = maxMessageBufferSize;

            tickMessageBuffer = new Dictionary<uint, Message>();
        }

        public void AddMessageToBuffer(Message message)
        {
            uint tick = message.GetUInt();

            tickMessageBuffer.Add(tick, message);
        }

        public (uint, Message) PopMessageFromBuffer()
        {
            if (tickMessageBuffer.Count >= maxMessageBufferSize)
            {
                Debug.Log("here");
                return GetMessageWithLowestTickFromBuffer();
            }

            return (0, null);
        }

        private (uint, Message) GetMessageWithLowestTickFromBuffer(bool removeMessageFromBufferIfFound = true)
        {
            KeyValuePair<uint, Message> tickMessagePairReturn;

            foreach (KeyValuePair<uint, Message> tickMessagePair in tickMessageBuffer)
            {
                if (tickMessagePairReturn.Key > tickMessagePair.Key)
                {
                    tickMessagePairReturn = tickMessagePair;
                }
            }

            if (removeMessageFromBufferIfFound) tickMessageBuffer.Remove(tickMessagePairReturn.Key);

            return (tickMessagePairReturn.Key, tickMessagePairReturn.Value);
        }
    }
}
