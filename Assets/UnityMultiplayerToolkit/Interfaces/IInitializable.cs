namespace UnityMultiplayerToolkit
{
    public interface IInitializable
    {
        bool Initialize();
        void Uninitialize();
    }
}