using HLRiptide.NetworkedObjects;
using HLRiptide.Util.ContainerUtil;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HLRiptide.NetworkedCommand
{
    //TODO: give this a normal class similar to ClientSceneManager
    public static class InternalCommands
    {
        public static InternalNetworkedCommand<NetworkedObjectSpawnInfo> spawnObjectOnNetworkCommand;

        public static InternalNetworkedCommand<uint> destroyObjectOnNetworkCommand;

        internal static void Init()
        {
            spawnObjectOnNetworkCommand = new InternalNetworkedCommand<NetworkedObjectSpawnInfo>((uint)InternalCommandId.SpawnObject, NetworkedCommandPriority.High, NetworkPermission.Server, SpawnObjectOnClient, NetworkedObjectSpawnInfo.AddCommandArgToMessage, NetworkedObjectSpawnInfo.GetCommandArgFromMessage); //
            destroyObjectOnNetworkCommand = new InternalNetworkedCommand<uint>((uint)InternalCommandId.DestroyObject, NetworkedCommandPriority.Medium, NetworkPermission.Server, DestroyObjectOnClient); //
        }

        internal static void SpawnObjectOnClient(NetworkedObjectSpawnInfo networkedObjectInfo)
        {
            //object has already spawned, don't respawn it
            if (NetworkManager.Singleton.NetworkedObjectContainer.GetValue(networkedObjectInfo.objectInfo.id) != null) return;

            //don't spawn anything if we're loading a scene
            if (NetworkManager.Singleton.Network.networkSceneManager.IsLocalClientLoadingScene) return;

            GameObject go = Object.Instantiate(NetworkManager.Singleton.networkedObjectPrefabs[networkedObjectInfo.objectPrefabIndex].gameObject);

            NetworkedObject networkedObject = go.GetComponent<NetworkedObject>();

            NetworkedBehaviour[] networkedBehaviours = go.GetComponents<NetworkedBehaviour>();

            networkedObject.InitProperties(networkedObjectInfo.clientIdWithAuthority);

            networkedObject.SetNetworkedObjectInfo(networkedObjectInfo.objectInfo);

            NetworkManager.Singleton.NetworkedObjectContainer.RegisterValue(networkedObject, networkedObjectInfo.objectInfo.id);

            SetCommandIdsInNetworkedBehaviours(networkedBehaviours, networkedObjectInfo.commandHashCodes, networkedObjectInfo.overrideIds);
        }

        internal static void DestroyObjectOnClient(uint id)
        {
            GameObject go = NetworkManager.Singleton.NetworkedObjectContainer.GetValue(id).gameObject;

            Object.Destroy(go);

            NetworkManager.Singleton.NetworkedObjectContainer.RemoveValue(id);
        }

        internal static void SetCommandIdsInNetworkedBehaviours(NetworkedBehaviour[] networkedBehaviours, List<int[]> commandHashCodes, List<uint[]> overrideIds)
        {
            //iterate through all networked behaviours and override the command ids
            for (int i = 0; i < networkedBehaviours.Length; i++)
            {
                networkedBehaviours[i].OverrideCommandIds(commandHashCodes[i], overrideIds[i]);
            }
        }
    }
}