using System.Collections.Generic;
using UnityEngine;
using UniRx;
using MLAPI;
using UnityMultiplayerToolkit.MLAPIExtension;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer
{
    public class PlayerManager : NetworkedObjectManagerBase<Player>
    {
        [SerializeField] MonoBehaviour _NetworkingManagerExtension;
        [SerializeField] NetworkedObject _NetworkedPlayerPrefab;

        [SerializeField] private Dictionary<ulong, Player> _Players = new Dictionary<ulong, Player>();

        void Awake()
        {
            NetworkedObjects.ObserveAdd().Subscribe(kv => 
            {
                _Players.Add(kv.Key, kv.Value);
                Debug.Log("Added ObjectID: " + kv.Key + ", Players.Count: " + _Players.Count);
            })
            .AddTo(this);

            NetworkedObjects.ObserveRemove().Subscribe(kv => 
            {
                _Players.Remove(kv.Key);
                Debug.Log("Removed ObjectID: " + kv.Key + ", Players.Count: " + _Players.Count);
            })
            .AddTo(this);

            INetworkingManagerExtension networkingManagerExtension = _NetworkingManagerExtension.GetComponent<INetworkingManagerExtension>();

            if (networkingManagerExtension != null)
            {
                Initialize(networkingManagerExtension, _NetworkedPlayerPrefab, this.transform);

                networkingManagerExtension.OnClientConnectedAsObservable()
                .Where(_ => networkingManagerExtension.IsServer)
                .Subscribe(clientId => 
                {
                    SpawnWithClientOwnership(clientId);
                })
                .AddTo(this);
            }
            else
            {
                Debug.LogError("NetworkingManagerExtension is null.");
            }
        }
    }
}