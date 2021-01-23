using System;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using MLAPI.Transports;
using MLAPI.Transports.Tasks;

namespace UnityMultiplayerToolkit.MLAPIExtension
{
    [AddComponentMenu("MLAPI Extension/MLAPIClient")]
    [RequireComponent(typeof(MLAPI.NetworkingManager))]
    public class MLAPIClient : MonoBehaviour
    {
        public IObservable<Unit> OnHostStartedAsObservable() => _OnHostStartedSubject;
        private Subject<Unit> _OnHostStartedSubject = new Subject<Unit>();

        public IObservable<ulong> OnClientConnectedAsObservable() => _OnClientConnectedSubject;
        private Subject<ulong> _OnClientConnectedSubject = new Subject<ulong>();

        public IObservable<ulong> OnClientDisconnectedAsObservable() => _OnClientDisconnectedSubject;
        private Subject<ulong> _OnClientDisconnectedSubject = new Subject<ulong>();

        public bool Initialized => _Initialized;
        private bool _Initialized;

        private MLAPIClient _Instance;

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
                Application.runInBackground = true;
            }
        }

        public bool Initialize(NetworkConfig networkConfig = null, ConnectionConfig connectionConfig = null)
        {
            _Initialized = false;

            if (networkConfig == null)
            {
                networkConfig = new NetworkConfig();
            }
            if (connectionConfig == null)
            {
                connectionConfig = ConnectionConfig.LoadConfigFile();
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
            if (MLAPI.NetworkingManager.Singleton.IsHost)
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
            SocketTasks tasks = MLAPI.NetworkingManager.Singleton.StartClient();
            await UniTask.WaitUntil(() => tasks.IsDone);

            return MLAPI.NetworkingManager.Singleton.IsConnectedClient;
        }

        public void Disconnect()
        {
            if (MLAPI.NetworkingManager.Singleton.IsClient)
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

#endregion

    }
}
