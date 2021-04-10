using UnityEngine;
using Cysharp.Threading.Tasks;

namespace UnityMultiplayerToolkit.Shared
{
    public class ConnectionConfigProvider : MonoBehaviour, IConnectionConfigProvider
    {
        [SerializeField] ConnectionConfig _Config;

        public async UniTask<bool> Initialize()
        {
            return true;
        }
        
        public async UniTask<ConnectionConfig> GetConnectionConfig(string roomName, string playerId = null)
        {
            return _Config;
        }
    }
}
