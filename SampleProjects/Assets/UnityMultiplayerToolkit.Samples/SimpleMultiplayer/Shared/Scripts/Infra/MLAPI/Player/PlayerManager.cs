using System;
using UnityEngine;
using UniRx;
using MLAPI;
using UnityMultiplayerToolkit.MLAPIExtension;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Shared
{
    public class PlayerManager : NetworkedObjectManagerBase<Player>, IPlayerManager
    {
        [SerializeField] MonoBehaviour _NetworkManagerObject;
        [SerializeField] NetworkObject _NetworkPlayerPrefab;

        public IObservable<IPlayer> OnSpawnedLocalPlayerAsObservable() => _OnSpawnedLocalPlayerSubject;
        private Subject<IPlayer> _OnSpawnedLocalPlayerSubject = new Subject<IPlayer>();

        public IObservable<Unit> OnDespawnedLocalPlayerAsObservable() => _OnDespawnedLocalPlayerSubject;
        private Subject<Unit> _OnDespawnedLocalPlayerSubject = new Subject<Unit>();

        private ulong _LocalPlayerObjectId;

        void Awake()
        {
            Initialize(_NetworkPlayerPrefab, this.transform);

            NetworkObjects.ObserveAdd().Subscribe(addEvent => 
            {
                ulong objectId = addEvent.Key;
                Debug.Log("Added ObjectID: " + objectId + ", Players.Count: " + NetworkObjects.Count);

                Player player = addEvent.Value;
                player.transform.SetParent(this.transform);

                if (player.IsOwner)
                {
                    _LocalPlayerObjectId = objectId;
                    _OnSpawnedLocalPlayerSubject.OnNext(player);
                }
            })
            .AddTo(this);

            NetworkObjects.ObserveRemove().Subscribe(removeEvent => 
            {
                ulong objectId = removeEvent.Key;
                Debug.Log("Removed ObjectID: " + objectId + ", Players.Count: " + NetworkObjects.Count);

                if (_LocalPlayerObjectId.Equals(objectId))
                {
                    _OnDespawnedLocalPlayerSubject.OnNext(Unit.Default);
                }
            })
            .AddTo(this);

            INetworkManager networkManager = _NetworkManagerObject.GetComponent<INetworkManager>();

            if (networkManager != null)
            {
                networkManager.OnClientConnectedAsObservable()
                .Where(_ => networkManager.IsServer)
                .Subscribe(clientId => 
                {
                    SpawnWithClientOwnership(clientId);
                })
                .AddTo(this);

                networkManager.OnClientDisconnectedAsObservable()
                .Where(_ => networkManager.IsServer)
                .Subscribe(clientId => 
                {
                    DespawnObjects(clientId);
                })
                .AddTo(this);
            }
            else
            {
                Debug.LogError("[SimpleMultiplayer] NetworkManager is null.");
            }
        }
    }
}