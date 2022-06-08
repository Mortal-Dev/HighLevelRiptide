using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HLRiptide.Networks
{
    public class HostNetworkStartInfo : INetworkStartInfo
    {
        public ushort Port { get; set; }

        public Action OnTick { get; set; }

        public ServerNetworkStartInfo ServerNetworkStartInfo { get; set; }
        public ClientNetworkStartInfo ClientNetworkStartInfo { get; set; }

        public HostNetworkStartInfo(ServerNetworkStartInfo serverNetworkStartInfo, ClientNetworkStartInfo clientNetworkStartInfo)
        {
            ServerNetworkStartInfo = serverNetworkStartInfo;
            ClientNetworkStartInfo = clientNetworkStartInfo;

            Port = serverNetworkStartInfo.Port;
            OnTick = serverNetworkStartInfo.OnTick;
        }
    }
}
