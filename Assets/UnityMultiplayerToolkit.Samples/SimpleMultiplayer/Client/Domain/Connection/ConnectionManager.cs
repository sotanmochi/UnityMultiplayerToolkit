using UnityEngine;
using UniRx;
using UnityMultiplayerToolkit.MLAPIExtension;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer
{
    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField] NetworkConfig _NetworkConfig;
        [SerializeField] MLAPIClient _Client;
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
        }

        void Start()
        {
            _Client.Initialize(_NetworkConfig);
        }

        void OnDestroy()
        {
            if (IsHost)
            {
                _Client.StopHost();
            }
            else
            {
                _Client.Disconnect();
            }
        }

        public async void StartClient()
        {
            if (IsHost)
            {
                bool success = await _Client.StartHost();
                if (success)
                {
                    Debug.Log("[SimpleMultiplayer] This client has started as a host.");
                }
            }
            else
            {
                bool success = await _Client.Connect();
                if (success)
                {
                    Debug.Log("[SimpleMultiplayer] This client has connected to the server.");
                }
            }
        }

        public void StopClient()
        {
            if (IsHost)
            {
                _Client.StopHost();
            }
            else
            {
                _Client.Disconnect();
            }
        }
    }
}
