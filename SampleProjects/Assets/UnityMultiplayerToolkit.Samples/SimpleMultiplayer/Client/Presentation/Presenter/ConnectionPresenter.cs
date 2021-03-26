using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Client
{
    public class ConnectionPresenter : MonoBehaviour
    {
        [SerializeField] ConnectionView _ConnectionView;
        [SerializeField] ConnectionManager _ConnectionManager;
        
        private IConnectionConfigProvider _ConfigProvider;

        // Called from IInitializableBeforeSceneLoad.InitializeBeforeSceneLoad()
        public void Construct(IConnectionConfigProvider configProvider)
        {
            _ConfigProvider = configProvider;
        }

        async void Awake()
        {
            _ConnectionView.IsHostAsObservable()
            .Subscribe(isHost => 
            {
                _ConnectionManager.IsHost = isHost;
            })
            .AddTo(this);

            _ConnectionView.OnClickStartClientAsObservable()
            .Subscribe(_ => UniTask.Void(async () => 
            {
                var config = await _ConfigProvider.GetConnectionConfig(_ConnectionView.RoomName);
                _ConnectionManager.Initialize(config);
                _ConnectionManager.StartClient();
            }))
            .AddTo(this);

            _ConnectionView.OnClickStopClientAsObservable()
            .Subscribe(_ => 
            {
                _ConnectionManager.StopClient();
            })
            .AddTo(this);
        }
    }
}
