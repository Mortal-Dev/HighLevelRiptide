using HLRiptide.NetworkedCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HLRiptide.Networks.NetworkSceneManagers
{
    public class ClientSceneManager
    {
        private readonly Action<ushort> localClientFinishConnectingAction;
        private readonly Action<AsyncOperation> localClientBeginLoadingSceneAction;
        private readonly Action<ushort> localClientFinishedLoadingSceneAction;

        private readonly InternalNetworkedCommand<int> syncClientSceneToServerScene;
        private readonly InternalNetworkedCommand<ushort> clientFinishedLoadingSceneCommand;

        private bool hasLoadedDefualtSceneOnce = false;

        public bool IsLocalClientLoadingScene { get; private set; }

        public ClientSceneManager(Action<ushort> localClientFinishConnecting, Action<AsyncOperation> localClientBeginLoadingScene, Action<ushort> localClientFinishedLoadingScene)
        {
            localClientFinishConnectingAction = localClientFinishConnecting;
            localClientBeginLoadingSceneAction = localClientBeginLoadingScene;
            localClientFinishedLoadingSceneAction = localClientFinishedLoadingScene;

            SceneManager.sceneLoaded += OnLocalClientSceneChange;

            syncClientSceneToServerScene = new InternalNetworkedCommand<int>((uint)InternalCommandId.SyncClientToServerScene, NetworkedCommandPriority.High, NetworkPermission.Server, LocalClientSyncSceneToServer); //
            clientFinishedLoadingSceneCommand = new InternalNetworkedCommand<ushort>((uint)InternalCommandId.ClientFinishedLoadingScene, NetworkedCommandPriority.High, NetworkPermission.Client, null); //
        }

        private void LocalClientSyncSceneToServer(int buildIndex)
        {
            if (NetworkManager.Singleton.serverSceneIndex == -1 || !hasLoadedDefualtSceneOnce) NetworkManager.Singleton.serverSceneIndex = buildIndex;

            IsLocalClientLoadingScene = true;

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(buildIndex);

            localClientBeginLoadingSceneAction?.Invoke(asyncOperation);
        }

        private void OnLocalClientSceneChange(Scene scene, LoadSceneMode loadSceneMode)
        {
            IsLocalClientLoadingScene = false;

            AwakeNetworkedBehaviours();

            localClientFinishedLoadingSceneAction?.Invoke(NetworkManager.Singleton.NetworkId);

            if (!hasLoadedDefualtSceneOnce)
            {
                localClientFinishConnectingAction?.Invoke(NetworkManager.Singleton.NetworkId);
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
