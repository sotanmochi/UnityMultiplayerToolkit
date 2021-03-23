using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using MLAPI;
using MLAPI.Transports;
using MLAPI.Transports.Tasks;
using MLAPI.Messaging;
using MLAPI.Security;
using MLAPI.Spawning;
using MLAPI.Serialization.Pooled;

namespace UnityMultiplayerToolkit.MLAPIExtension
{
    [RequireComponent(typeof(NetworkingManager))]
    [AddComponentMenu("Unity Multiplayer Toolkit/MLAPI/NetworkServer")]
    public class NetworkServer : MonoBehaviour, INetworkManager
    {
        [SerializeField] bool _AutoStart = true;
        [SerializeField] NetworkConfig _NetworkConfig;

        public bool IsServer => true;
        public bool IsClient => false;
        public bool IsRunning
        {
            get
            {
                if (NetworkingManager.Singleton != null)
                {
                    return NetworkingManager.Singleton.IsServer;
                }
                else
                {
                    return false;
                }
            }
        }

        public IObservable<Unit> OnServerStartedAsObservable() => _OnServerStartedSubject;
        private Subject<Unit> _OnServerStartedSubject = new Subject<Unit>();

        public IObservable<ulong> OnClientConnectedAsObservable() => _OnClientConnectedSubject;
        private Subject<ulong> _OnClientConnectedSubject = new Subject<ulong>();

        public IObservable<ulong> OnClientDisconnectedAsObservable() => _OnClientDisconnectedSubject;
        private Subject<ulong> _OnClientDisconnectedSubject = new Subject<ulong>();

        public IObservable<List<NetworkedObject>> OnNetworkedObjectSpawnedAsObservable() => _OnNetworkedObjectSpawnedSubject;
        private Subject<List<NetworkedObject>> _OnNetworkedObjectSpawnedSubject = new Subject<List<NetworkedObject>>();

        public IObservable<List<ulong>> OnNetworkedObjectDestroyedAsObservable() => _OnNetworkedObjectDestroyedSubject;
        private Subject<List<ulong>> _OnNetworkedObjectDestroyedSubject = new Subject<List<ulong>>();

        public bool Initialized => _Initialized;
        private bool _Initialized;

        private ConnectionConfig _ConnectionConfig;
        private CompositeDisposable _CompositeDisposable;

        void Awake()
        {
            _CompositeDisposable = new CompositeDisposable();
            SubscribeSpawnedObjects();
        }

        async void Start()
        {
#if UNITY_SERVER
            _AutoStart = true;
#endif
            if (_AutoStart)
            {
                int listeningPort = 7777;
                string roomKey = "MultiplayerRoom";

                string[] args = System.Environment.GetCommandLineArgs();
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--port")
                    {
                        listeningPort = int.Parse(args[i + 1]);
                    }
                    if (args[i] == "--room")
                    {
                        roomKey = args[i + 1];
                    }
                }

                ConnectionConfig connectionConfig = ConnectionConfig.GetDefault();
                connectionConfig.Port = listeningPort;
                connectionConfig.Key = roomKey;

                bool success = await StartServer(_NetworkConfig, connectionConfig);
                if (success)
                {
                    Debug.Log("[MLAPI Extension] MLAPI Server has started. Port: " + _ConnectionConfig.Port);
                }
            }
        }

        void OnDestroy()
        {
            _CompositeDisposable.Dispose();
            StopServer();
        }

        public async void ApplicationQuit(int timeSeconds = 30)
        {
            string message = "The connected server process is going down in " + timeSeconds + "seconds.";

            Debug.Log(message);
            NotifyServerProcessDownToAllClients(message);

            await UniTask.Delay(TimeSpan.FromSeconds(timeSeconds));
            Application.Quit();
        }

        public async UniTask<bool> StartServer(NetworkConfig networkConfig = null, ConnectionConfig connectionConfig = null)
        {
            if (MLAPI.NetworkingManager.Singleton.IsServer)
            {
                Debug.LogWarning("[MLAPI Extension] Cannot start server while an instance is already running.");
                return false;
            }

            if (networkConfig == null)
            {   
                networkConfig = new NetworkConfig();
            }
            if (connectionConfig == null)
            {
                connectionConfig = ConnectionConfig.GetDefault();
            }

            _ConnectionConfig = connectionConfig;

            // Network config
            NetworkingManager.Singleton.NetworkConfig.EnableSceneManagement = networkConfig.EnableSceneManagement;
            NetworkingManager.Singleton.NetworkConfig.ConnectionApproval = networkConfig.ConnectionApproval;
            NetworkingManager.Singleton.NetworkConfig.CreatePlayerPrefab = networkConfig.CreatePlayerPrefab;
            NetworkingManager.Singleton.NetworkConfig.ForceSamePrefabs = networkConfig.ForceSamePrefabs;
            NetworkingManager.Singleton.NetworkConfig.RecycleNetworkIds = networkConfig.RecycleNetworkIds;

            // Transport
            Transport transport = NetworkingManager.Singleton.NetworkConfig.NetworkTransport;
            if (transport is MLAPI.Transports.UNET.UnetTransport unetTransport)
            {
                // unetTransport.ConnectAddress = connectionConfig.Address.Trim();
                // unetTransport.ConnectPort = connectionConfig.Port;
                unetTransport.ServerListenPort = connectionConfig.Port;
            }
            else if (transport is LiteNetLibTransport.LiteNetLibTransport liteNetLibTransport)
            {
                // liteNetLibTransport.Address = _ConnectionConfig.Address.Trim();
                liteNetLibTransport.Port = (ushort)_ConnectionConfig.Port;
            }
            else
            {
                Debug.LogError("[MLAPI Extension] Unknown transport.");
                return false;
            }

            // Callbacks
            NetworkingManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkingManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkingManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkingManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // Initialize transport
            NetworkingManager.Singleton.NetworkConfig.NetworkTransport.Init();

            // Start server
            SocketTasks tasks = NetworkingManager.Singleton.StartServer();
            await UniTask.WaitUntil(() => tasks.IsDone);

            return NetworkingManager.Singleton.IsServer;
        }

