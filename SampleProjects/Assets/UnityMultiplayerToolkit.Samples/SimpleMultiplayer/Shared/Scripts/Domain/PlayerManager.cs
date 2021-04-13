using UnityEngine;
using UniRx;
using MLAPI;
using UnityMultiplayerToolkit.MLAPIExtension;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer
{
    public class PlayerManager : NetworkedObjectManagerBase<Player>
    {
        [SerializeField] MonoBehaviour _NetworkManagerObject;
        [SerializeField] NetworkObject _NetworkPlayerPrefab;

        void Awake()
        {
            Initialize(_NetworkPlayerPrefab, this.transform);

            NetworkObjects.ObserveAdd().Subscribe(addEvent => 
            {
                ulong objectId = addEvent.Key;
                Debug.Log("Added ObjectID: " + objectId + ", Players.Count: " + NetworkObjects.Count);

                Player player = addEvent.Value;
                player.transform.SetParent(this.transform);
            })
            .AddTo(this);

            NetworkObjects.ObserveRemove().Subscribe(removeEvent => 
            {
                ulong objectId = removeEvent.Key;
                Debug.Log("Removed ObjectID: " + objectId + ", Players.Count: " + NetworkObjects.Count);
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
            }
            else
            {
                Debug.LogError("[SimpleMultiplayer] NetworkManager is null.");
            }
        }
    }
}