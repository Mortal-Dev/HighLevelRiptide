using HLRiptide.Util.MessageUtil;
using RiptideNetworking;
using RiptideNetworking.Transports;
using System;
using System.Collections.Generic;

namespace HLRiptide.Networks
{
    public class ServerNetwork : Network
    {
        internal readonly Server Server;

        private static Dictionary<ushort, MessageBuffer> messageBuffers;

        Action OnTick;

        public ServerNetwork()
        {
            Server = new Server();

            Server.ClientConnected += OnClientConnected;
            Server.ClientDisconnected += OnClientDisconnect;

            messageBuffers = new Dictionary<ushort, MessageBuffer>();
        }

        public override void Start(INetworkStartInfo networkStartInfo)
        {
            ServerNetworkStartInfo serverNetworkStartInfo = (ServerNetworkStartInfo)networkStartInfo;
            
            clientJoinAction = serverNetworkStartInfo.OnServerClientBeginConnecting;
            clientLeaveAction = serverNetworkStartInfo.OnServerClientDisconnect;

            OnTick = serverNetworkStartInfo.OnTick;

            Server.Start(serverNetworkStartInfo.Port, serverNetworkStartInfo.MaxPlayerCount);

            networkSceneManager = new NetworkSceneManager(serverNetworkStartInfo.OnServerStart, serverNetworkStartInfo.OnServerClientFinishedConnecting, serverNetworkStartInfo.OnServerClientBeginLoadScene, serverNetworkStartInfo.OnServerClientFinishLoadScene);

            NetworkManager.Singleton.NetworkId = ushort.MaxValue;
        }

        public override void Stop()
        {
            Server.Stop();
        }

        public override void InternalTick()
        {
            Server.Tick();

            ExecuteMessagesFromMessageBuffers();

            OnTick?.Invoke();

            Server.SendToAll(messageGenerator.GenerateMessage(NetworkTick));

            SendIndividualClientMessages();
        }

        private void ExecuteMessagesFromMessageBuffers()
        {
            foreach (KeyValuePair<ushort, MessageBuffer> networkIdMessageBufferPair in messageBuffers)
            {
                (uint tick, Message message) = networkIdMessageBufferPair.Value.PopMessageFromBuffer();

                if (message == null) continue;

                messageHandler.HandleMessage(NetworkManager.Singleton.NetworkedCommandContainer, NetworkManager.Singleton.NetworkedObjectContainer, message, tick, networkIdMessageBufferPair.Key);
            }
        }

        private void OnClientConnected(object sender, ServerClientConnectedEventArgs clientConnectedEventArgs)
        {
            messageBuffers.Add(clientConnectedEventArgs.Client.Id, new MessageBuffer(3));
            networkSceneManager.OnServerClientConnect(clientConnectedEventArgs.Client.Id);

            clientJoinAction?.Invoke(clientConnectedEventArgs.Client.Id);
        }

        private void OnClientDisconnect(object sender, ClientDisconnectedEventArgs clientDisconnectedEventArgs)
        {
            messageBuffers.Remove(clientDisconnectedEventArgs.Id);

            clientLeaveAction?.Invoke(clientDisconnectedEventArgs.Id);
        }

        private void SendIndividualClientMessages()
        {
            foreach (IConnectionInfo connectionInfo in Server.Clients)
            {
                SendIndividualClientMessage(connectionInfo.Id);
            }
        }

        private void SendIndividualClientMessage(ushort clientId)
        {
            Server.Send(messageGenerator.GenerateMessageForServerClient(NetworkTick, clientId), clientId);
        }

        [MessageHandler((ushort)UniversalMessageId.message)]
        private static void HandleMessage(ushort fromClient, Message message)
        {
           // messageBuffers[fromClient].AddMessageToBuffer(message);

            messageHandler.HandleMessage(NetworkManager.Singleton.NetworkedCommandContainer, NetworkManager.Singleton.NetworkedObjectContainer, message, message.GetUInt(), fromClient);
        }
    }
}