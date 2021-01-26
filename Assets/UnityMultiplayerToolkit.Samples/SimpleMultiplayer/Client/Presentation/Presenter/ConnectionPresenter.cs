using UnityEngine;
using UniRx;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Client
{
    public class ConnectionPresenter : MonoBehaviour
    {
        [SerializeField] ConnectionView _ConnectionView;
        [SerializeField] ConnectionManager _ConnectionManager;

        async void Awake()
        {
            _ConnectionView.IsHostAsObservable()
            .Subscribe(isHost => 
            {
                _ConnectionManager.IsHost = isHost;
            })
            .AddTo(this);

            _ConnectionView.OnClickStartClientAsObservable()
            .Subscribe(_ => 
            {
                _ConnectionManager.StartClient();
            })
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
