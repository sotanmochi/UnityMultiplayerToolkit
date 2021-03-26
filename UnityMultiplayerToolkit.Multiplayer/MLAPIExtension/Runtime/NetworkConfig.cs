using UnityEngine;

namespace UnityMultiplayerToolkit.MLAPIExtension
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Unity Multiplayer Toolkit/MLAPI/Create Network Config", fileName = "MLAPINetworkConfig")]
    public class NetworkConfig : ScriptableObject
    {
        public bool EnableSceneManagement = false;
        public bool ConnectionApproval = false;
        public bool CreatePlayerPrefab = false;
        public bool ForceSamePrefabs = false;
        public bool RecycleNetworkIds = false;
    }
}
