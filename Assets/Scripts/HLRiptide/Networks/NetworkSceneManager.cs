using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using HLRiptide.NetworkedCommand;
using HLRiptide.NetworkedObjects;

namespace HLRiptide.Networks
{
    public class NetworkSceneManager
    {
        private readonly InternalNetworkedCommand<int> syncClientSceneToServerScene;
        private readonly InternalNetworkedCommand<ushort> clientFinishedLoadingSceneCommand;

        private readonly Dictionary<ushort, bool> serverClientsLoadingScene;
        private readonly Dictionary<ushort, bool> serverClientHasLoadedDefaultScene;

        private readonly Action onServerStart;

        private readonly Action<ushort> serverClientFinishedLoadingSceneAction;
        private readonly Action<ushort> serverClientBeginLoadingSceneAction;

        private readonly Action<ushort> serverClientFinishedConnectingAction;

        private readonly Action<AsyncOperation> localClientBeginLoadingSceneAction;
        private readonly Action<ushort> localClientFinishedLoadingSceneAction;

        private bool hasLoadedDefualtSceneOnce = false;

        public NetworkSceneManager(Action onServerStart, Action<ushort> serverClientFinishedConnecting, Action<ushort> serverClientBeginLoadingScene, Action<ushort> serverClientFinishedLoadingScene, Action<AsyncOperation> localClientBeginLoadingScene, Action<ushort> localClientFinishedLoadingScene)
        {
            this.onServerStart = onServerStart;

            serverClientBeginLoadingSceneAction = serverClientBeginLoadingScene;
            serverClientFinishedLoadingSceneAction = serverClientFinishedLoadingScene;
            localClientBeginLoadingSceneAction = localClientBeginLoadingScene;
            localClientFinishedLoadingSceneAction = localClientFinishedLoadingScene;
            serverClientFinishedConnectingAction = serverClientFinishedConnecting;

            NetworkManager.Singleton.OnServerClientDisconnect += OnServerClientDisconnect;
           // NetworkManager.Singleton.OnServerClientBeginConnected += OnServerClientConnect;

            if (NetworkManager.Singleton.IsClient) SceneManager.sceneLoaded += OnLocalClientSceneChange;
            else SceneManager.sceneLoaded += OnServerSceneChange;

            syncClientSceneToServerScene = new InternalNetworkedCommand<int>((uint)InternalCommandId.SyncClientToServerScene, NetworkedCommandPriority.High, NetworkPermission.Server, ClientSyncSceneToServer); //
            clientFinishedLoadingSceneCommand = new InternalNetworkedCommand<ushort>((uint)InternalCommandId.ClientFinishedLoadingScene, NetworkedCommandPriority.High, NetworkPermission.Client, ServerClientFinishedLoadingScene); //

            serverClientsLoadingScene = new Dictionary<ushort, bool>();
            serverClientHasLoadedDefaultScene = new Dictionary<ushort, bool>();
        }

        public bool IsLocalClientLoadingScene { get; private set; }

        public bool IsServerClientLoadingScene(ushort id)
        {
            if (serverClientsLoadingScene.TryGetValue(id, out bool value))
            {
                return value;
            }

            return false;
        }

        private void ClientSyncSceneToServer(int buildIndex)
        {
            IsLocalClientLoadingScene = true;

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(buildIndex);

            localClientBeginLoadingSceneAction?.Invoke(asyncOperation);
        }

        private void ServerClientFinishedLoadingScene(ushort clientId)
        {
            serverClientsLoadingScene[clientId] = false;

            if (SceneManager.GetActiveScene().buildIndex == NetworkManager.Singleton.defaultSceneIndex && !serverClientHasLoadedDefaultScene[clientId])
            {
                serverClientHasLoadedDefaultScene[clientId] = true;
                serverClientFinishedConnectingAction?.Invoke(clientId);
            }
            
            serverClientFinishedLoadingSceneAction?.Invoke(clientId);

            foreach (NetworkedObject networkedObject in NetworkManager.Singleton.NetworkedObjectContainer.ContainerDict.Values)
            {
                InternalCommands.spawnObjectOnNetworkCommand.ExecuteCommandForClient(clientId, networkedObject.GetNetworkedObjectSpawnInfo());
            }
        }

        public void OnServerClientConnect(ushort clientId)
        {
            syncClientSceneToServerScene.ExecuteCommandForClient(clientId, SceneManager.GetActiveScene().buildIndex);

            serverClientsLoadingScene.Add(clientId, true);
            serverClientHasLoadedDefaultScene.Add(clientId, false);

            serverClientBeginLoadingSceneAction?.Invoke(clientId);
        }

        private void OnServerClientDisconnect(ushort clientId)
        {
            serverClientsLoadingScene.Remove(clientId);
            serverClientHasLoadedDefaultScene.Remove(clientId);
        }

        private void OnLocalClientSceneChange(Scene scene, LoadSceneMode loadSceneMode)
        {
            AwakeNetworkedBehaviours();

            IsLocalClientLoadingScene = false;

            clientFinishedLoadingSceneCommand.ExecuteCommandOnNetwork(NetworkManager.Singleton.NetworkId);

            localClientFinishedLoadingSceneAction?.Invoke(NetworkManager.Singleton.NetworkId);
        }

        private void OnServerSceneChange(Scene scene, LoadSceneMode loadSceneMode)
        {
            AwakeNetworkedBehaviours();

            syncClientSceneToServerScene.ExecuteCommandOnNetwork(scene.buildIndex);

            foreach (ushort clientId in serverClientsLoadingScene.Keys)
            {
                serverClientBeginLoadingSceneAction?.Invoke(clientId);
                serverClientsLoadingScene[clientId] = true;
            }

            if (!hasLoadedDefualtSceneOnce)
            {
                onServerStart?.Invoke();
                hasLoadedDefualtSceneOnce = true;
            }
        }

        private void AwakeNetworkedBehaviours()
        {
            NetworkedBehaviour[] networkedBehaviours = UnityEngine.Object.FindObjectsOfType(typeof(NetworkedBehaviour)) as NetworkedBehaviour[];

            foreach (NetworkedBehaviour networkedBehaviour in networkedBehaviours) networkedBehaviour.InternalAwake();
        }
    }
}
