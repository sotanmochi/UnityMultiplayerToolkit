﻿using System;
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
using UnityMultiplayerToolkit.Shared;

namespace UnityMultiplayerToolkit.MLAPIExtension
{
    [RequireComponent(typeof(NetworkManager))]
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
                if (NetworkManager.Singleton != null)
                {
                    return NetworkManager.Singleton.IsServer;
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

        public int ConnectedClientCount => (NetworkManager.Singleton != null) ? NetworkManager.Singleton.ConnectedClientsList.Count : -1;

#if MLAPI_PERFORMANCE_TEST
        public IObservable<(float networkTime, int processedEventsPerTick, ulong receivedDataBytesPerTick)> OnNetworkEarlyUpdatedAsObservable() 
            => _OnNetworkEarlyUpdatedSubject;
        private Subject<(float networkTime, int processedEventsPerTick, ulong receivedDataBytesPerTick)> _OnNetworkEarlyUpdatedSubject 
            = new Subject<(float networkTime, int processedEventsPerTick, ulong receivedDataBytesPerTick)>();

        public int MaxReceiveEventsPerTickRate => (NetworkManager.Singleton != null) ? NetworkManager.Singleton.MaxReceiveEventsPerTickRate : 0;
#endif

        public bool Initialized => _Initialized;
        private bool _Initialized;

        private ConnectionConfig _ConnectionConfig;

        void Awake()
        {
        }

        async void Start()
        {
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
            if (MLAPI.NetworkManager.Singleton.IsServer)
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
            NetworkManager.Singleton.NetworkConfig.EnableSceneManagement = networkConfig.EnableSceneManagement;
            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = networkConfig.ConnectionApproval;
            NetworkManager.Singleton.NetworkConfig.CreatePlayerPrefab = networkConfig.CreatePlayerPrefab;
            NetworkManager.Singleton.NetworkConfig.ForceSamePrefabs = networkConfig.ForceSamePrefabs;
            NetworkManager.Singleton.NetworkConfig.RecycleNetworkIds = networkConfig.RecycleNetworkIds;

            // Transport
            NetworkTransport transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            if (transport is MLAPI.Transports.UNET.UNetTransport unetTransport)
            {
                // unetTransport.ConnectAddress = connectionConfig.Address.Trim();
                // unetTransport.ConnectPort = connectionConfig.Port;
                unetTransport.ServerListenPort = connectionConfig.Port;
            }
            // else if (transport is MLAPI.Transports.Ruffles.RufflesTransport rufflesTransport)
            // {
            //     // rufflesTransport.ConnectAddress = _ConnectionConfig.Address.Trim();
            //     rufflesTransport.Port = (ushort)_ConnectionConfig.Port;
            // }
            else if (transport is MLAPI.Transports.LiteNetLib.LiteNetLibTransport liteNetLibTransport)
            {
                // liteNetLibTransport.Address = _ConnectionConfig.Address.Trim();
                liteNetLibTransport.Port = (ushort)_ConnectionConfig.Port;
            }
            // else if (transport is MLAPI.Transports.Enet.EnetTransport enetTransport)
            // {
            //     // enetTransport.Address = _ConnectionConfig.Address.Trim();
            //     enetTransport.Port = (ushort)_ConnectionConfig.Port;
            // }
            else
            {
                Debug.LogError("[MLAPI Extension] Unknown transport.");
                return false;
            }

            // Callbacks
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

#if MLAPI_PERFORMANCE_TEST
            NetworkManager.Singleton.OnNetworkEarlyUpdated += OnNetworkEarlyUpdated;
#endif

            // Initialize transport
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.Init();

            // Start server
            SocketTasks tasks = NetworkManager.Singleton.StartServer();
            await UniTask.WaitUntil(() => tasks.IsDone);

            return tasks.Success;
        }

        public void StopServer()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.StopServer();
                Debug.Log("[MLAPI Extension] MLAPI Server has stopped.");

                // Remove callbacks
                NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
                NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

#if MLAPI_PERFORMANCE_TEST
                NetworkManager.Singleton.OnNetworkEarlyUpdated -= OnNetworkEarlyUpdated;
#endif
            }
            else
            {
                Debug.LogWarning("[MLAPI Extension] Cannot stop server because it is not running.");
            }
        }

        public async void DisconnectClient(ulong clientId, string message)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                SendDisconnectMessageToClient(message, clientId);
                await UniTask.DelayFrame(60);
                NetworkManager.Singleton.DisconnectClient(clientId);
            }
        }

        public void SendDisconnectMessageToClient(string message, ulong clientId)
        {
            using (PooledNetworkBuffer stream = PooledNetworkBuffer.Get())
            {
                using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
                {
                    writer.WriteStringPacked(message);
                    var clientIds = new List<ulong>{ clientId };
                    SendMessageToClients("SendDisconnectMessageToClient", clientIds, stream);
                }
            }
        }

        public void NotifyServerProcessDownToAllClients(string message, NetworkChannel channel = NetworkChannel.Internal)
        {
            using (PooledNetworkBuffer outputStream = PooledNetworkBuffer.Get())
            {
                using (PooledNetworkWriter writer = PooledNetworkWriter.Get(outputStream))
                {
                    writer.WriteStringPacked(message);
                }

                List<ulong> clientIds = MLAPI.NetworkManager.Singleton.ConnectedClientsList.Select(client => client.ClientId).ToList<ulong>();
                CustomMessagingManager.SendNamedMessage("NotifyServerProcessDown", clientIds, outputStream, channel);
            }
        }

        public void SendMessageToClients(string messageName, List<ulong> clientIds, Stream dataStream, NetworkChannel channel = NetworkChannel.Internal)
        {
            CustomMessagingManager.SendNamedMessage(messageName, clientIds, dataStream, channel);
        }

        public void SendMessageToAllClients(string messageName, Stream dataStream, NetworkChannel channel = NetworkChannel.Internal)
        {
            List<ulong> clientIds = MLAPI.NetworkManager.Singleton.ConnectedClientsList.Select(client => client.ClientId).ToList<ulong>();
            CustomMessagingManager.SendNamedMessage(messageName, clientIds, dataStream, channel);
        }

        public void SendMessageToAllClientsExcept(string messageName, ulong clientIdToIgnore, Stream dataStream, NetworkChannel channel = NetworkChannel.Internal)
        {
            List<ulong> clientIds = MLAPI.NetworkManager.Singleton.ConnectedClientsList
                                    .Where(client => client.ClientId != clientIdToIgnore)
                                    .Select(client => client.ClientId).ToList<ulong>();
            CustomMessagingManager.SendNamedMessage(messageName, clientIds, dataStream, channel);
        }

#region Callbacks

#if MLAPI_PERFORMANCE_TEST
        private void OnNetworkEarlyUpdated(float networkTime, int processedEventsPerTick, ulong receivedDataBytesPerTick)
        {
            _OnNetworkEarlyUpdatedSubject.OnNext((networkTime, processedEventsPerTick, receivedDataBytesPerTick));
        }
#endif

        private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
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

    }
}
