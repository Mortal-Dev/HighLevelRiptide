using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Assets.Scripts.HLRiptide.Util.SerializeUtil;
using HLRiptide.Util.MessageUtil;
using RiptideNetworking;
using HLRiptide.Util.ContainerUtil;

namespace HLRiptide.NetworkedCommand
{
    public class NetworkedCommand<T> : NetworkedCommandBase
    {
        internal Action<T> networkedCommand;

        internal readonly Action<Message, T> addCommandArgToMessage;
        internal readonly Func<Message, T> getCommandArgFromMessage;

        public NetworkedCommand(NetworkPermission networkWithAuthority, Action<T> networkedCommand, Action<Message, T> addCommandArgToMessage = null, 
            Func<Message, T> getCommandArgFromMessage = null) : base(networkWithAuthority, NetworkedCommandPriority.Low)
        {
            NetworkManager.Singleton.OnLocalClientConnect += OnLocalClientConnect;

            this.networkedCommand = networkedCommand;

            if (addCommandArgToMessage == null || getCommandArgFromMessage == null)
            {
                this.addCommandArgToMessage = PrimativeSerializers.AddPrimativeToMessage;
                this.getCommandArgFromMessage = PrimativeSerializers.GetPrimativeFromMessage<T>;
            }
            else
            {
                this.addCommandArgToMessage = addCommandArgToMessage;
                this.getCommandArgFromMessage = getCommandArgFromMessage;
            }
        }

        ~NetworkedCommand()
        {
            NetworkManager.Singleton.OnLocalClientConnect -= OnLocalClientConnect;
        }

        public override void ExecuteCommandOnNetwork(object arg) 
        {
            if (!NetworkPermissionMatches()) ThrowNetworkPermissionExcpetion();

            bufferedCommandArgs.Add(arg);
        }

        public void ExecuteCommandForClient(ushort id, T arg) 
        {
            if (NetworkManager.Singleton.IsClient || networkWithAuthority == NetworkPermission.Client) ThrowNetworkPermissionExcpetion();
            
            if (bufferedCommandArgsPerClient.TryGetValue(id, out List<object> value))
            {
                value.Add(arg);
            }
            else
            {
                List<object> clientArgs = new List<object>();
                clientArgs.Add(arg);
                bufferedCommandArgsPerClient.Add(id, clientArgs);
            }
        }

        internal override void AddClientCommandArgsToMessage(ushort clientId, Message message)
        {
            AddIdToMessage(message);

            if (bufferedCommandArgsPerClient.TryGetValue(clientId, out List<object> commandArgs))
            {
                message.Add(commandArgs.Count);

                foreach (object commandArg in commandArgs)
                {
                    addCommandArgToMessage.Invoke(message, (T)commandArg);
                }
                
                commandArgs.Clear();
            }
            else
            {
                //zero meant to represent that there's no commands
                message.Add(0);
            }
        }

        internal override void AddCommandArgsToMessage(Message message)
        {
            AddIdToMessage(message);

            message.Add(bufferedCommandArgs.Count);

            foreach (object commandArg in bufferedCommandArgs)
            {
                addCommandArgToMessage.Invoke(message, (T)commandArg);
            }

            bufferedCommandArgs.Clear();
        }
        
        internal override void ExecuteCommand(Message message)
        {
            T arg = getCommandArgFromMessage.Invoke(message);

            networkedCommand.Invoke(arg);
        }

        private void ThrowNetworkPermissionExcpetion()
        {
            throw new Exception($"Attempted to execute command on network without having proper permission");
        }

        private void AddIdToMessage(Message message) 
        {
            IId iid = this;
            message.Add(iid.Id);
        }

        private void OnLocalClientConnect(ushort id)
        {
            clientIdWithAuthority = id;
        }
    }
}