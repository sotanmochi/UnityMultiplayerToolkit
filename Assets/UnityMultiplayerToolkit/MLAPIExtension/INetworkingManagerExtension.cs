using System;
using System.Collections.Generic;
using MLAPI;

namespace UnityMultiplayerToolkit.MLAPIExtension
{
    public interface INetworkingManagerExtension
    {
        bool IsServer { get; }
        bool IsClient { get; }
        bool IsRunning { get; }
        IObservable<ulong> OnClientConnectedAsObservable();
        IObservable<ulong> OnClientDisconnectedAsObservable();
        IObservable<List<NetworkedObject>> OnSpawnedObjectsAsObservable();
        IObservable<List<ulong>> OnDestroyedObjectsAsObservable();
    }
}
