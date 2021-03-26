using Cysharp.Threading.Tasks;

namespace UnityMultiplayerToolkit
{
    public interface IConnectionConfigProvider
    {
        UniTask<bool> Initialize();
        UniTask<ConnectionConfig> GetConnectionConfig(string roomName);
    }
}
