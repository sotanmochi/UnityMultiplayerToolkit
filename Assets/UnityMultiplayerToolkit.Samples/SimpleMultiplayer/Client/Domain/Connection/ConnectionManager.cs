using UnityEngine;
using UniRx;
using UnityMultiplayerToolkit.MLAPIExtension;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Client
{
    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField] NetworkConfig _NetworkConfig;
        [SerializeField] ConnectionConfig _ConnectionConfig;
        [SerializeField] NetworkClient _Client;
        public bool IsHost;

        void Awake()
        {
            _Client.OnClientConnectedAsObservable()
            .Subscribe(clientId => 
            {
                Debug.Log("[SimpleMultiplayer] Connected client id: " + clientId);
            })
            .AddTo(this);

            _Client.OnClientDisconnectedAsObservable()
            .Subscribe(clientId => 
            {
                Debug.Log("[SimpleMultiplayer] Disconnected client id: " + clientId);
            })
            .AddTo(this);

            _Client.OnReceivedServerProcessDownEventAsObservable()
            .Subscribe(message => 
            {
                Debug.Log(message);
            });
        }

        void OnDestroy()
        {
            _Client.Disconnect();
        }

        public void Initialize(ConnectionConfig config)
        {
            _ConnectionConfig.Address = config.Address;
            _ConnectionConfig.Port = config.Port;
            _ConnectionConfig.Key = config.Key;
            _Client.Construct(_NetworkConfig, _ConnectionConfig);
            _Client.Initialize();
        }

        public async void StartClient()
        {
            bool success = await _Client.Connect();
            if (success)
            {
                Debug.Log("[SimpleMultiplayer] This client has connected to the server.");
            }
        }

        public void StopClient()
        {
            _Client.Disconnect();
        }
    }
}
