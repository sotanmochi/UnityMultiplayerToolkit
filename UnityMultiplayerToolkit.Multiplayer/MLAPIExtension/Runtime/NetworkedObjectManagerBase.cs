using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using MLAPI;
using MLAPI.Spawning;

namespace UnityMultiplayerToolkit.MLAPIExtension
{
    public abstract class NetworkedObjectManagerBase<T> : MonoBehaviour
    {
        public IReadOnlyReactiveDictionary<ulong, T> NetworkObjects => _NetworkObjects; 
        private ReactiveDictionary<ulong, T> _NetworkObjects = new ReactiveDictionary<ulong, T>();

        public IObservable<List<NetworkObject>> OnNetworkObjectSpawnedAsObservable() => _OnNetworkObjectSpawnedSubject;
        private Subject<List<NetworkObject>> _OnNetworkObjectSpawnedSubject = new Subject<List<NetworkObject>>();

        public IObservable<List<ulong>> OnNetworkObjectDestroyedAsObservable() => _OnNetworkObjectDestroyedSubject;
        private Subject<List<ulong>> _OnNetworkObjectDestroyedSubject = new Subject<List<ulong>>();

        private NetworkObject _NetworkObjectPrefab;
        private Transform _NetworkObjectParent;
        private bool _Initialized;

        public void Initialize(NetworkObject networkedObjectPrefab, Transform networkedObjectParent)
        {
            if (_Initialized) return;

            _NetworkObjectPrefab = networkedObjectPrefab;
            _NetworkObjectParent = networkedObjectParent;

            ObserveNetworkSpawnedObjects();

            OnNetworkObjectSpawnedAsObservable()
            .Subscribe(spawnedObjects => 
            {
                // Debug.Log("SpawnedNetObjects: " + spawnedObjects.Count);
                foreach(var networkObject in spawnedObjects)
                {
                    T component = networkObject.GetComponent<T>();
                    if (component != null)
                    {
                        _NetworkObjects.Add(networkObject.NetworkObjectId, component);
                    }
                }
            })
            .AddTo(this);

            OnNetworkObjectDestroyedAsObservable()
            .Subscribe(destroyedObjectIds => 
            {
                // Debug.Log("DestroyedNetObjects: " + destroyedObjectIds.Count);
                foreach(ulong objectId in destroyedObjectIds)
                {
                    _NetworkObjects.Remove(objectId);
                }
            })
            .AddTo(this);

            _Initialized = true;
        }

#region Server method

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

        public void DespawnObjects(ulong ownerClientId, bool destroy = true)
        {
            List<NetworkObject> spawnedObjects = NetworkSpawnManager.SpawnedObjectsList.Where(netObj => netObj.OwnerClientId == ownerClientId).ToList();
            foreach(var networkObject in spawnedObjects)
            {
                T component = networkObject.GetComponent<T>();
                if (component != null)
                {
                    networkObject.Despawn(destroy);
                }
            }
        }

#endregion

        private void ObserveNetworkSpawnedObjects()
        {
            var beforeKeys = new ulong[0];

            NetworkSpawnManager.SpawnedObjects
            .ObserveEveryValueChanged(dict => dict.Count)
            .Skip(1)
            .Subscribe(count => 
            {
                var spawnedObjKeys = NetworkSpawnManager.SpawnedObjects.Keys.Except(beforeKeys);
                var destroyedObjKeys = beforeKeys.Except(NetworkSpawnManager.SpawnedObjects.Keys);

                beforeKeys = NetworkSpawnManager.SpawnedObjects.Keys.ToArray();

                List<NetworkObject> spawnedObjects = new List<NetworkObject>();
                foreach(var key in spawnedObjKeys)
                {
                    if (NetworkSpawnManager.SpawnedObjects.TryGetValue(key, out NetworkObject netObject))
                    {
                        spawnedObjects.Add(netObject);
                    }
                }

                List<ulong> destroyedObjectIds = destroyedObjKeys.ToList();

                if (spawnedObjects.Count > 0)
                {
                    // Debug.Log("SpawnedNetObjects: " + spawnedObjects.Count);
                    _OnNetworkObjectSpawnedSubject.OnNext(spawnedObjects);
                }
                if (destroyedObjectIds.Count > 0)
                {
                    // Debug.Log("DestroyedNetObjects: " + destroyedObjectIds.Count);
                    _OnNetworkObjectDestroyedSubject.OnNext(destroyedObjectIds);
                }
            })
            .AddTo(this);
        }
    }
}