        public void StopServer()
        {
            if (NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsServer)
            {
                NetworkingManager.Singleton.StopServer();
                Debug.Log("[MLAPI Extension] MLAPI Server has stopped.");

                // Remove callbacks
                NetworkingManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
                NetworkingManager.Singleton.OnServerStarted -= OnServerStarted;
                NetworkingManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkingManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
            else
            {
                Debug.LogWarning("[MLAPI Extension] Cannot stop server because it is not running.");
            }
        }

        public void NotifyServerProcessDownToAllClients(string message, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
        {
            using (PooledBitStream outputStream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(outputStream))
                {
                    writer.WriteStringPacked(message);
                }

                List<ulong> clientIds = MLAPI.NetworkingManager.Singleton.ConnectedClientsList.Select(client => client.ClientId).ToList<ulong>();
                CustomMessagingManager.SendNamedMessage("NotifyServerProcessDown", clientIds, outputStream, channel, security);
            }
        }

        public void SendMessageToAllClients(string messageName, Stream dataStream, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
        {
            List<ulong> clientIds = MLAPI.NetworkingManager.Singleton.ConnectedClientsList.Select(client => client.ClientId).ToList<ulong>();
            CustomMessagingManager.SendNamedMessage(messageName, clientIds, dataStream, channel, security);
        }

        public void SendMessageToAllClientsExcept(string messageName, ulong clientIdToIgnore, Stream dataStream, string channel = null, SecuritySendFlags security = SecuritySendFlags.None)
        {
            List<ulong> clientIds = MLAPI.NetworkingManager.Singleton.ConnectedClientsList
                                    .Where(client => client.ClientId != clientIdToIgnore)
                                    .Select(client => client.ClientId).ToList<ulong>();
            CustomMessagingManager.SendNamedMessage(messageName, clientIds, dataStream, channel, security);
        }

#region Callbacks

        private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkingManager.ConnectionApprovedDelegate callback)
        {
            string connectionKey = System.Text.Encoding.ASCII.GetString(connectionData);
            bool approved = connectionKey.Equals(_ConnectionConfig.Key);

            Debug.Log("[MLAPI Extension] Client.ConnectionKey: " + connectionKey);
            Debug.Log("[MLAPI Extension] ConnectionConfig.Key: " + _ConnectionConfig.Key);

            callback(false, null, approved, null, null);
        }

        private void OnServerStarted()
        {
            _OnServerStartedSubject.OnNext(Unit.Default);
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log("[MLAPI Extension] Connected client id: " + clientId);
            _OnClientConnectedSubject.OnNext(clientId);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log("[MLAPI Extension] Disconnected client id: " + clientId);
            _OnClientDisconnectedSubject.OnNext(clientId);
        }

#endregion

        private void SubscribeSpawnedObjects()
        {
            var beforeKeys = new ulong[0];

            SpawnManager.SpawnedObjects
            .ObserveEveryValueChanged(dict => dict.Count)
            .Skip(1)
            .Subscribe(count => 
            {
                var spawnedObjKeys = SpawnManager.SpawnedObjects.Keys.Except(beforeKeys);
                var destroyedObjKeys = beforeKeys.Except(SpawnManager.SpawnedObjects.Keys);

                beforeKeys = SpawnManager.SpawnedObjects.Keys.ToArray();

                List<NetworkedObject> spawnedObjects = new List<NetworkedObject>();
                foreach(var key in spawnedObjKeys)
                {
                    if (SpawnManager.SpawnedObjects.TryGetValue(key, out NetworkedObject netObject))
                    {
                        spawnedObjects.Add(netObject);
                    }
                }

                List<ulong> destroyedObjectIds = destroyedObjKeys.ToList();

                if (spawnedObjects.Count > 0)
                {
                    _OnNetworkedObjectSpawnedSubject.OnNext(spawnedObjects);
                }
                if (destroyedObjectIds.Count > 0)
                {
                    _OnNetworkedObjectDestroyedSubject.OnNext(destroyedObjectIds);
                }
            })
            .AddTo(_CompositeDisposable);
        }
    }
}
