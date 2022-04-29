using RiptideNetworking;
using System;

namespace HLRiptide.Networks
{
    class ClientNetwork : Network
    {
        private readonly Client Client;

        Action OnTick;

        public ClientNetwork()
        {
            Client = new Client();

            Client.Connected += OnLocalClientConnect;
            Client.Disconnected += OnLocalClientDisconnect;
        }

        public override void Start(INetworkStartInfo networkStartInfo)
        {
            ClientNetworkStartInfo clientNetworkStartInfo = (ClientNetworkStartInfo)networkStartInfo;

            OnTick = clientNetworkStartInfo.OnTick;

            clientJoinAction = clientNetworkStartInfo.OnLocalClientConnect;
            clientLeaveAction = clientNetworkStartInfo.OnLocalClientDisconnect;

            networkSceneManager = new NetworkSceneManager(null, null, null, null, clientNetworkStartInfo.OnLocalClientBeginLoadScene, clientNetworkStartInfo.OnLocalClientFinishLoadScene);

            Client.Connect($"{clientNetworkStartInfo.Ip}:{clientNetworkStartInfo.Port}");
        }

        public override void InternalTick()
        {
            Client.Tick();

            if (Client.IsConnected) 
            {
                OnTick?.Invoke();

                Client.Send(messageGenerator.GenerateMessage(NetworkTick));
            }
        }

        public override void Stop()
        {
            Client.Disconnect();
        }

        public void OnLocalClientConnect(object sender, EventArgs clientConnectedEventArgs)
        {
            NetworkManager.Singleton.NetworkId = Client.Id;

            clientJoinAction.Invoke(Client.Id);
        }

        public void OnLocalClientDisconnect(object sender, EventArgs e)
        {
            clientLeaveAction.Invoke(Client.Id);
        }

        [MessageHandler((ushort)UniversalMessageId.message)]
        private static void HandleMessage(Message message)
        {
            uint tick = message.GetUInt();

            messageHandler.HandleMessage(NetworkManager.Singleton.NetworkedCommandContainer, NetworkManager.Singleton.NetworkedObjectContainer, message, tick, ushort.MaxValue);
        }
    }
}