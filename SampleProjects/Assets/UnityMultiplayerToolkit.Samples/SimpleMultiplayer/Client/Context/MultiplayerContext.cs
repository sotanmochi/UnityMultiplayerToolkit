using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityMultiplayerToolkit.Shared;
using UnityMultiplayerToolkit.MLAPIExtension;
using UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Shared;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Client
{
    public class MultiplayerContext : MonoBehaviour
    {
        [SerializeField] private GameObject _ConnectionConfigProviderObject;
        [SerializeField] private NetworkConfig _NetworkClientConfig;
        [SerializeField] private NetworkClient _NetworkClient;
        [SerializeField] private MessagingHub _MessagingHub;

        private bool _Initialized;
        private IConnectionConfigProvider _ConnectionConfigProvider;

        void OnDestroy()
        {
            Disconnect();
        }

        // [Inject]
        // Called from IInitializableBeforeSceneLoad.InitializeBeforeSceneLoad()
        public void Construct(IConnectionConfigProvider connectionConfigProvider)
        {
            _ConnectionConfigProvider = connectionConfigProvider;
        }

        public async UniTask<bool> Initialize()
        {
            _ConnectionConfigProvider = _ConnectionConfigProviderObject.GetComponent<IConnectionConfigProvider>();
            _NetworkClient.Disconnect();

            bool success = true;
            success &= await _ConnectionConfigProvider.Initialize();
            success &= _NetworkClient.Initialize(_NetworkClientConfig);

            if (!success)
            {
                Debug.LogError("[SimpleMultiplayer] Initialization of MultiplayerContext has been failed.");
            }

            return _Initialized = success;
        }

        public void Uninitialize()
        {
            _Initialized = false;
        }

        public async UniTask<bool> Connect(string roomName, string userId = null)
        {
            if (!_Initialized)
            {
                await Initialize();
            }

            if (string.IsNullOrEmpty(userId))
            {
                userId = System.Guid.NewGuid().ToString();
                // userId = "User-Dummy-1234567890";
            }

            var connectionConfig = await _ConnectionConfigProvider.GetConnectionConfig(roomName, userId);
            bool connected = await _NetworkClient.Connect(connectionConfig);

            if (connected)
            {
                Debug.Log("<color=orange> ConnectionConfig.SystemUserId: </color>" + connectionConfig.SystemUserId);
                _MessagingHub.SendSystemUserIdToServer(connectionConfig.SystemUserId);
            }
            else
            {
                Debug.LogError("[SimpleMultiplayer] This client cannot connected to the server.");
            }

            return connected;
        }

        public void Disconnect()
        {
            _NetworkClient.Disconnect();
        }
    }
}