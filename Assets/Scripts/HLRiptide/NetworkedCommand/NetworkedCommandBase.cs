using HLRiptide.Util.ContainerUtil;
using RiptideNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLRiptide.NetworkedCommand
{
    public class NetworkedCommandBase : IId
    {
        public NetworkPermission networkWithAuthority;

        internal NetworkedCommandPriority networkedCommandPriority;

        uint IId.Id { get; set; }

        internal readonly List<object> bufferedCommandArgs;

        internal readonly Dictionary<ushort, List<object>> bufferedCommandArgsPerClient;

        internal ushort clientIdWithAuthority;

        public NetworkedCommandBase(NetworkPermission networkWithAuthority, NetworkedCommandPriority networkedCommandPriority)
        {
            this.networkWithAuthority = networkWithAuthority;
            this.networkedCommandPriority = networkedCommandPriority;

            bufferedCommandArgs = new List<object>();
            bufferedCommandArgsPerClient = new Dictionary<ushort, List<object>>();

            if (networkWithAuthority == NetworkPermission.Server) clientIdWithAuthority = ushort.MaxValue;

            NetworkManager.Singleton.NetworkedCommandContainer.RegisterValue(this);
        }

        ~NetworkedCommandBase()
        {
            NetworkManager.Singleton.NetworkedCommandContainer.RemoveValue(this);
        }

        internal virtual void AddCommandArgsToMessage(Message message) { }

        internal virtual void AddClientCommandArgsToMessage(ushort clientId, Message message) { }

        internal virtual void ExecuteCommand(Message message) { }

        public virtual void ExecuteCommandOnNetwork(object arg) { }

        internal bool NetworkPermissionMatches()
        {
            return NetworkManager.Singleton.IsClient && networkWithAuthority == NetworkPermission.Client || NetworkManager.Singleton.IsServer && networkWithAuthority == NetworkPermission.Server;
        }
    }
}
