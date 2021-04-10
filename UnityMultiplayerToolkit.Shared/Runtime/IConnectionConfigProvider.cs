using Cysharp.Threading.Tasks;

namespace UnityMultiplayerToolkit.Shared
{
    public interface IConnectionConfigProvider
    {
        UniTask<bool> Initialize();
        UniTask<ConnectionConfig> GetConnectionConfig(string roomName, string playerId = null);
    }
}
