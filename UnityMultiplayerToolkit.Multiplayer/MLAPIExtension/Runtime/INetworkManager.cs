using System;
using System.Collections.Generic;
using MLAPI;

namespace UnityMultiplayerToolkit.MLAPIExtension
{
    public interface INetworkManager
    {
        bool IsServer { get; }
        bool IsClient { get; }
        bool IsRunning { get; }
        IObservable<ulong> OnClientConnectedAsObservable();
        IObservable<ulong> OnClientDisconnectedAsObservable();
    }
}
