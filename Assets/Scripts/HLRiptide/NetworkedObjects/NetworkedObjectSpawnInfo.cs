using RiptideNetworking;
using System.Collections.Generic;
using HLRiptide.Util.MessageUtil;
using Unity.Collections;
using UnityEngine;

namespace HLRiptide.NetworkedObjects
{
    public class NetworkedObjectSpawnInfo
    {
        public ushort clientIdWithAuthority;
        public int objectPrefabIndex;

        public List<int[]> commandHashCodes;
        public List<uint[]> overrideIds;

        public NetworkedObjectInfo objectInfo;

        public NetworkedObjectSpawnInfo(NetworkedObjectInfo objectInfo, ushort clientIdWithAuthority, int objectPrefabIndex)
        {
            this.clientIdWithAuthority = clientIdWithAuthority;
            this.objectPrefabIndex = objectPrefabIndex;
            this.objectInfo = objectInfo;

            commandHashCodes = new List<int[]>();
            overrideIds = new List<uint[]>();
        }

        public static void AddCommandArgToMessage(Message message, NetworkedObjectSpawnInfo networkedObjectSpawnInfo)
        {
            message.Add(networkedObjectSpawnInfo.objectInfo);
            message.Add(networkedObjectSpawnInfo.clientIdWithAuthority);
            message.Add((ushort)networkedObjectSpawnInfo.objectPrefabIndex);

            message.Add((ushort)networkedObjectSpawnInfo.commandHashCodes.Count);

            for (int i = 0; i < networkedObjectSpawnInfo.commandHashCodes.Count; i++) message.Add(networkedObjectSpawnInfo.commandHashCodes[i]);

            message.Add((ushort)networkedObjectSpawnInfo.overrideIds.Count);

            for (int i = 0; i < networkedObjectSpawnInfo.overrideIds.Count; i++) message.Add(networkedObjectSpawnInfo.overrideIds[i]);
        }

        public static NetworkedObjectSpawnInfo GetCommandArgFromMessage(Message message)
        {
            NetworkedObjectSpawnInfo networkedObjectSpawnInfo =  new NetworkedObjectSpawnInfo(message.GetNetworkedObjectInfo(), message.GetUShort(), message.GetUShort());

            int commandHashCodesLength = message.GetUShort();

            for (int i = 0; i < commandHashCodesLength; i++) networkedObjectSpawnInfo.commandHashCodes.Add(message.GetInts());

            int overrideIdsLength = message.GetUShort();

            for (int i = 0; i < overrideIdsLength; i++) networkedObjectSpawnInfo.overrideIds.Add(message.GetUInts());

            return networkedObjectSpawnInfo;
        }
    }
}