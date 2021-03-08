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
using MLAPI.Spawning;
using MLAPI.Serialization.Pooled;

namespace UnityMultiplayerToolkit.MLAPIExtension
{
    [AddComponentMenu("MLAPI Extension/MLAPIClient")]
    [RequireComponent(typeof(MLAPI.NetworkingManager))]
    public class MLAPIClient : MonoBehaviour, INetworkingManagerExtension
    {
        public bool IsServer => false;
        public bool IsClient => true;
        public bool IsRunning
        {
            get
            {
                if (MLAPI.NetworkingManager.Singleton != null) return MLAPI.NetworkingManager.Singleton.IsClient;
                else return false;
            }
        }

        public IObservable<Unit> OnHostStartedAsObservable() => _OnHostStartedSubject;
        private Subject<Unit> _OnHostStartedSubject = new Subject<Unit>();

        public IObservable<ulong> OnClientConnectedAsObservable() => _OnClientConnectedSubject;
        private Subject<ulong> _OnClientConnectedSubject = new Subject<ulong>();

        public IObservable<ulong> OnClientDisconnectedAsObservable() => _OnClientDisconnectedSubject;
        private Subject<ulong> _OnClientDisconnectedSubject = new Subject<ulong>();

        public IObservable<string> OnReceivedServerProcessDownEventAsObservable() => _OnReceivedServerProcessDownEventSubject;
        private Subject<string> _OnReceivedServerProcessDownEventSubject = new Subject<string>();

        public IObservable<List<NetworkedObject>> OnSpawnedObjectsAsObservable() => _OnSpawnedObjectsSubject;
        private Subject<List<NetworkedObject>> _OnSpawnedObjectsSubject = new Subject<List<NetworkedObject>>();

        public IObservable<List<ulong>> OnDestroyedObjectsAsObservable() => _OnDestroyedObjectsSubject;
        private Subject<List<ulong>> _OnDestroyedObjectsSubject = new Subject<List<ulong>>();

        public bool Initialized => _Initialized;
        private bool _Initialized;

        public bool Connected => _Connected;
        private bool _Connected;

        private string _ConnectionKey;

        private CompositeDisposable _CompositeDisposable;

        private void Awake()
        {
            _CompositeDisposable = new CompositeDisposable();
            SubscribeSpawnedObjects();
            CustomMessagingManager.RegisterNamedMessageHandler("NotifyServerProcessDown", OnReceivedServerProcessDownEvent);
        }

        private void OnDestroy()
        {
            CustomMessagingManager.UnregisterNamedMessageHandler("NotifyServerProcessDown");
            _CompositeDisposable.Dispose();
        }

        public bool Initialize(NetworkConfig networkConfig = null, MLAPIConnectionConfig connectionConfig = null)
        {
            _Initialized = false;

            if (networkConfig == null)
            {
                networkConfig = new NetworkConfig();
            }
            if (connectionConfig == null)
            {
                connectionConfig = MLAPIConnectionConfig.GetDefault();
            }

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
                unetTransport.ConnectAddress = connectionConfig.Address.Trim();
                unetTransport.ConnectPort = connectionConfig.Port;
                unetTransport.ServerListenPort = connectionConfig.Port;
            }
#if RUFFLES_TRANSPORT
            else if (transport is RufflesTransport.RufflesTransport rufflesTransport)
            {
                rufflesTransport.ConnectAddress = connectionConfig.Address.Trim();
                rufflesTransport.Port = (ushort)connectionConfig.Port;
            }
#endif
            else
            {
                Debug.LogError("[MLAPI Extension] Unknown transport.");
                return false;
            }

            _ConnectionKey = connectionConfig.Key;
            _Initialized = true;

            return true;
        }

