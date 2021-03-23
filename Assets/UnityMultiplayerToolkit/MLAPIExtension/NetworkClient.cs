using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using MLAPI;
using MLAPI.Transports;
using MLAPI.Transports.Tasks;
using MLAPI.Messaging;
using MLAPI.Spawning;
using MLAPI.Serialization.Pooled;

namespace UnityMultiplayerToolkit.MLAPIExtension
{
    [RequireComponent(typeof(NetworkingManager))]
    [AddComponentMenu("Unity Multiplayer Toolkit/MLAPI/NetworkClient")]
    public class NetworkClient : MonoBehaviour, INetworkManager, IInitializable, IConnectable
    {
        public bool IsServer => false;
        public bool IsClient => true;
        public bool IsRunning
        {
            get
            {
                if (NetworkingManager.Singleton != null)
                {
                    return NetworkingManager.Singleton.IsClient;
                }
                else
                {
                    return false;
                }
            }
        }

        public IObservable<ulong> OnClientConnectedAsObservable() => _OnClientConnectedSubject;
        private Subject<ulong> _OnClientConnectedSubject = new Subject<ulong>();

        public IObservable<ulong> OnClientDisconnectedAsObservable() => _OnClientDisconnectedSubject;
        private Subject<ulong> _OnClientDisconnectedSubject = new Subject<ulong>();

        public IObservable<List<NetworkedObject>> OnNetworkedObjectSpawnedAsObservable() => _OnNetworkedObjectSpawnedSubject;
        private Subject<List<NetworkedObject>> _OnNetworkedObjectSpawnedSubject = new Subject<List<NetworkedObject>>();

        public IObservable<List<ulong>> OnNetworkedObjectDestroyedAsObservable() => _OnNetworkedObjectDestroyedSubject;
        private Subject<List<ulong>> _OnNetworkedObjectDestroyedSubject = new Subject<List<ulong>>();

        public IObservable<string> OnReceivedServerProcessDownEventAsObservable() => _OnReceivedServerProcessDownEventSubject;
        private Subject<string> _OnReceivedServerProcessDownEventSubject = new Subject<string>();

        public bool Initialized => _Initialized;
        private bool _Initialized;

        public bool Connected => _Connected;
        private bool _Connected;

        private int _TimeoutSeconds;
        private string _ConnectionKey;

        private NetworkConfig _NetworkConfig;
        private ConnectionConfig _ConnectionConfig;
        private CompositeDisposable _CompositeDisposable;

        void Awake()
        {
            _CompositeDisposable = new CompositeDisposable();
            SubscribeSpawnedObjects();
            CustomMessagingManager.RegisterNamedMessageHandler("NotifyServerProcessDown", OnReceivedServerProcessDownEvent);
        }

        void OnDestroy()
        {
            CustomMessagingManager.UnregisterNamedMessageHandler("NotifyServerProcessDown");
            _CompositeDisposable.Dispose();
        }

        // [Inject]
        public void Construct(NetworkConfig networkConfig, ConnectionConfig connectionConfig)
        {
            _NetworkConfig = networkConfig;
            _ConnectionConfig = connectionConfig;
        }

        public bool Initialize()
        {
            _Initialized = false;

            if (_NetworkConfig == null)
            {
                _NetworkConfig = new NetworkConfig();
            }
            if (_ConnectionConfig == null)
            {
                _ConnectionConfig = ConnectionConfig.GetDefault();
            }

            // Network config
            NetworkingManager.Singleton.NetworkConfig.EnableSceneManagement = _NetworkConfig.EnableSceneManagement;
            NetworkingManager.Singleton.NetworkConfig.ConnectionApproval = _NetworkConfig.ConnectionApproval;
            NetworkingManager.Singleton.NetworkConfig.CreatePlayerPrefab = _NetworkConfig.CreatePlayerPrefab;
            NetworkingManager.Singleton.NetworkConfig.ForceSamePrefabs = _NetworkConfig.ForceSamePrefabs;
            NetworkingManager.Singleton.NetworkConfig.RecycleNetworkIds = _NetworkConfig.RecycleNetworkIds;

            // Transport
            Transport transport = NetworkingManager.Singleton.NetworkConfig.NetworkTransport;
            if (transport is MLAPI.Transports.UNET.UnetTransport unetTransport)
            {
                unetTransport.ConnectAddress = _ConnectionConfig.Address.Trim();
                unetTransport.ConnectPort = _ConnectionConfig.Port;
                unetTransport.ServerListenPort = _ConnectionConfig.Port;
            }
            else if (transport is LiteNetLibTransport.LiteNetLibTransport liteNetLibTransport)
            {
                liteNetLibTransport.Address = _ConnectionConfig.Address.Trim();
                liteNetLibTransport.Port = (ushort)_ConnectionConfig.Port;
            }
            else
            {
                Debug.LogError("[MLAPI Extension] Unknown transport.");
                return false;
            }

            _ConnectionKey = _ConnectionConfig.Key;
            _Initialized = true;

            return true;
        }

        public void Uninitialize()
        {
            _Initialized = false;
        }

        public async UniTask<bool> Connect()
        {
            if (!_Initialized)
            {
                Debug.LogError("[MLAPI Extension] Cannot start client until initialized.");
                return false;
            }

            if (NetworkingManager.Singleton.IsHost)
            {
                Debug.LogError("[MLAPI Extension] This client is already running as a host client.");
                return false;
            }

            if (NetworkingManager.Singleton.IsConnectedClient)
            {
                Debug.LogWarning("[MLAPI Extension] This client is already connected to the server.");
                return true;
            }

            // Callbacks
            NetworkingManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkingManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // Start client
            NetworkingManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(_ConnectionKey);
            SocketTasks tasks = NetworkingManager.Singleton.StartClient();
            await UniTask.WaitUntil(() => tasks.IsDone).Timeout(TimeSpan.FromSeconds(_TimeoutSeconds));

            return _Connected = tasks.Success;
        }

        public void Disconnect()
        {
            _Connected = false;
            if (NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsClient)
            {
                NetworkingManager.Singleton.StopClient();

                // Remove callbacks
                NetworkingManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkingManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
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
                ulong serverClientId = NetworkingManager.Singleton.ServerClientId;
                CustomMessagingManager.SendNamedMessage(messageName, serverClientId, dataStream, channel);
            }
            else
            {
                Debug.LogError("[MLAPI Extension] Cannot send message to server until connected.");
            }
        }

#region Callbacks

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
