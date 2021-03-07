using UnityEngine;
using UnityMultiplayerToolkit;

namespace UnityMultiplayerToolkit.MLAPIExtension
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Unity Multiplayer Toolkit/MLAPI/Create Connection Config", fileName = "MLAPIConnectionConfig")]
    public class MLAPIConnectionConfig : ConnectionConfig
    {
        public static MLAPIConnectionConfig GetDefault()
        {
            MLAPIConnectionConfig config = new MLAPIConnectionConfig();
            config.Address = "127.0.0.1";
            config.Port = 7777;
            config.Key = "MultiplayerRoom";
            return config;
        }
    }
}