        public async UniTask<bool> StartHost()
        {
            if (!_Initialized)
            {
                Debug.LogError("[MLAPI Extension] Cannot start host until initialized.");
                return false;
            }

            if(MLAPI.NetworkingManager.Singleton.IsClient)
            {
                Debug.LogError("[MLAPI Extension] This client is already running as a client.");
                return false;
            }

            if (MLAPI.NetworkingManager.Singleton.IsHost)
            {
                Debug.LogError("[MLAPI Extension] This client is already running as a host client.");
                return false;
            }

            // Callbacks
            MLAPI.NetworkingManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            MLAPI.NetworkingManager.Singleton.OnServerStarted += OnServerStarted;
            MLAPI.NetworkingManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            MLAPI.NetworkingManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // Initialize transport
            MLAPI.NetworkingManager.Singleton.NetworkConfig.NetworkTransport.Init();

            // Start host
            SocketTasks tasks = MLAPI.NetworkingManager.Singleton.StartHost();
            await UniTask.WaitUntil(() => tasks.IsDone);

            return MLAPI.NetworkingManager.Singleton.IsHost;
        }

        public void StopHost()
        {
            if (MLAPI.NetworkingManager.Singleton != null && MLAPI.NetworkingManager.Singleton.IsHost)
            {
                Debug.Log("[MLAPI Extension] MLAPI host client has stopped.");
                MLAPI.NetworkingManager.Singleton.StopHost();

                // Remove callbacks
                MLAPI.NetworkingManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
                MLAPI.NetworkingManager.Singleton.OnServerStarted -= OnServerStarted;
                MLAPI.NetworkingManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                MLAPI.NetworkingManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
            else
            {
                Debug.Log("[MLAPI Extension] Cannot stop host client because it is not running.");
            }
        }

        public async UniTask<bool> Connect()
        {
            if (!_Initialized)
            {
                Debug.LogError("[MLAPI Extension] Cannot start client until initialized.");
                return false;
            }

            if (MLAPI.NetworkingManager.Singleton.IsHost)
            {
                Debug.LogError("[MLAPI Extension] This client is already running as a host client.");
                return false;
            }

            if (MLAPI.NetworkingManager.Singleton.IsConnectedClient)
            {
                Debug.LogWarning("[MLAPI Extension] This client is already connected to the server.");
                return true;
            }

            // Callbacks
            MLAPI.NetworkingManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            MLAPI.NetworkingManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // Start client
            MLAPI.NetworkingManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(_ConnectionKey);
            SocketTasks tasks = MLAPI.NetworkingManager.Singleton.StartClient();
            await UniTask.WaitUntil(() => tasks.IsDone);

            return _Connected = tasks.Success;
        }

        public void Disconnect()
        {
            _Connected = false;
            if (MLAPI.NetworkingManager.Singleton != null && MLAPI.NetworkingManager.Singleton.IsClient)
            {
                MLAPI.NetworkingManager.Singleton.StopClient();

                // Remove callbacks
                MLAPI.NetworkingManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                MLAPI.NetworkingManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
            else
            {
                Debug.LogWarning("[MLAPI Extension] Cannot disconnect client because it is not running.");
            }
        }

        public void SendMessageToServer(string messageName, Stream dataStream, string channel = null)
        {
            if (_Connected)
            {
                ulong serverClientId = MLAPI.NetworkingManager.Singleton.ServerClientId;
                CustomMessagingManager.SendNamedMessage(messageName, serverClientId, dataStream, channel);
            }
            else
            {
                Debug.LogError("[MLAPI Extension] Cannot send message to server until connected.");
            }
        }

#region Callbacks

        private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkingManager.ConnectionApprovedDelegate callback)
        {
            Debug.LogError("[MLAPI Extension] ConnectionApprovalCallback has not implemented.");
        }

        private void OnServerStarted()
        {
            _OnHostStartedSubject.OnNext(Unit.Default);
        }

        private void OnClientConnected(ulong clientId)
        {
            _OnClientConnectedSubject.OnNext(clientId);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            _OnClientDisconnectedSubject.OnNext(clientId);
        }

        private void OnReceivedServerProcessDownEvent(ulong sender, Stream dataStream)
        {
            using (PooledBitReader reader = PooledBitReader.Get(dataStream))
            {
                string message = reader.ReadStringPacked().ToString();
                _OnReceivedServerProcessDownEventSubject.OnNext(message);
            }
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
