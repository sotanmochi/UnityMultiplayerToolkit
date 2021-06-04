using System;
using UnityEngine;
using UniRx;
using MLAPI;
using UnityMultiplayerToolkit.MLAPIExtension;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Shared
{
    public class GameStateContainer : NetworkedObjectManagerBase<GameState>
    {
        [SerializeField] NetworkObject _NetworkStatePrefab;
        [SerializeField] NetworkServer _NetworkServer;

        public IObservable<GameState> OnInitializedGameState() => _OnInitializedStateInstanceSubject;
        private Subject<GameState> _OnInitializedStateInstanceSubject = new Subject<GameState>();

        private GameState _NetworkStateInstance;

        void Awake()
        {
            Initialize(_NetworkStatePrefab, this.transform);

            if (_NetworkServer != null)
            {
                _NetworkServer.OnServerStartedAsObservable()
                .Subscribe(_ => 
                {
                    SpawnWithServerOwnership();
                })
                .AddTo(this);
            }

            NetworkObjects.ObserveAdd().Subscribe(addEvent => 
            {
                _NetworkStateInstance = addEvent.Value;
                _OnInitializedStateInstanceSubject.OnNext(_NetworkStateInstance);
                _OnInitializedStateInstanceSubject.OnCompleted();
            })
            .AddTo(this);
        }
    }
}
