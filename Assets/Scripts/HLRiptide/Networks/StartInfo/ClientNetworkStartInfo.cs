using System;
using UnityEngine;

namespace HLRiptide.Networks
{
    public class ClientNetworkStartInfo : INetworkStartInfo
    {
        public ushort Port { get; set; }
        public string Ip { get; }

        public Action<ushort> OnLocalClientBeginConnect { get;  }
        public Action<ushort> OnLocalClientFinishConnect { get;  }
        public Action<ushort> OnLocalClientDisconnect { get;  }
        public Action<AsyncOperation> OnLocalClientBeginLoadScene { get; }
        public Action<ushort> OnLocalClientFinishLoadScene { get; }
        public Action OnTick { get; set; }

        public ClientNetworkStartInfo(Action<ushort> OnLocalClientBeginConnect, Action<ushort> OnLocalClientFinishConnect, Action<ushort> OnLocalClientDisconnect, Action<AsyncOperation> OnLocalClientBeginLoadScene, Action<ushort> OnLocalClientFinishLoadScene, Action OnTick, string Ip, ushort Port)
        {
            this.OnLocalClientBeginConnect = OnLocalClientBeginConnect;
            this.OnLocalClientFinishConnect = OnLocalClientFinishConnect;
            this.OnLocalClientDisconnect = OnLocalClientDisconnect;
            this.OnLocalClientBeginLoadScene = OnLocalClientBeginLoadScene;
            this.OnLocalClientFinishLoadScene = OnLocalClientFinishLoadScene;

            this.OnTick = OnTick;

            this.Ip = Ip;
            this.Port = Port;
        }
    }
}
