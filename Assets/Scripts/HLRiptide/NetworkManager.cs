using HLRiptide.NetworkedCommand;
using HLRiptide.NetworkedObjects;
using HLRiptide.Networks;
using HLRiptide.Util.ContainerUtil;
using RiptideNetworking.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

using Network = HLRiptide.Networks.Network;

namespace HLRiptide
{
    /// <summary>
    /// A singleton that holds the required logic to run RiptideNetworking
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        private enum NetworkType
        {
            Server,
            Client,
            Host
        }

        /// <summary>
        /// A singleton that holds the required logic to run RiptideNetworking
        /// </summary>
        public static NetworkManager Singleton { get; private set; }

        /// <summary>
        /// whether the local network is a client
        /// </summary>
        public bool IsClient { get; private set; }

        /// <summary>
        /// whether the local network is a server
        /// </summary>
        public bool IsServer { get; private set; }

        /// <summary>
        /// whether the local network is a host
        /// </summary>
        public bool IsHost { get; private set; }

        /// <summary>
        /// the id of the local network
        /// </summary>
        public ushort NetworkId { get; internal set; }

        /// <summary>
        /// the interval in seconds in which the network is updated
        /// </summary>
        public float StepTime { get; private set; }

        /// <summary>
        /// an event that fires when a client connects to the server
        /// </summary>
        /// <param name="id">the network id of the ServerClient that has just connected to the server</param>
        public delegate void OnServerClientBeginConnectAction(ushort id);
        public event OnServerClientBeginConnectAction OnServerClientBeginConnected;

        /// <summary>
        /// an event that fires when a client has finished connecting to the server
        /// </summary>
        /// <param name="id">the network id of the client that has finished connecting to the server</param>
        public delegate void OnServerClientFinishConnectAction(ushort id);
        public event OnServerClientFinishConnectAction OnServerClientFinishConnected;

        /// <summary>
        /// an event that fires when a client disconnects from the server
        /// </summary>
        /// <param name="id">the network id of the client that has just disconnected from the server</param>
        public delegate void OnServerClientDisconnectAction(ushort id);
        public event OnServerClientDisconnectAction OnServerClientDisconnect;


        /// <summary>
        /// an event that fires when the client begins loading the server' scene
        /// </summary>
        /// <param name="id">the network id of the client that has begun loading the server' scene</param>
        public delegate void OnServerClientBeginLoadSceneAction(ushort id);
        public event OnServerClientBeginLoadSceneAction OnServerClientBeginLoadScene;

        /// <summary>
        /// an event that fires when the client has finished loading the server' scene
        /// </summary>
        /// <param name="id">the network id of the client that has finished loading the server' scene</param>
        public delegate void OnServerClientFinishLoadSceneAction(ushort id);
        public event OnServerClientFinishLoadSceneAction OnServerClientFinishLoadScene;

        /// <summary>
        /// an event that fires when the server starts
        /// </summary>
        public delegate void OnServerStartAction();
        public event OnServerStartAction OnServerStart;

        /// <summary>
        /// an event that fires when the initial UDP connection between the client and server is made
        /// </summary>
        /// <param name="id">the network id of the local client</param>
        public delegate void OnLocalClientBeginConnectAction(ushort id);
        public event OnLocalClientBeginConnectAction OnLocalClientBeginConnect;

        /// <summary>
        /// an event that fires when the local client has finished loading the server' scene after the initial connection
        /// </summary>
        /// <param name="id">the network id of the local client</param>
        public delegate void OnLocalClientConnectAction(ushort id);
        public event OnLocalClientConnectAction OnLocalClientFinishConnect;

        /// <summary>
        /// an event that fires when the local client disconnects
        /// </summary>
        public delegate void OnLocalClientDisconnectAction();
        public event OnLocalClientDisconnectAction OnLocalClientDisconnect;

        /// <summary>
        /// an event that fires when the local client begins loading the server' scene
        /// </summary>
        /// <param name="asyncOperation">the async operation returned from SceneManager.LoadSceneAsync</param>
        public delegate void OnLocalClientBeginSceneLoadAction(AsyncOperation asyncOperation);
        public event OnLocalClientBeginSceneLoadAction OnLocalClientBeginSceneLoad;

        /// <summary>
        /// an event that fires when the local client finishes loading the server' scene
        /// </summary>
        /// <param name="id">the network id of the local client</param>
        public delegate void OnLocalClientFinishSceneLoadAction(ushort id);
        public event OnLocalClientFinishSceneLoadAction OnLocalClientFinishSceneLoad;

