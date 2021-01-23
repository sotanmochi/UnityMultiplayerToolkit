using System.IO;
using UnityEngine;

namespace UnityMultiplayerToolkit.MLAPIExtension
{
    [System.Serializable]
    public class ConnectionConfig
    {
        public string Address;
        public int Port;
        public string Key;

        public static ConnectionConfig GetDefault()
        {
            ConnectionConfig config = new ConnectionConfig();
            config.Address = "127.0.0.1";
            config.Port = 7777;
            config.Key = "MultiplayerRoom";
            return config;
        }

        public static ConnectionConfig LoadConfigFile()
        {
            if (!File.Exists(ConfigFilePath))
            {
                var defaultConfig = GetDefault();
                defaultConfig.SaveConfigFile();
                return defaultConfig;
            }
            string jsonStr = File.ReadAllText(ConfigFilePath);
            var config = JsonUtility.FromJson<ConnectionConfig>(jsonStr);
            return config;
        }

        public void SaveConfigFile()
        {
            string jsonStr = JsonUtility.ToJson(this);
            File.WriteAllText(ConfigFilePath, jsonStr);
        }

        protected static string ConfigFilePath
        {
            get
            {
                return Path.Combine(Application.dataPath, "MultiplayerConnectionConfig.json");
            }
        }
    }
}
