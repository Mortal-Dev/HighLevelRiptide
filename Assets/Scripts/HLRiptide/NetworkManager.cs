using HLRiptide.NetworkedCommand;
using HLRiptide.NetworkedObjects;
using HLRiptide.Networks;
using HLRiptide.Util.ContainerUtil;
using HLRiptide.Util.MessageUtil;
using RiptideNetworking.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HLRiptide
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Singleton { get; private set; }

        public bool IsClient { get; private set; }

        public bool IsServer { get; private set; }

        public ushort NetworkId { get; internal set; }

        public float StepTime { get; private set; }

        //events
        public delegate void OnServerClientBeginConnectAction(ushort id);
        public event OnServerClientBeginConnectAction OnServerClientBeginConnected;

        public delegate void OnServerClientFinishConnectAction(ushort id);
        public event OnServerClientFinishConnectAction OnServerClientFinishConnected;

        public delegate void OnServerClientDisconnectAction(ushort id);
        public event OnServerClientDisconnectAction OnServerClientDisconnect;

        public delegate void OnServerClientBeginLoadSceneAction(ushort id);
        public event OnServerClientBeginLoadSceneAction OnServerClientBeginLoadScene;

        public delegate void OnServerClientFinishLoadSceneAction(ushort id);
        public event OnServerClientFinishLoadSceneAction OnServerClientFinishLoadScene;

        public delegate void OnServerStartAction();
        public event OnServerStartAction OnServerStart;

        public delegate void OnLocalClientConnectAction(ushort id);
        public event OnLocalClientConnectAction OnLocalClientConnect;

        public delegate void OnLocalClientDisconnectAction();
        public event OnLocalClientDisconnectAction OnLocalClientDisconnect;

        public delegate void OnLocalClientBeginSceneLoadAction(AsyncOperation asyncOperation);
        public event OnLocalClientBeginSceneLoadAction OnLocalClientBeginSceneLoad;

        public delegate void OnLocalClientFinishSceneLoadAction(ushort id);
        public event OnLocalClientFinishSceneLoadAction OnLocalClientFinishSceneLoad;

        public delegate void OnTickAction();
        public event OnTickAction OnTick;

        internal Container<NetworkedObjects.NetworkedObject> NetworkedObjectContainer { get; private set; }

        internal Container<NetworkedCommandBase> NetworkedCommandContainer { get; private set; }

        internal Networks.Network Network { get; private set; }

        //unity inspector fields
        [Header("Prefabs that can be used across the network")]
        [SerializeField] internal NetworkedObjects.NetworkedObject[] networkedObjectPrefabs;

        [Header("Network Settings")]
        [SerializeField] private bool runOnFixedUpdate;
        [SerializeField] private float updateRate;

        [Header("Start Server Instantly Settings")]
        [SerializeField] private bool startServerInstantly;
        [SerializeField] private ushort defaultPort;
        [SerializeField] private ushort maxPlayerCount;
        [SerializeField] internal int defaultSceneIndex;

        private float sumDeltaTime = 0f;

        private void Awake()
        {
            if (Singleton == null) Singleton = this;
            else
            {
                DestroyImmediate(gameObject);
                return;
            }

            NetworkedObjectContainer = new Container<NetworkedObjects.NetworkedObject>();
            NetworkedCommandContainer = new Container<NetworkedCommandBase>();

            InternalCommands.Init();

            RiptideLogger.Initialize(Debug.Log, true);

            DontDestroyOnLoad(this);

            if (runOnFixedUpdate) StepTime = Time.fixedDeltaTime;
            else StepTime = updateRate;

            if (startServerInstantly) StartServer(defaultPort, maxPlayerCount);
        }
        
        public void StartServer(ushort port, ushort maxPlayer)
        {
            IsClient = false;
            IsServer = true;

            Application.targetFrameRate = (int)(1f / Time.fixedDeltaTime);

            Network = new ServerNetwork();

            Network.Start(new ServerNetworkStartInfo(() => OnServerStart?.Invoke(), (ushort id) => OnServerClientBeginConnected?.Invoke(id), (ushort id) => OnServerClientFinishConnected?.Invoke(id),
                (ushort id) => OnServerClientDisconnect?.Invoke(id), (ushort id) => OnServerClientBeginLoadScene?.Invoke(id), (ushort id) => OnServerClientFinishLoadScene?.Invoke(id), 
                () => OnTick?.Invoke(), defaultSceneIndex, port, maxPlayer));

            SceneManager.LoadScene(defaultSceneIndex);
        }

        public void StartClient(string ip, ushort port)
        {
            IsClient = true;
            IsServer = false;

            Network = new ClientNetwork();
            Network.Start(new ClientNetworkStartInfo((ushort id) => OnLocalClientConnect?.Invoke(id), (ushort id) => OnLocalClientDisconnect?.Invoke(),
                (AsyncOperation asyncOperation) => OnLocalClientBeginSceneLoad?.Invoke(asyncOperation), (ushort id) => OnLocalClientFinishSceneLoad?.Invoke(id), () => OnTick?.Invoke(), 
                ip, port));
        }

        private void FixedUpdate()
        {
            if (runOnFixedUpdate && Network != null)
            {
                Network.Tick();
            }
        }

        private void Update()
        {
            if (Network == null || runOnFixedUpdate) return;

            while (sumDeltaTime >= updateRate)
            {
                Network.Tick();
                sumDeltaTime -= updateRate;
            }

            sumDeltaTime += Time.deltaTime;
        }

        private void OnApplicationQuit()
        {
            if (Network != null) Network.Stop();
        }
    }
}