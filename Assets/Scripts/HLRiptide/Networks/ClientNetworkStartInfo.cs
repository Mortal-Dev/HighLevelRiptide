using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HLRiptide.Networks
{
    public class ClientNetworkStartInfo : INetworkStartInfo
    {
        public ushort Port { get; set; }
        public string Ip { get; }

        public Action<ushort> OnLocalClientConnect { get;  }
        public Action<ushort> OnLocalClientDisconnect { get;  }
        public Action<AsyncOperation> OnLocalClientBeginLoadScene { get; }
        public Action<ushort> OnLocalClientFinishLoadScene { get; }
        public Action OnTick { get; set; }

        public ClientNetworkStartInfo(Action<ushort> OnLocalClientConnect, Action<ushort> OnLocalClientDisconnect, Action<AsyncOperation> OnLocalClientBeginLoadScene, Action<ushort> OnLocalClientFinishLoadScene, Action OnTick, string Ip, ushort Port)
        {
            this.OnLocalClientConnect = OnLocalClientConnect;
            this.OnLocalClientDisconnect = OnLocalClientDisconnect;
            this.OnLocalClientBeginLoadScene = OnLocalClientBeginLoadScene;
            this.OnLocalClientFinishLoadScene = OnLocalClientFinishLoadScene;

            this.OnTick = OnTick;

            this.Ip = Ip;
            this.Port = Port;
        }
    }
}
