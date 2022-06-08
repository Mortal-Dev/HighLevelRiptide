using HLRiptide.Networks;
using RiptideNetworking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HLRiptide.Networks
{
    public class HostNetwork : Network
    {
        private readonly ClientNetwork clientNetwork;
        private readonly ServerNetwork serverNetwork;

        private HostNetworkStartInfo hostNetworkStartInfo;

        public HostNetwork()
        {
            clientNetwork = new ClientNetwork();
            serverNetwork = new ServerNetwork();
        }

        public override void InternalTick()
        {
            clientNetwork.InternalTick();
            serverNetwork.InternalTick();
        }

        public override void Start(INetworkStartInfo networkStartInfo)
        {
            hostNetworkStartInfo = (HostNetworkStartInfo)networkStartInfo;

            serverNetwork.Start(hostNetworkStartInfo.ServerNetworkStartInfo);
            clientNetwork.Start(hostNetworkStartInfo.ClientNetworkStartInfo);
        }

        public override void Stop()
        {
            clientNetwork.Stop();
        }
    }
}