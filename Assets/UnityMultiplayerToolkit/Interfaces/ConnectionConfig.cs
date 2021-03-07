using UnityEngine;

namespace UnityMultiplayerToolkit
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Unity Multiplayer Toolkit/Create Connection Config", fileName = "ConnectionConfig")]
    public class ConnectionConfig : ScriptableObject
    {
        public string Address = "127.0.0.1";
        public int Port = 7777;
        public string Key = "MultiplayerRoom";

        public static ConnectionConfig GetDefault()
        {
            ConnectionConfig config = new ConnectionConfig();
            config.Address = "127.0.0.1";
            config.Port = 7777;
            config.Key = "MultiplayerRoom";
            return config;
        }
    }
}
