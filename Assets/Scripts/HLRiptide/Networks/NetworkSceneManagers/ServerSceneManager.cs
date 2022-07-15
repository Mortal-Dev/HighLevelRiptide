using HLRiptide.NetworkedCommand;
using HLRiptide.NetworkedObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace HLRiptide.Networks.NetworkSceneManagers
{
    public class ServerSceneManager : BaseNetworkSceneManager
    {
        private readonly Dictionary<ushort, bool> serverClientsLoadingScene;
        private readonly Dictionary<ushort, bool> serverClientHasLoadedDefaultScene;

        private readonly Action onServerStart;

        private readonly Action<ushort> serverClientFinishedLoadingSceneAction;
        private readonly Action<ushort> serverClientBeginLoadingSceneAction;

        private readonly Action<ushort> serverClientFinishedConnectingAction;

        private bool hasLoadedDefualtSceneOnce = false;

        public ServerSceneManager(Action onServerStart, Action<ushort> serverClientFinishedConnecting, Action<ushort> serverClientBeginLoadingScene, Action<ushort> serverClientFinishedLoadingScene)
        {
            this.onServerStart = onServerStart;

            serverClientBeginLoadingSceneAction = serverClientBeginLoadingScene;
            serverClientFinishedLoadingSceneAction = serverClientFinishedLoadingScene;
            serverClientFinishedConnectingAction = serverClientFinishedConnecting;

            SceneManager.sceneLoaded += OnServerSceneChange;

            serverClientsLoadingScene = new Dictionary<ushort, bool>();
            serverClientHasLoadedDefaultScene = new Dictionary<ushort, bool>();

            syncClientSceneToServerScene = new InternalNetworkedCommand<int>((uint)InternalCommandId.SyncClientToServerScene, NetworkedCommandPriority.High, NetworkPermission.Server, null);
            clientFinishedLoadingSceneCommand = new InternalNetworkedCommand<ushort>((uint)InternalCommandId.ClientFinishedLoadingScene, NetworkedCommandPriority.High, NetworkPermission.Client, ServerClientFinishedLoadingScene);

            NetworkManager.Singleton.OnServerClientDisconnect += OnServerClientDisconnect;
        }

        public bool IsServerClientLoadingScene(ushort id)
        {
            if (serverClientsLoadingScene.TryGetValue(id, out bool isServerClientLoadingScene))
            {
                return isServerClientLoadingScene;
            }

            return false;
        }

        private void ServerClientFinishedLoadingScene(ushort clientId)
        {
            serverClientsLoadingScene[clientId] = false;

            serverClientFinishedLoadingSceneAction?.Invoke(clientId);

            if (SceneManager.GetActiveScene().buildIndex == NetworkManager.Singleton.serverSceneIndex && !serverClientHasLoadedDefaultScene[clientId])
            {
                serverClientHasLoadedDefaultScene[clientId] = true;
                serverClientFinishedConnectingAction?.Invoke(clientId);
            }

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

        private void OnServerSceneChange(Scene scene, LoadSceneMode loadSceneMode)
        {
            AwakeNetworkedBehaviours();

            syncClientSceneToServerScene.ExecuteCommandOnNetwork(scene.buildIndex);

            foreach (ushort clientId in serverClientsLoadingScene.Keys)
            {
                serverClientBeginLoadingSceneAction?.Invoke(clientId);
                serverClientsLoadingScene[clientId] = true;
            }

            if (!hasLoadedDefualtSceneOnce && scene.buildIndex == NetworkManager.Singleton.serverSceneIndex)
            {
                onServerStart?.Invoke();
                hasLoadedDefualtSceneOnce = true;
            }
        }
    }
}
