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

namespace UnityMultiplayerToolkit.MLAPIExtension
{
    [AddComponentMenu("MLAPI Extension/MLAPIServer")]
    [RequireComponent(typeof(MLAPI.NetworkingManager))]
    public class MLAPIServer : MonoBehaviour, INetworkingManagerExtension
    {
        public bool AutoStart = true;
        [SerializeField] NetworkConfig _NetworkConfig;

        public bool IsServer => true;
        public bool IsClient => false;
        public bool IsRunning => MLAPI.NetworkingManager.Singleton.IsServer;

        public IObservable<Unit> OnServerStartedAsObservable() => _OnServerStartedSubject;
        private Subject<Unit> _OnServerStartedSubject = new Subject<Unit>();

        public IObservable<ulong> OnClientConnectedAsObservable() => _OnClientConnectedSubject;
        private Subject<ulong> _OnClientConnectedSubject = new Subject<ulong>();

        public IObservable<ulong> OnClientDisconnectedAsObservable() => _OnClientDisconnectedSubject;
        private Subject<ulong> _OnClientDisconnectedSubject = new Subject<ulong>();

        public IObservable<List<NetworkedObject>> OnSpawnedObjectsAsObservable() => _OnSpawnedObjectsSubject;
        private Subject<List<NetworkedObject>> _OnSpawnedObjectsSubject = new Subject<List<NetworkedObject>>();

        public IObservable<List<ulong>> OnDestroyedObjectsAsObservable() => _OnDestroyedObjectsSubject;
        private Subject<List<ulong>> _OnDestroyedObjectsSubject = new Subject<List<ulong>>();

        private ConnectionConfig _ConnectionConfig;

        private CompositeDisposable _CompositeDisposable;
        private MLAPIServer _Instance; // Singleton instance

        private void Awake()
        {
            if (_Instance != null && _Instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _Instance = this;
                DontDestroyOnLoad(this.gameObject);
                _CompositeDisposable = new CompositeDisposable();
                Application.runInBackground = true;
                Application.targetFrameRate = 60;
            }
        }

        private async void Start()
        {
#if UNITY_SERVER
            AutoStart = true;
#endif
            if (AutoStart)
            {
                bool success = await StartServer(_NetworkConfig);
                if (success)
                {
                    Debug.Log("[MLAPI Extension] MLAPI Server has started. Port: " + _ConnectionConfig.Port);
                }
            }
        }

        private void OnDestroy()
        {
            if (_Instance != null && _Instance == this)
            {
                _Instance = null;
                _CompositeDisposable.Dispose();
                StopServer();
            }
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
                connectionConfig = ConnectionConfig.LoadConfigFile();
            }

            _ConnectionConfig = connectionConfig;

            // Network config
            MLAPI.NetworkingManager.Singleton.NetworkConfig.EnableSceneManagement = networkConfig.EnableSceneManagement;
            MLAPI.NetworkingManager.Singleton.NetworkConfig.ConnectionApproval = networkConfig.ConnectionApproval;
            MLAPI.NetworkingManager.Singleton.NetworkConfig.CreatePlayerPrefab = networkConfig.CreatePlayerPrefab;
            MLAPI.NetworkingManager.Singleton.NetworkConfig.ForceSamePrefabs = networkConfig.ForceSamePrefabs;
            MLAPI.NetworkingManager.Singleton.NetworkConfig.RecycleNetworkIds = networkConfig.RecycleNetworkIds;

            // Transport
            Transport transport = MLAPI.NetworkingManager.Singleton.NetworkConfig.NetworkTransport;
            if (transport is MLAPI.Transports.UNET.UnetTransport unetTransport)
            {
                // unetTransport.ConnectAddress = connectionConfig.Address.Trim();
                // unetTransport.ConnectPort = connectionConfig.Port;
                unetTransport.ServerListenPort = connectionConfig.Port;
            }
#if RUFFLES_TRANSPORT
            else if (transport is RufflesTransport.RufflesTransport rufflesTransport)
            {
                // rufflesTransport.ConnectAddress = connectionConfig.Address.Trim();
                rufflesTransport.Port = (ushort)connectionConfig.Port;
            }
#endif
            else
            {
                Debug.LogError("[MLAPI Extension] Unknown transport.");
                return false;
            }

            SubscribeSpawnedObjects();

            // Callbacks
            MLAPI.NetworkingManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            MLAPI.NetworkingManager.Singleton.OnServerStarted += OnServerStarted;
            MLAPI.NetworkingManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            MLAPI.NetworkingManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // Initialize transport
            MLAPI.NetworkingManager.Singleton.NetworkConfig.NetworkTransport.Init();

            // Start server
            SocketTasks tasks = MLAPI.NetworkingManager.Singleton.StartServer();
            await UniTask.WaitUntil(() => tasks.IsDone);

            return MLAPI.NetworkingManager.Singleton.IsServer;
        }

        public void StopServer()
        {
            if (MLAPI.NetworkingManager.Singleton.IsServer)
            {
                MLAPI.NetworkingManager.Singleton.StopServer();
                Debug.Log("[MLAPI Extension] MLAPI Server has stopped.");

                // Remove callbacks
                MLAPI.NetworkingManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
                MLAPI.NetworkingManager.Singleton.OnServerStarted -= OnServerStarted;
                MLAPI.NetworkingManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                MLAPI.NetworkingManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
            else
            {
                Debug.LogWarning("[MLAPI Extension] Cannot stop server because it is not running.");
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

        private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkingManager.ConnectionApprovedDelegate callback)
        {
            Debug.LogError("[MLAPI Extension] ConnectionApprovalCallback has not implemented.");
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
                    _OnSpawnedObjectsSubject.OnNext(spawnedObjects);
                }
                if (destroyedObjectIds.Count > 0)
                {
                    _OnDestroyedObjectsSubject.OnNext(destroyedObjectIds);
                }
            })
            .AddTo(_CompositeDisposable);
        }

    }
}
