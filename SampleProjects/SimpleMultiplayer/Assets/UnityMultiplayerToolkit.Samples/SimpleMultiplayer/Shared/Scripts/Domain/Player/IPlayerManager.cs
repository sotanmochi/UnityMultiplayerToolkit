using System;
using UniRx;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Shared
{
    public interface IPlayerManager
    {
        IObservable<IPlayer> OnSpawnedLocalPlayerAsObservable();
        IObservable<Unit> OnDespawnedLocalPlayerAsObservable();
    }
}