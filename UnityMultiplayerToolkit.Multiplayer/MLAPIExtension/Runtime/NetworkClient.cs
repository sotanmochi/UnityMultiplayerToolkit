using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityMultiplayerToolkit.Shared;
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
    public class NetworkClient : MonoBehaviour, INetworkManager
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

        public IObservable<string> OnReceivedServerProcessDownEventAsObservable() => _OnReceivedServerProcessDownEventSubject;
        private Subject<string> _OnReceivedServerProcessDownEventSubject = new Subject<string>();

        public IObservable<string> OnReceivedDisconnectMessageAsObservable() => _OnReceivedDisconnectMessageSubject;
        private Subject<string> _OnReceivedDisconnectMessageSubject = new Subject<string>();

        public bool Initialized => _Initialized;
        private bool _Initialized;

        public bool Connected => _Connected;
        private bool _Connected;

        private int _TimeoutSeconds = 30;

        void Awake()
        {
            CustomMessagingManager.RegisterNamedMessageHandler("NotifyServerProcessDown", OnReceivedServerProcessDownEvent);
            CustomMessagingManager.RegisterNamedMessageHandler("SendDisconnectMessageToClient", OnReceivedDisconnectMessage);
        }

        void OnDestroy()
        {
            CustomMessagingManager.UnregisterNamedMessageHandler("NotifyServerProcessDown");
            CustomMessagingManager.UnregisterNamedMessageHandler("SendDisconnectMessageToClient");
        }

        public bool Initialize(NetworkConfig networkConfig = null)
        {
            if (networkConfig == null)
            {
                networkConfig = new NetworkConfig();
            }

            // Network config
            NetworkManager.Singleton.NetworkConfig.EnableSceneManagement = networkConfig.EnableSceneManagement;
            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = networkConfig.ConnectionApproval;
            NetworkManager.Singleton.NetworkConfig.CreatePlayerPrefab = networkConfig.CreatePlayerPrefab;
            NetworkManager.Singleton.NetworkConfig.ForceSamePrefabs = networkConfig.ForceSamePrefabs;
            NetworkManager.Singleton.NetworkConfig.RecycleNetworkIds = networkConfig.RecycleNetworkIds;

            return _Initialized = true;
        }

        public void Uninitialize()
        {
            _Initialized = false;
        }

        public async UniTask<bool> Connect(ConnectionConfig connectionConfig = null)
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

            if (connectionConfig == null)
            {
                connectionConfig = ConnectionConfig.GetDefault();
            }

            // Transport
            NetworkTransport transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            if (transport is MLAPI.Transports.UNET.UNetTransport unetTransport)
            {
                unetTransport.ConnectAddress = connectionConfig.Address.Trim();
                unetTransport.ConnectPort = connectionConfig.Port;
                unetTransport.ServerListenPort = connectionConfig.Port;
            }
            // else if (transport is MLAPI.Transports.Ruffles.RufflesTransport rufflesTransport)
            // {
            //     rufflesTransport.ConnectAddress = connectionConfig.Address.Trim();
            //     rufflesTransport.Port = (ushort)connectionConfig.Port;
            // }
            else if (transport is MLAPI.Transports.LiteNetLib.LiteNetLibTransport liteNetLibTransport)
            {
                liteNetLibTransport.Address = connectionConfig.Address.Trim();
                liteNetLibTransport.Port = (ushort)connectionConfig.Port;
            }
            // else if (transport is MLAPI.Transports.Enet.EnetTransport enetTransport)
            // {
            //     enetTransport.Address = connectionConfig.Address.Trim();
            //     enetTransport.Port = (ushort)connectionConfig.Port;
            // }
            else
            {
                Debug.LogError("[MLAPI Extension] Unknown transport.");
                return false;
            }

            // Callbacks
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // Start client
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(connectionConfig.Key);
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

        private void OnReceivedDisconnectMessage(ulong sender, Stream dataStream)
        {
            using (PooledNetworkReader reader = PooledNetworkReader.Get(dataStream))
            {
                string message = reader.ReadStringPacked().ToString();
                _OnReceivedDisconnectMessageSubject.OnNext(message);
            }
        }

#endregion

    }
}
