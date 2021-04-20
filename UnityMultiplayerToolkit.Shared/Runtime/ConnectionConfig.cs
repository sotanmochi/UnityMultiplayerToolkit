using UnityEngine;

namespace UnityMultiplayerToolkit.Shared
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Unity Multiplayer Toolkit/Create Connection Config", fileName = "ConnectionConfig")]
    public class ConnectionConfig : ScriptableObject
    {
        public string Address = "127.0.0.1";
        public int Port = 7777;
        public string Key = "MultiplayerRoom";
        public string SystemUserId = "User-Dummy-1234567890";

        public ConnectionConfig()
        {
        }

        public ConnectionConfig(ConnectionConfig config)
        {
            Address = config.Address;
            Port = config.Port;
            Key = config.Key;
            SystemUserId = config.SystemUserId;
        }

        public ConnectionConfig(string address, int port)
        {
            Address = address;
            Port = port;
        }

        public ConnectionConfig(string address, int port, string systemUserId)
        {
            Address = address;
            Port = port;
            SystemUserId = systemUserId;
        }

        public static ConnectionConfig GetDefault()
        {
            ConnectionConfig config = new ConnectionConfig();
            config.Address = "127.0.0.1";
            config.Port = 7777;
            config.Key = "MultiplayerRoom";
            config.SystemUserId = "User-Dummy-1234567890";
            return config;
        }
    }
}
