using RiptideNetworking;
using HLRiptide.NetworkedObject;
using UnityEngine;

namespace HLRiptide.Util.MessageUtil
{
    public static class MessageExtentions
    {
        public static Message Add(this Message message, NetworkedObjectInfo value)
        {
            message.Add(value.id);
            message.Add(value.position);
            message.Add(value.rotation);
            message.Add(value.scale);

            return message;
        }

        public static NetworkedObjectInfo GetNetworkedObjectInfo(this Message message)
        {
            return new NetworkedObjectInfo(message.GetUInt(), message.GetVector3(), message.GetVector3(), message.GetVector3());
        }
    }
}