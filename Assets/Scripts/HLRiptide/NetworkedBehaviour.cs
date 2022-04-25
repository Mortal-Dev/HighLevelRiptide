using HLRiptide.NetworkedCommand;
using HLRiptide.Util.ContainerUtil;
using RiptideNetworking;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HLRiptide
{
    public abstract class NetworkedBehaviour : MonoBehaviour, IId
    {
        private NetworkedObjects.NetworkedObject networkedObject;

        public bool IsLocalPlayer => networkedObject.IsLocalPlayer;

        public bool IsServer => networkedObject.NetworkWithAuthority == NetworkPermission.Server;

        public bool IsClient => NetworkWithAuthority == NetworkPermission.Client;

        public NetworkPermission NetworkWithAuthority => networkedObject.NetworkWithAuthority;

        public ushort NetworkId => networkedObject.NetworkId;

        public bool IsLocalPlayerWithAuthority => IsClient && IsLocalPlayer;

        public bool HasAuthority => NetworkManager.Singleton.NetworkId == NetworkId;

        uint IId.Id { get; set; }

        internal readonly Dictionary<int, NetworkedCommandBase> networkedCommands = new Dictionary<int, NetworkedCommandBase>();

        private bool awoken = false;

        public virtual void OnRegisterCommands() { }

        private void OnDestroy()
        {
            if (!awoken) return;

            UnregisterCommands();

            NetworkManager.Singleton.OnTick -= OnTick;

            NetworkManager.Singleton.OnServerStart -= OnServerStart;
            NetworkManager.Singleton.OnServerClientBeginLoadScene -= OnServerClientStartLoadScene;
            NetworkManager.Singleton.OnServerClientFinishLoadScene -= OnServerClientFinishedLoadScene;
            NetworkManager.Singleton.OnServerClientFinishConnected -= OnServerClientFinishConnecting;
            NetworkManager.Singleton.OnServerClientBeginConnected -= OnServerClientStartConnecting;
        }

        private void Awake()
        {
            if (awoken) return;

            if (NetworkManager.Singleton == null) return;

            InternalAwake();
        }

        internal void InternalAwake()
        {
            if (awoken) return;

            awoken = true;

            NetworkManager.Singleton.OnTick += OnTick;

            NetworkManager.Singleton.OnServerStart += OnServerStart;
            NetworkManager.Singleton.OnServerClientBeginLoadScene += OnServerClientStartLoadScene;
            NetworkManager.Singleton.OnServerClientFinishLoadScene += OnServerClientFinishedLoadScene;
            NetworkManager.Singleton.OnServerClientBeginConnected += OnServerClientStartConnecting;
            NetworkManager.Singleton.OnServerClientFinishConnected += OnServerClientFinishConnecting;

            if (transform.root.gameObject.TryGetComponent(out NetworkedObjects.NetworkedObject networkedObject))
                this.networkedObject = networkedObject;
            
            OnRegisterCommands();

            OnAwake();
        }

        private void Start()
        {
            OnStart();
        }

        public void RegisterNetworkedCommand<T>(Action<T> command, NetworkPermission networkWithAuthority) where T : unmanaged
        {
            if (networkedObject == null) ThrowNoNetworkedObjectException();

            NetworkedCommand<T> networkedCommand = new NetworkedCommand<T>(networkWithAuthority, command);

            networkedCommands[command.Method.Name.GetHashCode()] = networkedCommand;
        }

        public void RegisterCustomArgNetworkedCommand<T>(NetworkPermission networkWithAuthority, Action<T> command, Action<Message, T> addCommandArgToMessage, Func<Message, T> getCommandArgFromMessage)
        {
            if (networkedObject == null) ThrowNoNetworkedObjectException();

            NetworkedCommand<T> networkedCommand = new NetworkedCommand<T>(networkWithAuthority, command, addCommandArgToMessage, getCommandArgFromMessage);

            networkedCommands[command.Method.Name.GetHashCode()] = networkedCommand;
        }

        public void ExecuteCommandOnNetwork<T>(Action<T> command, T arg)
        {
            if (networkedObject == null) ThrowNoNetworkedObjectException();

            if (networkedCommands.TryGetValue(command.Method.Name.GetHashCode(), out NetworkedCommandBase networkedCommandBase))
            {
                networkedCommandBase.ExecuteCommandOnNetwork(arg);
            }
        }

        public virtual void OnAwake() { }

        public virtual void OnStart() { }

        public virtual void OnTick() { }

        public virtual void OnServerStart() { }

        public virtual void OnServerClientStartLoadScene(ushort id) { }

        public virtual void OnServerClientFinishedLoadScene(ushort id) { }

        public virtual void OnServerClientStartConnecting(ushort id) { }

        public virtual void OnServerClientFinishConnecting(ushort id) { }

        public virtual void OnLocalClientConnected(ushort id) { }

        internal uint[] GetCommandIds()
        {
            List<uint> commandIds = new List<uint>();

            foreach (NetworkedCommandBase networkedCommandBase in networkedCommands.Values)
            {
                IId iid = networkedCommandBase;
                commandIds.Add(iid.Id);
            }

            return commandIds.ToArray();
        }

        internal int[] GetCommandHashCodes()
        {
            List<int> commandHashs = new List<int>();

            foreach (int hashCode in networkedCommands.Keys)
            {
                commandHashs.Add(hashCode);
            }

            return commandHashs.ToArray();
        }

        internal void OverrideCommandIds(int[] commandHashCodes, uint[] newIds)
        {
            for (int i = 0; i < newIds.Length; i++)
            {
                IId networkedCommandId = NetworkManager.Singleton.NetworkedCommandContainer.GetValue(networkedCommands[commandHashCodes[i]]);

                NetworkManager.Singleton.NetworkedCommandContainer.OverrideId(networkedCommandId.Id, newIds[i]);
            }
        }

        private void UnregisterCommands()
        {
            foreach (NetworkedCommandBase networkedCommandBase in networkedCommands.Values)
            {
                NetworkManager.Singleton.NetworkedCommandContainer.RemoveValue(networkedCommandBase);
            }

            networkedCommands.Clear();
        }

        

        private void ThrowNoNetworkedObjectException() => throw new Exception($"Error no networked object on base of Game Object");
    }
}