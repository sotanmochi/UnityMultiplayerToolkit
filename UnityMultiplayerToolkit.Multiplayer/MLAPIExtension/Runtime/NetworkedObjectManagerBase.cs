using UnityEngine;
using UniRx;
using MLAPI;

namespace UnityMultiplayerToolkit.MLAPIExtension
{
    public abstract class NetworkedObjectManagerBase<T> : MonoBehaviour
    {
        public IReadOnlyReactiveDictionary<ulong, T> NetworkedObjects => _NetworkObjects; 
        private ReactiveDictionary<ulong, T> _NetworkObjects = new ReactiveDictionary<ulong, T>();

        private NetworkObject _NetworkObjectPrefab;
        private Transform _NetworkObjectParent;

        public void Initialize(INetworkManager networkManager, NetworkObject networkedObjectPrefab, Transform networkedObjectParent)
        {
            _NetworkObjectPrefab = networkedObjectPrefab;
            _NetworkObjectParent = networkedObjectParent;

            if (networkManager != null)
            {
                networkManager.OnNetworkedObjectSpawnedAsObservable()
                .Subscribe(netObjects => 
                {
                    // Debug.Log("SpawnedNetObjects: " + netObjects.Count);
                    foreach(var netObject in netObjects)
                    {
                        // Debug.Log("SpawnedNetObjId: " + netObject.NetworkId);

                        T component = netObject.GetComponent<T>();
                        if (component != null)
                        {
                            _NetworkObjects.Add(netObject.NetworkObjectId, component);
                        }
                    }
                })
                .AddTo(this);

                networkManager.OnNetworkedObjectDestroyedAsObservable()
                .Subscribe(destroyedObjectIds => 
                {
                    // Debug.Log("DestroyedNetObjects: " + destroyedObjectIds.Count);
                    foreach(ulong objectId in destroyedObjectIds)
                    {
                        // Debug.Log("DestroyedObjId: " + objectId);
                        _NetworkObjects.Remove(objectId);
                    }
                })
                .AddTo(this);
            }
            else
            {
                Debug.LogError("NetworkingManagerExtension is null.");
            }
        }

#region Server

        public void SpawnAsPlayerObject(ulong clientId, Vector3? position = null, Quaternion? rotation = null)
        {
            var go = GameObject.Instantiate(_NetworkObjectPrefab.gameObject, position.GetValueOrDefault(Vector3.zero), rotation.GetValueOrDefault(Quaternion.identity));
            go.transform.SetParent(_NetworkObjectParent, false);

            var networkedObject = go.GetComponent<NetworkObject>();
            networkedObject.SpawnAsPlayerObject(clientId);
        }

        public void SpawnWithClientOwnership(ulong ownerClientId, Vector3? position = null, Quaternion? rotation = null)
        {
            var go = GameObject.Instantiate(_NetworkObjectPrefab.gameObject, position.GetValueOrDefault(Vector3.zero), rotation.GetValueOrDefault(Quaternion.identity));
            go.transform.SetParent(_NetworkObjectParent, false);

            var networkedObject = go.GetComponent<NetworkObject>();
            networkedObject.SpawnWithOwnership(ownerClientId);
        }

        public void SpawnWithServerOwnership(Vector3? position = null, Quaternion? rotation = null)
        {
            var go = GameObject.Instantiate(_NetworkObjectPrefab.gameObject, position.GetValueOrDefault(Vector3.zero), rotation.GetValueOrDefault(Quaternion.identity));
            go.transform.SetParent(_NetworkObjectParent, false);

            var networkedObject = go.GetComponent<NetworkObject>();
            networkedObject.Spawn();
        }

#endregion

    }
}