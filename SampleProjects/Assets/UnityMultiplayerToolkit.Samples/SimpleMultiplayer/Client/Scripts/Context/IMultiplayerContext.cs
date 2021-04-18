using Cysharp.Threading.Tasks;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Client
{
    public interface IMultiplayerContext
    {
        UniTask<bool> Connect(string roomName, string userId = null);
        void Disconnect();
    }
}