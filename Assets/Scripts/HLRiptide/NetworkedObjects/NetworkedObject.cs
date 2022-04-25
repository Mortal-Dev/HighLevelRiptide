using HLRiptide.NetworkedCommand;
using HLRiptide.Util.ContainerUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HLRiptide.NetworkedObjects
{
    public class NetworkedObject : MonoBehaviour, IId
    {
        uint IId.Id { get; set; }

        //display properties in inspector
        [SerializeField] private ushort networkId;
        [SerializeField] private bool isLocalPlayer;
        [SerializeField] private bool destroyObjectWhenClientDisconnect;
        [SerializeField] private bool hasSpawnedOnNetwork = false;
        [SerializeField] private NetworkPermission networkWithAuthority;

        internal int ObjectPrefabIndex { get; private set; }

        internal Vector3 pastPosition;
        internal Vector3 pastRotation;
        internal Vector3 pastScale;

        private bool sentDestroyCommand = false;

        public ushort NetworkId
        {
            get { return networkId; }
            internal set
            {
                networkId = value;
            }
        }

        public bool IsLocalPlayer
        {
            get { return isLocalPlayer; }
            internal set
            {
                isLocalPlayer = value;
            }
        }

        public NetworkPermission NetworkWithAuthority
        {
            get { return networkWithAuthority; }
            internal set
            {
                networkWithAuthority = value;
            }
        }

        public bool HasSpawnedOnNetwork 
        { 
            get { return hasSpawnedOnNetwork; } 
            private set
            {
                hasSpawnedOnNetwork = value;
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                DestroyOnNetwork();

                if (destroyObjectWhenClientDisconnect)
                {
                    NetworkManager.Singleton.OnServerClientDisconnect -= OnServerClientDisconnect;
                }
            }

            if (NetworkManager.Singleton.NetworkedCommandContainer.GetValue(this) != null) NetworkManager.Singleton.NetworkedCommandContainer.RemoveValue(this);
        }

        private void Awake()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                if (NetworkId != ushort.MaxValue)
                {
                    DestroyRigidbody();

                    DestroyColliders();
                }

                if (destroyObjectWhenClientDisconnect)
                {
                    NetworkManager.Singleton.OnServerClientDisconnect += OnServerClientDisconnect;
                }
            }
        }

        public void SpawnOnNetwork() => SpawnOnNetwork(ushort.MaxValue);

        public void SpawnOnNetwork(ushort clientIdWithAuthority)
        {
            if (HasSpawnedOnNetwork) return;

            if (NetworkManager.Singleton.IsClient) throw new Exception("Cannot spawn objects on Client!");

            if (NetworkManager.Singleton.IsServer)
            {
                if (NetworkId != ushort.MaxValue)
                {
                    DestroyRigidbody();

                    DestroyColliders();
                }
            }

            InitProperties(clientIdWithAuthority);

            NetworkManager.Singleton.NetworkedObjectContainer.RegisterValue(this);

            InternalCommands.spawnObjectOnNetworkCommand.ExecuteCommandOnNetwork(GetNetworkedObjectSpawnInfo());
        }

        public void DestroyOnNetwork()
        {
            SendDestroyCommand();

            NetworkManager.Singleton.NetworkedObjectContainer.RemoveValue(this);

            Destroy(gameObject);
        }

        internal void InitProperties(ushort networkId)
        {
            NetworkId = networkId;
            IsLocalPlayer = CheckIfLocalPlayer();
            ObjectPrefabIndex = GetObjectPrefabIndex();
            HasSpawnedOnNetwork = true;
            SetNetworkPermission();

            UpdatePastTransform();
        }

        internal void UpdatePastTransform()
        {
            pastPosition = transform.position;
            pastRotation = transform.rotation.eulerAngles;
            pastScale = transform.localScale;
        }

        internal void SetNetworkedObjectInfo(NetworkedObjectInfo networkedObjectInfo)
        {
            transform.localScale = networkedObjectInfo.scale;

            /*if (NetworkManager.Singleton.IsClient)
            {
                StartCoroutine(LerpPosition(networkedObjectInfo.position, Time.fixedDeltaTime));
                StartCoroutine(LerpRotation(networkedObjectInfo.rotation, Time.fixedDeltaTime));
            }
            else
            {*/
                transform.position = networkedObjectInfo.position;
                transform.rotation = Quaternion.Euler(networkedObjectInfo.rotation.x, networkedObjectInfo.rotation.y, networkedObjectInfo.rotation.z);
            //}
        }

        internal NetworkedObjectInfo GetNetworkedObjectInfo()
        {
            IId iid = this;

            return new NetworkedObjectInfo(iid.Id, transform.position, transform.rotation.eulerAngles, transform.localScale);
        }

        internal NetworkedObjectSpawnInfo GetNetworkedObjectSpawnInfo()
        {
            NetworkedObjectSpawnInfo networkedObjectSpawnInfo = new NetworkedObjectSpawnInfo(GetNetworkedObjectInfo(), networkId, ObjectPrefabIndex);

            var (commandHashCodes, commandIds) = GetCommandHashCodesAndIds(gameObject);

            networkedObjectSpawnInfo.commandHashCodes = commandHashCodes;
            networkedObjectSpawnInfo.overrideIds = commandIds;

            return networkedObjectSpawnInfo;
        }

        private void DestroyColliders()
        { 

            if (TryGetComponent(out Collider baseCollider))
            {
                Destroy(baseCollider);
            }

            Collider[] colliders = GetComponentsInChildren<Collider>();

            foreach (Collider collider in colliders) Destroy(collider);
        }

        private void DestroyRigidbody()
        {
            if (TryGetComponent(out Rigidbody baseRigidBody))
            {
                Destroy(baseRigidBody);
            }

            Rigidbody[] rigidbodys = GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rigidbody in rigidbodys) Destroy(rigidbody);
        }

        private void SendDestroyCommand()
        {
            if (sentDestroyCommand) return;

            sentDestroyCommand = true;

            IId iid = this;

            InternalCommands.destroyObjectOnNetworkCommand.ExecuteCommandOnNetwork(iid.Id);
        }

        private bool CheckIfLocalPlayer()
        {
            if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.NetworkId == networkId) return true;

            return false;
        }

        private void SetNetworkPermission()
        {
            if (NetworkId == ushort.MaxValue) NetworkWithAuthority = NetworkPermission.Server;
            else NetworkWithAuthority = NetworkPermission.Client;
        }

        private void OnServerClientDisconnect(ushort id)
        {
            if (id == networkId) DestroyOnNetwork();
        }

        private (List<int[]>, List<uint[]>) GetCommandHashCodesAndIds(GameObject go)
        {
            NetworkedBehaviour[] networkedBehaviours = go.GetComponents<NetworkedBehaviour>();

            List<int[]> commandHashCodes = new List<int[]>();
            List<uint[]> commandIds = new List<uint[]>();

            for (int i = 0; i < networkedBehaviours.Length; i++)
            {
                commandHashCodes.Add(networkedBehaviours[i].GetCommandHashCodes());
                commandIds.Add(networkedBehaviours[i].GetCommandIds());
            }

            return (commandHashCodes, commandIds);
        }

        private int GetObjectPrefabIndex()
        {
            NetworkedObject[] networkedObjects = NetworkManager.Singleton.networkedObjectPrefabs;

            for (int i = 0; i < networkedObjects.Length; i++)
            {
                if (gameObject.name.Contains(networkedObjects[i].gameObject.name)) return i;
            }

            throw new Exception($"could not find {gameObject.name} in networked object prefabs. Make sure {gameObject.name} has the NetworkedObject component attached to its root Game Object");
        }

        private IEnumerator LerpPosition(Vector3 newPosition, float duration)
        {
            float time = 0;
            Vector3 startPosition = transform.position;
            while (time < duration)
            {
                transform.position = Vector3.Lerp(startPosition, newPosition, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            transform.position = newPosition;
        }

        private IEnumerator LerpRotation(Vector3 newRotation, float duration)
        {
            float time = 0;
            Quaternion startRotation = transform.rotation;
            while (time < duration)
            {
                transform.rotation = Quaternion.Lerp(startRotation, Quaternion.Euler(newRotation.x, newRotation.y, newRotation.z), time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            transform.rotation = Quaternion.Euler(newRotation.x, newRotation.y, newRotation.z);
        }
    }
}