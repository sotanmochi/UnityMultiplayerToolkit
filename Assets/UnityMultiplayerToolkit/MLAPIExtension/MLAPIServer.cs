using System;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using MLAPI.Transports;
using MLAPI.Transports.Tasks;

namespace UnityMultiplayerToolkit.MLAPIExtension
{
    [AddComponentMenu("MLAPI Extension/MLAPIServer")]
    [RequireComponent(typeof(MLAPI.NetworkingManager))]
    public class MLAPIServer : MonoBehaviour
    {
        public bool AutoStart;
        [SerializeField] NetworkConfig _NetworkConfig;

        public IObservable<Unit> OnServerStartedAsObservable() => _OnServerStartedSubject;
        private Subject<Unit> _OnServerStartedSubject = new Subject<Unit>();

        public IObservable<ulong> OnClientConnectedAsObservable() => _OnClientConnectedSubject;
        private Subject<ulong> _OnClientConnectedSubject = new Subject<ulong>();

        public IObservable<ulong> OnClientDisconnectedAsObservable() => _OnClientDisconnectedSubject;
        private Subject<ulong> _OnClientDisconnectedSubject = new Subject<ulong>();

        public bool IsRunning => MLAPI.NetworkingManager.Singleton.IsServer;

        private ConnectionConfig _ConnectionConfig;
        private MLAPIServer _Instance;

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

    }
}
