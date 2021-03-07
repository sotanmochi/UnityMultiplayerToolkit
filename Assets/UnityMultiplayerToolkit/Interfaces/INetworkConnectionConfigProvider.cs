using Cysharp.Threading.Tasks;

namespace UnityMultiplayerToolkit
{
    public interface INetworkConnectionConfigProvider
    {
        UniTask<bool> Initialize();
        UniTask<ConnectionConfig> GetConnectionConfig(string roomName);
    }
}
