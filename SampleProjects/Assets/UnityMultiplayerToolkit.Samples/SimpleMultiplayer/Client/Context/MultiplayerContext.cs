using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityMultiplayerToolkit.Shared;
using UnityMultiplayerToolkit.MLAPIExtension;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Client
{
    public class MultiplayerContext : MonoBehaviour
    {
        [SerializeField] private NetworkConfig _NetworkClientConfig;
        [SerializeField] private NetworkClient _NetworkClient;
        private IConnectionConfigProvider _ConnectionConfigProvider;

        private bool _Initialized;

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

            var connectionConfig = await _ConnectionConfigProvider.GetConnectionConfig(roomName, userId);
            bool connected = await _NetworkClient.Connect(connectionConfig);

            if (!connected)
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