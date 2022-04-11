using HLRiptide.NetworkedCommand;
using HLRiptide.Util.ContainerUtil;
using HLRiptide.Util.MessageUtil;
using RiptideNetworking;
using RiptideNetworking.Transports;
using RiptideNetworking.Transports.RudpTransport;
using RiptideNetworking.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HLRiptide.Networks
{
    public class ServerNetwork : Network
    {
        internal const ushort SERVER_NETWORK_ID = ushort.MaxValue;

        internal readonly Server Server;

        Action OnTick;

        public ServerNetwork()
        {
            Server = new Server();

            Server.ClientConnected += OnClientConnected;
            Server.ClientDisconnected += OnClientDisconnect;
        }

        public override void Start(INetworkStartInfo networkStartInfo)
        {
            ServerNetworkStartInfo serverNetworkStartInfo = (ServerNetworkStartInfo)networkStartInfo;
            
            clientJoinAction = serverNetworkStartInfo.OnServerClientBeginConnecting;
            clientLeaveAction = serverNetworkStartInfo.OnServerClientDisconnect;

            OnTick = serverNetworkStartInfo.OnTick;

            Server.Start(serverNetworkStartInfo.Port, serverNetworkStartInfo.MaxPlayerCount);

            networkSceneManager = new NetworkSceneManager(serverNetworkStartInfo.OnServerStart, serverNetworkStartInfo.OnServerClientFinishedConnecting, serverNetworkStartInfo.OnServerClientBeginLoadScene, serverNetworkStartInfo.OnServerClientFinishLoadScene, null, null);

            NetworkManager.Singleton.NetworkId = ushort.MaxValue;
        }

        public override void Stop()
        {
            Server.Stop();
        }

        public override void InternalTick()
        {
            Server.Tick();

            OnTick?.Invoke();

            Server.SendToAll(messageGenerator.GenerateMessage(NetworkTick));

            SendIndividualClientMessages();
        }

        private void OnClientConnected(object sender, ServerClientConnectedEventArgs clientConnectedEventArgs)
        {
            networkSceneManager.OnServerClientConnect(clientConnectedEventArgs.Client.Id);

            clientJoinAction?.Invoke(clientConnectedEventArgs.Client.Id);
        }

        private void OnClientDisconnect(object sender, ClientDisconnectedEventArgs clientDisconnectedEventArgs)
        {
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
            messageHandler.HandleMessage(NetworkManager.Singleton.NetworkedCommandContainer, NetworkManager.Singleton.NetworkedObjectContainer, message, fromClient);
        }
    }
}