using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityMultiplayerToolkit.Shared;
using UnityMultiplayerToolkit.MLAPIExtension;

namespace UnityMultiplayerToolkit.PerformanceTest
{
    public class PerformanceTestClientContext : MonoBehaviour
    {
        [SerializeField] GameObject _networkClientObject;
        [SerializeField] int _ConnectionDueTime = 3;
        [SerializeField] int _ConnectionInterval = 30;

        private bool _IsConnected;
        private NetworkClient _networkClient;

        async void Start()
        {
            bool argsExist = false;
            string address = "127.0.0.1";
            int listeningPort = 7777;
            string roomKey = "MultiplayerRoom";

            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--address")
                {
                    address = args[i + 1];
                    argsExist = true;
                }
                if (args[i] == "--port")
                {
                    listeningPort = int.Parse(args[i + 1]);
                    argsExist = true;
                }
            }

#if !UNITY_EDITOR
            if (!argsExist)
            {
                Debug.LogError($"Usage: UnityApp.exe --address <IP Address> --port <Port Number>");
                await UniTask.Delay(TimeSpan.FromSeconds(5));
                Application.Quit();
                return;
            }
#endif

            _networkClient = _networkClientObject.GetComponent<NetworkClient>();
            _networkClient.Initialize();

            ConnectionConfig connectionConfig = ConnectionConfig.GetDefault();
            connectionConfig.Address = address;
            connectionConfig.Port = listeningPort;
            connectionConfig.Key = roomKey;

            Observable.Timer(TimeSpan.FromSeconds(_ConnectionDueTime), TimeSpan.FromSeconds(_ConnectionInterval))
            .Where(_ => !_IsConnected)
            .Subscribe(_ => UniTask.Void(async() =>
            {
                Debug.Log($"Connecting to server.");
                _IsConnected = await _networkClient.Connect(connectionConfig);
                Debug.Log($"IsConnected: " + _IsConnected);
            }))
            .AddTo(this);

            _networkClient.OnClientConnectedAsObservable()
            .Subscribe(_ =>
            {
                Debug.Log($"Connected to server.");
            })
            .AddTo(this);

            _networkClient.OnClientDisconnectedAsObservable()
            .Subscribe(_ =>
            {
                _IsConnected = false;
                Debug.Log("Disconnected from server.");
            })
            .AddTo(this);
        }

        void OnDestroy()
        {
            _networkClient.Disconnect();
        }
    }
}
