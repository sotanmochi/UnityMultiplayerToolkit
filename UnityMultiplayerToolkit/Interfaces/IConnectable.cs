using Cysharp.Threading.Tasks;

namespace UnityMultiplayerToolkit
{
    public interface IConnectable
    {
        UniTask<bool> Connect();
        void Disconnect();
    }
}