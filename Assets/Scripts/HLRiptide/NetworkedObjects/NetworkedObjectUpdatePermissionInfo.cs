using RiptideNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLRiptide.NetworkedObjects
{
    public class NetworkedObjectUpdatePermissionInfo
    {
        public uint id;
        public ushort networkId;

        public NetworkedObjectUpdatePermissionInfo(uint id, ushort networkId)
        {
            this.id = id;
            this.networkId = networkId;
        }

        public static NetworkedObjectUpdatePermissionInfo GetCommandArgsFromMessage(Message message)
        {
            return new NetworkedObjectUpdatePermissionInfo(message.GetUInt(), message.GetUShort());
        }

        public static void AddCommandArgToMessage(Message message, NetworkedObjectUpdatePermissionInfo networkedObjectUpdatePermissionInfo)
        {
            message.Add(networkedObjectUpdatePermissionInfo.id);
            message.Add(networkedObjectUpdatePermissionInfo.networkId);
        }
    }
}
