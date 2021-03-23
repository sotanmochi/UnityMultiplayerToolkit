using UnityEngine;
using UniRx;

namespace UnityMultiplayerToolkit.MLAPIExtension
{
    public abstract class NetworkedObjectManagerBase<T> : MonoBehaviour
    {
        public IReadOnlyReactiveDictionary<ulong, T> NetworkedObjects => _NetworkedObjects; 
        private ReactiveDictionary<ulong, T> _NetworkedObjects = new ReactiveDictionary<ulong, T>();

        private MLAPI.NetworkedObject _NetworkedObjectPrefab;
        private Transform _NetworkedObjectParent;

        public void Initialize(INetworkManager networkManager, MLAPI.NetworkedObject networkedObjectPrefab, Transform networkedObjectParent)
        {
            _NetworkedObjectPrefab = networkedObjectPrefab;
            _NetworkedObjectParent = networkedObjectParent;

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
                            _NetworkedObjects.Add(netObject.NetworkId, component);
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
                        _NetworkedObjects.Remove(objectId);
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
            var go = GameObject.Instantiate(_NetworkedObjectPrefab.gameObject, position.GetValueOrDefault(Vector3.zero), rotation.GetValueOrDefault(Quaternion.identity));
            go.transform.SetParent(_NetworkedObjectParent, false);

            var networkedObject = go.GetComponent<MLAPI.NetworkedObject>();
            networkedObject.SpawnAsPlayerObject(clientId);
        }

        public void SpawnWithClientOwnership(ulong ownerClientId, Vector3? position = null, Quaternion? rotation = null)
        {
            var go = GameObject.Instantiate(_NetworkedObjectPrefab.gameObject, position.GetValueOrDefault(Vector3.zero), rotation.GetValueOrDefault(Quaternion.identity));
            go.transform.SetParent(_NetworkedObjectParent, false);

            var networkedObject = go.GetComponent<MLAPI.NetworkedObject>();
            networkedObject.SpawnWithOwnership(ownerClientId);
        }

        public void SpawnWithServerOwnership(Vector3? position = null, Quaternion? rotation = null)
        {
            var go = GameObject.Instantiate(_NetworkedObjectPrefab.gameObject, position.GetValueOrDefault(Vector3.zero), rotation.GetValueOrDefault(Quaternion.identity));
            go.transform.SetParent(_NetworkedObjectParent, false);

            var networkedObject = go.GetComponent<MLAPI.NetworkedObject>();
            networkedObject.Spawn();
        }

#endregion

    }
}