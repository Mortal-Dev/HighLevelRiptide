using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLRiptide.Networks
{
    public class ServerNetworkStartInfo : INetworkStartInfo
    {
        public ushort Port { get; set;  }
        public int DefautSceneBuildIndex { get; }
        public ushort MaxPlayerCount { get; set; }

        public Action OnServerStart { get; }
        public Action<ushort> OnServerClientBeginConnecting { get; }
        public Action<ushort> OnServerClientFinishedConnecting { get; }
        public Action<ushort> OnServerClientDisconnect { get; }
        public Action<ushort> OnServerClientBeginLoadScene { get; }
        public Action<ushort> OnServerClientFinishLoadScene { get; }
        public Action OnTick { get; set; }

        public ServerNetworkStartInfo(Action onServerStart, Action<ushort> onServerBeginClientConnected, Action<ushort> onServerClientFinishedConnecting, Action<ushort> onServerClientDisconnect, Action<ushort> onServerClientBeginLoadScene, Action<ushort> onServerClientFinishLoadScene, Action OnTick , int defaultSceneBuildIndex, ushort defaultPort, ushort maxPlayerCount)
        {
            OnServerStart = onServerStart;

            OnServerClientBeginConnecting = onServerBeginClientConnected;
            OnServerClientFinishedConnecting = onServerClientFinishedConnecting;
            OnServerClientDisconnect = onServerClientDisconnect;
            OnServerClientBeginLoadScene = onServerClientBeginLoadScene;
            OnServerClientFinishLoadScene = onServerClientFinishLoadScene;

            this.OnTick = OnTick;

            DefautSceneBuildIndex = defaultSceneBuildIndex;
            Port = defaultPort;
            MaxPlayerCount = maxPlayerCount;
        }
    }
}
