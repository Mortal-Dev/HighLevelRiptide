using HLRiptide.NetworkedCommand;
using HLRiptide.NetworkedObjects;
using HLRiptide.Util.ContainerUtil;
using RiptideNetworking;
using System.Collections.Generic;

namespace HLRiptide.Util.MessageUtil
{
    class MessageHandler
    {
        public void HandleMessage(Container<NetworkedCommandBase> networkedCommandContainer, Container<NetworkedObject> networkedObjectContainer, Message message, uint tick, ushort clientId)
        {
            if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.Network.networkSceneManager.IsLocalClientLoadingScene) return;

            ExecuteNetworkedCommandsFromMessage(message, networkedCommandContainer, tick, clientId);

            SetNetworkedObjectsFromMessage(message, networkedObjectContainer);
        }

        private void ExecuteNetworkedCommandsFromMessage(Message message, Container<NetworkedCommandBase> networkedCommandContainer, uint tick, ushort clientId)
        {
            int commandCount = message.GetInt();

            for (int i = 0; i < commandCount; i++) ExecuteNetworkedCommandFromMessage(message, networkedCommandContainer, clientId);
        }

        private void ExecuteNetworkedCommandFromMessage(Message message, Container<NetworkedCommandBase> networkedCommandContainer, ushort clientId)
        {
            uint networkedCommandId = message.GetUInt();

            int argCount = message.GetInt();

            NetworkedCommandBase networkedCommand = networkedCommandContainer.GetValue(networkedCommandId);

            if (networkedCommand.networkWithAuthority == NetworkPermission.Client && NetworkManager.Singleton.IsServer ||
                networkedCommand.networkWithAuthority == NetworkPermission.Server && NetworkManager.Singleton.IsClient)
            {
                //TODO fix this
                // if (networkedCommand.clientIdWithAuthority != clientId) return;

                for (int i = 0; i < argCount; i++)
                {
                    networkedCommandContainer.GetValue(networkedCommandId).ExecuteCommand(message);
                }
            }
        }

        private void SetNetworkedObjectsFromMessage(Message message, Container<NetworkedObject> networkedObjectContainer)
        {
            List<NetworkedObjectInfo> networkedObjectInfos = GetNetworkedObjectInfosFromMessage(message);

            foreach (NetworkedObjectInfo networkedObjectInfo in networkedObjectInfos)
            {
                NetworkedObject networkedObject = networkedObjectContainer.GetValue(networkedObjectInfo.id);

                if (networkedObject == null) continue;

                //if we have authority over the object, ignore this
                if (networkedObject.NetworkId == NetworkManager.Singleton.NetworkId) continue;



                networkedObject.SetNetworkedObjectInfo(networkedObjectInfo);
            }
        }

        private List<NetworkedObjectInfo> GetNetworkedObjectInfosFromMessage(Message message)
        {
            int networkedObjectCount = message.GetInt();

            List<NetworkedObjectInfo> networkedObjectInfos = new List<NetworkedObjectInfo>();

            for (int i = 0; i < networkedObjectCount; i++)
            {
                networkedObjectInfos.Add(message.GetNetworkedObjectInfo());
            }

            return networkedObjectInfos;
        }
    }
}