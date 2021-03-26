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
    [RequireComponent(typeof(NetworkManager))]
    [AddComponentMenu("Unity Multiplayer Toolkit/MLAPI/NetworkClient")]
    public class NetworkClient : MonoBehaviour, INetworkManager, IInitializable, IConnectable
    {
        public bool IsServer => false;
        public bool IsClient => true;
        public bool IsRunning
        {
            get
            {
                if (NetworkManager.Singleton != null)
                {
                    return NetworkManager.Singleton.IsClient;
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

        public IObservable<List<NetworkObject>> OnNetworkedObjectSpawnedAsObservable() => _OnNetworkObjectSpawnedSubject;
        private Subject<List<NetworkObject>> _OnNetworkObjectSpawnedSubject = new Subject<List<NetworkObject>>();

        public IObservable<List<ulong>> OnNetworkedObjectDestroyedAsObservable() => _OnNetworkedObjectDestroyedSubject;
        private Subject<List<ulong>> _OnNetworkedObjectDestroyedSubject = new Subject<List<ulong>>();

        public IObservable<string> OnReceivedServerProcessDownEventAsObservable() => _OnReceivedServerProcessDownEventSubject;
        private Subject<string> _OnReceivedServerProcessDownEventSubject = new Subject<string>();

        public bool Initialized => _Initialized;
        private bool _Initialized;

        public bool Connected => _Connected;
        private bool _Connected;

        private int _TimeoutSeconds = 30;
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
            NetworkManager.Singleton.NetworkConfig.EnableSceneManagement = _NetworkConfig.EnableSceneManagement;
            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = _NetworkConfig.ConnectionApproval;
            NetworkManager.Singleton.NetworkConfig.CreatePlayerPrefab = _NetworkConfig.CreatePlayerPrefab;
            NetworkManager.Singleton.NetworkConfig.ForceSamePrefabs = _NetworkConfig.ForceSamePrefabs;
            NetworkManager.Singleton.NetworkConfig.RecycleNetworkIds = _NetworkConfig.RecycleNetworkIds;

            // Transport
            NetworkTransport transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            if (transport is MLAPI.Transports.UNET.UNetTransport unetTransport)
            {
                unetTransport.ConnectAddress = _ConnectionConfig.Address.Trim();
                unetTransport.ConnectPort = _ConnectionConfig.Port;
                unetTransport.ServerListenPort = _ConnectionConfig.Port;
            }
            else if (transport is MLAPI.Transports.Ruffles.RufflesTransport rufflesTransport)
            {
                rufflesTransport.ConnectAddress = _ConnectionConfig.Address.Trim();
                rufflesTransport.Port = (ushort)_ConnectionConfig.Port;
            }
            else if (transport is MLAPI.Transports.LiteNetLib.LiteNetLibTransport liteNetLibTransport)
            {
                liteNetLibTransport.Address = _ConnectionConfig.Address.Trim();
                liteNetLibTransport.Port = (ushort)_ConnectionConfig.Port;
            }
            // else if (transport is MLAPI.Transports.Enet.EnetTransport enetTransport)
            // {
            //     enetTransport.Address = _ConnectionConfig.Address.Trim();
            //     enetTransport.Port = (ushort)_ConnectionConfig.Port;
            // }
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

            if (NetworkManager.Singleton.IsHost)
            {
                Debug.LogError("[MLAPI Extension] This client is already running as a host client.");
                return false;
            }

            if (NetworkManager.Singleton.IsConnectedClient)
            {
                Debug.LogWarning("[MLAPI Extension] This client is already connected to the server.");
                return true;
            }

            // Callbacks
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // Start client
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(_ConnectionKey);
            SocketTasks tasks = NetworkManager.Singleton.StartClient();
            await UniTask.WaitUntil(() => tasks.IsDone).Timeout(TimeSpan.FromSeconds(_TimeoutSeconds));

            return _Connected = tasks.Success;
        }

        public void Disconnect()
        {
            _Connected = false;
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.StopClient();

                // Remove callbacks
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
            else
            {
                Debug.LogWarning("[MLAPI Extension] Cannot disconnect client because it is not running.");
            }
        }

        public void SendMessageToServer(string messageName, Stream dataStream, NetworkChannel channel = NetworkChannel.Internal)
        {
            if (_Connected)
            {
                ulong serverClientId = NetworkManager.Singleton.ServerClientId;
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
            using (PooledNetworkReader reader = PooledNetworkReader.Get(dataStream))
            {
                string message = reader.ReadStringPacked().ToString();
                _OnReceivedServerProcessDownEventSubject.OnNext(message);
            }
        }

#endregion

        private void SubscribeSpawnedObjects()
        {
            var beforeKeys = new ulong[0];

            NetworkSpawnManager.SpawnedObjects
            .ObserveEveryValueChanged(dict => dict.Count)
            .Skip(1)
            .Subscribe(count => 
            {
                var spawnedObjKeys = NetworkSpawnManager.SpawnedObjects.Keys.Except(beforeKeys);
                var destroyedObjKeys = beforeKeys.Except(NetworkSpawnManager.SpawnedObjects.Keys);

                beforeKeys = NetworkSpawnManager.SpawnedObjects.Keys.ToArray();

                List<NetworkObject> spawnedObjects = new List<NetworkObject>();
                foreach(var key in spawnedObjKeys)
                {
                    if (NetworkSpawnManager.SpawnedObjects.TryGetValue(key, out NetworkObject netObject))
                    {
                        spawnedObjects.Add(netObject);
                    }
                }

                List<ulong> destroyedObjectIds = destroyedObjKeys.ToList();

                if (spawnedObjects.Count > 0)
                {
                    _OnNetworkObjectSpawnedSubject.OnNext(spawnedObjects);
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
