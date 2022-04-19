using HLRiptide.Util.MessageUtil;
using HLRiptide.NetworkedCommand;
using HLRiptide.Util.ContainerUtil;
using RiptideNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HLRiptide.Networks;
using UnityEngine;
using HLRiptide.NetworkedObjects;

namespace HLRiptide.Util.MessageUtil
{
    class MessageGenerator
    {
        public Message GenerateMessage(uint tick)
        {
            Message message = Message.Create(MessageSendMode.reliable, (ushort)UniversalMessageId.message);

            message.Add(tick);

            AddCommandsToMessage(message);

            AddNetworkedObjectInfosToMessage(message);

            return message;
        }

        public Message GenerateMessageForServerClient(uint tick, ushort clientId)
        {
            Message message = Message.Create(MessageSendMode.reliable, (ushort)UniversalMessageId.message);

            message.Add(tick);

            AddClientCommandsToMessage(message, clientId);

            //for networked objects
            message.Add(0);

            return message;
        }

        private void AddCommandsToMessage(Message message)
        {
            (List<NetworkedCommandBase> lowPriorityCommands, List<NetworkedCommandBase> mediumPriorityCommands, List<NetworkedCommandBase> highPriorityCommands) = GetNetworkedCommandsByPriority(NetworkManager.Singleton.NetworkedCommandContainer, ushort.MaxValue);

            message.Add(lowPriorityCommands.Count + mediumPriorityCommands.Count + highPriorityCommands.Count);

            AddCommandListToMessage(message, highPriorityCommands, ushort.MaxValue);
            AddCommandListToMessage(message, mediumPriorityCommands, ushort.MaxValue);
            AddCommandListToMessage(message, lowPriorityCommands, ushort.MaxValue);
        }

        private void AddClientCommandsToMessage(Message message, ushort clientId)
        {
            (List<NetworkedCommandBase> lowPriorityCommands, List<NetworkedCommandBase> mediumPriorityCommands, List<NetworkedCommandBase> highPriorityCommands) = GetNetworkedCommandsByPriority(NetworkManager.Singleton.NetworkedCommandContainer, clientId);

            message.Add(lowPriorityCommands.Count + mediumPriorityCommands.Count + highPriorityCommands.Count);

            AddCommandListToMessage(message, highPriorityCommands, clientId);
            AddCommandListToMessage(message, mediumPriorityCommands, clientId);
            AddCommandListToMessage(message, lowPriorityCommands, clientId);
        }

        private void AddCommandListToMessage(Message message, List<NetworkedCommandBase> networkedCommandBases, ushort clientId)
        {
            foreach (NetworkedCommandBase networkedCommandBase in networkedCommandBases)
            {
                AddCommandToMessage(message, networkedCommandBase, clientId);
            }
        }

        private void AddCommandToMessage(Message message, NetworkedCommandBase networkedCommandBase, ushort clientId)
        {
            if (PermissionMatches(networkedCommandBase))
            {
                if (clientId == ushort.MaxValue)
                {
                    networkedCommandBase.AddCommandArgsToMessage(message);
                }
                else
                {
                    networkedCommandBase.AddClientCommandArgsToMessage(clientId, message);
                }
            }
        }

        private void AddNetworkedObjectInfosToMessage(Message message)
        {
            List<NetworkedObject> networkedObjects = GetNetworkedObjectsWithChangedPositions();

            message.Add(networkedObjects.Count);

            foreach (NetworkedObject networkedObject in networkedObjects)
            {
                message.Add(networkedObject.GetNetworkedObjectInfo());
            }
        }

        private (List<NetworkedCommandBase>, List<NetworkedCommandBase>, List<NetworkedCommandBase>) GetNetworkedCommandsByPriority(Container<NetworkedCommandBase> networkedCommandContainer, ushort clientId)
        {
            return (GetNetworkedCommandBasesWithPriority(networkedCommandContainer, NetworkedCommandPriority.Low, clientId), GetNetworkedCommandBasesWithPriority(networkedCommandContainer, NetworkedCommandPriority.Medium, clientId), GetNetworkedCommandBasesWithPriority(networkedCommandContainer, NetworkedCommandPriority.High, clientId));
        }

        private List<NetworkedCommandBase> GetNetworkedCommandBasesWithPriority(Container<NetworkedCommandBase> networkedCommandContainer, NetworkedCommandPriority networkedCommandPriority, ushort clientId)
        {
            List<NetworkedCommandBase> networkedCommandBases = new List<NetworkedCommandBase>();

            foreach (NetworkedCommandBase networkedCommand in networkedCommandContainer.ContainerDict.Values)
            {
                if (networkedCommand.networkedCommandPriority == networkedCommandPriority && PermissionMatches(networkedCommand))
                {
                    if (networkedCommand.bufferedCommandArgs.Count > 0 && clientId == ushort.MaxValue) 
                    {
                        networkedCommandBases.Add(networkedCommand);
                    }
                    else if (networkedCommand.bufferedCommandArgsPerClient.TryGetValue(clientId, out List<object> list) && list.Count > 0)
                    {
                        networkedCommandBases.Add(networkedCommand);
                    }
                }
            }

            return networkedCommandBases;
        }

        private bool PermissionMatches(NetworkedCommandBase networkedCommandBase)
        {
            return (NetworkManager.Singleton.IsClient && networkedCommandBase.networkWithAuthority == NetworkPermission.Client || NetworkManager.Singleton.IsServer && networkedCommandBase.networkWithAuthority == NetworkPermission.Server);
        }

        private List<NetworkedObject> GetNetworkedObjectsWithChangedPositions()
        {
            List <NetworkedObject> networkedObjects = NetworkManager.Singleton.NetworkedObjectContainer.ContainerDict.Values.ToList();

            for (int i = networkedObjects.Count - 1; i >= 0; i--)
            {
                NetworkedObject networkedObject = networkedObjects[i];

                if (networkedObject.pastPosition.Equals(networkedObject.transform.position) && networkedObject.pastRotation.Equals(networkedObject.transform.rotation.eulerAngles) && networkedObject.pastScale.Equals(networkedObject.transform.localScale))
                {
                    networkedObjects.RemoveAt(i);
                }
                else
                {
                    networkedObject.UpdatePastTransform();
                }
            }

            return networkedObjects;
        }
    }
}