using HLRiptide.Util.ContainerUtil;
using RiptideNetworking;
using System;

namespace HLRiptide.NetworkedCommand
{
    public class InternalNetworkedCommand<T> : NetworkedCommand<T>
    {
        public InternalNetworkedCommand(uint overrideCommandId, NetworkedCommandPriority networkCommandPriority, NetworkPermission networkWithAuthority, Action<T> networkedCommand, Action<Message, T> addCommandArgToMessage = null, Func<Message, T> getCommandArgFromMessage = null) : base(networkWithAuthority, networkedCommand, addCommandArgToMessage, getCommandArgFromMessage) 
        {
            IId iid = this;

            //base() registers the networked command with a random id, so we override it
            NetworkManager.Singleton.NetworkedCommandContainer.OverrideId(iid.Id, overrideCommandId);

            networkedCommandPriority = networkCommandPriority;
        }
    }
}