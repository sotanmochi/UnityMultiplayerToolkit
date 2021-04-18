
using UnityEngine;
using UnityMultiplayerToolkit.Samples.Utility;
using UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Client;
using UnityMultiplayerToolkit.Shared;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer
{
    public class Resolver : MonoBehaviour, IInitializableBeforeSceneLoad
    {
        [SerializeField] GameObject _ConnectionConfigProviderObject;

        public void InitializeBeforeSceneLoad()
        {
// #if UNITY_SERVER
//             // NetworkUtility.RemoveUpdateSystemForHeadlessServer();
//             Debug.Log("NetworkUtility.RemoveUpdateSystemForHeadlessServer()");
// #endif
        }
    }
}
