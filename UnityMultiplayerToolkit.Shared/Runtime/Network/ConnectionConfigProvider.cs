using UnityEngine;
using Cysharp.Threading.Tasks;

namespace UnityMultiplayerToolkit.Shared
{
    public class ConnectionConfigProvider : MonoBehaviour, IConnectionConfigProvider
    {
        [SerializeField] ConnectionConfig _Config;

        private ConnectionConfig _ConfigInstance;

        public async UniTask<bool> Initialize()
        {
            _ConfigInstance = new ConnectionConfig(_Config);
            return true;
        }

        public async UniTask<ConnectionConfig> GetConnectionConfig(string roomName, string userId = null)
        {
            _ConfigInstance.SystemUserId = userId;
            return _ConfigInstance;
        }
    }
}
