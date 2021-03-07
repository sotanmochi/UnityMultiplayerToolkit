using UnityEngine;
using Cysharp.Threading.Tasks;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Client.Domain.Connection
{
    public class ConnectionConfigProvider : MonoBehaviour, INetworkConnectionConfigProvider
    {
        [SerializeField] ConnectionConfig _Config;

        public async UniTask<bool> Initialize()
        {
            return true;
        }
        
        public async UniTask<ConnectionConfig> GetConnectionConfig(string roomName)
        {
            return _Config;
        }
    }
}
