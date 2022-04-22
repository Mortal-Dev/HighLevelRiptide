using RiptideNetworking;
using HLRiptide.NetworkedObjects;
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

        public static Vector3 GetVector3(this Message message)
        {
            return new Vector3(message.GetFloat(), message.GetFloat(), message.GetFloat());
        }

        public static Message Add(this Message message, Vector3 vector3)
        {
             return message.Add(vector3.x).Add(vector3.y).Add(vector3.z);
        }
    }
}