        /// <summary>
        /// an event that fires when the network ticks
        /// </summary>
        public delegate void OnTickAction();
        public event OnTickAction OnTick;

        internal Container<NetworkedObject> NetworkedObjectContainer { get; private set; }

        internal Container<NetworkedCommandBase> NetworkedCommandContainer { get; private set; }

        internal Network Network { get; private set; }

        //unity inspector fields

        [Header("Prefabs that can be used across the network")]
        [SerializeField] internal NetworkedObject[] networkedObjectPrefabs;

        [Header("Network Settings")]
        [SerializeField] private bool runOnFixedUpdate;
        [SerializeField] private float updateRate;

        internal int serverSceneIndex = -1;

        private float sumDeltaTime = 0f;

        private void Awake()
        {
            if (Singleton == null) Singleton = this;
            else
            {
                DestroyImmediate(gameObject);
                return;
            }

            NetworkedObjectContainer = new Container<NetworkedObject>();
            NetworkedCommandContainer = new Container<NetworkedCommandBase>();

            InternalCommands.Init();

            RiptideLogger.Initialize(Debug.Log, true);

            DontDestroyOnLoad(this);

            if (runOnFixedUpdate) StepTime = Time.fixedDeltaTime;
            else StepTime = updateRate;
        }

        public void StartHost(ushort port, ushort maxPlayer, int defaultSceneIndex)
        {
            IsClient = true;
            IsServer = true;
            IsHost = true;

            Network = new HostNetwork();

            serverSceneIndex = defaultSceneIndex;

            Network.Start(new HostNetworkStartInfo(new ServerNetworkStartInfo(() => OnServerStart?.Invoke(), (ushort id) => OnServerClientBeginConnected?.Invoke(id), (ushort id) => OnServerClientFinishConnected?.Invoke(id),
                (ushort id) => OnServerClientDisconnect?.Invoke(id), (ushort id) => OnServerClientBeginLoadScene?.Invoke(id), (ushort id) => OnServerClientFinishLoadScene?.Invoke(id),
                () => OnTick?.Invoke(), defaultSceneIndex, port, maxPlayer), new ClientNetworkStartInfo((ushort id) => OnLocalClientBeginConnect?.Invoke(id), (ushort id) => OnLocalClientFinishConnect?.Invoke(id), (ushort id) => OnLocalClientDisconnect?.Invoke(),
                (AsyncOperation asyncOperation) => OnLocalClientBeginSceneLoad?.Invoke(asyncOperation), (ushort id) => OnLocalClientFinishSceneLoad?.Invoke(id), () => OnTick?.Invoke(), "127.0.0.1", port)));

            SceneManager.LoadScene(defaultSceneIndex);
        }
        
        public void StartServer(ushort port, ushort maxPlayer, int defaultSceneIndex)
        {
            IsServer = true;

            Application.targetFrameRate = (int)(1f / Time.fixedDeltaTime);

            serverSceneIndex = defaultSceneIndex;

            Network = new ServerNetwork();

            Network.Start(new ServerNetworkStartInfo(() => OnServerStart?.Invoke(), (ushort id) => OnServerClientBeginConnected?.Invoke(id), (ushort id) => OnServerClientFinishConnected?.Invoke(id),
                (ushort id) => OnServerClientDisconnect?.Invoke(id), (ushort id) => OnServerClientBeginLoadScene?.Invoke(id), (ushort id) => OnServerClientFinishLoadScene?.Invoke(id), 
                () => OnTick?.Invoke(), defaultSceneIndex, port, maxPlayer));

            SceneManager.LoadScene(defaultSceneIndex);
        }

        public void StartClient(string ip, ushort port)
        {
            IsClient = true;

            Network = new ClientNetwork();
            Network.Start(new ClientNetworkStartInfo((ushort id) => OnLocalClientBeginConnect?.Invoke(id), (ushort id) => OnLocalClientFinishConnect?.Invoke(id), (ushort id) => OnLocalClientDisconnect?.Invoke(),
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

            while (sumDeltaTime >= StepTime)
            {
                Network.Tick();
                sumDeltaTime -= StepTime;
            }

            sumDeltaTime += Time.deltaTime;
        }

        private void OnApplicationQuit()
        {
            Network?.Stop();
        }
    }
}