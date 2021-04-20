using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Client
{
    public class ConnectionPresenter : MonoBehaviour
    {
        [SerializeField] ConnectionView _ConnectionView;
        [SerializeField] MultiplayerContext _MultiplayerContext;

        void Awake()
        {
            _ConnectionView.OnClickStartClientAsObservable()
            .Subscribe(_ => UniTask.Void(async () => 
            {
                await _MultiplayerContext.Connect(_ConnectionView.RoomName);
            }))
            .AddTo(this);

            _ConnectionView.OnClickStopClientAsObservable()
            .Subscribe(_ => 
            {
                _MultiplayerContext.Disconnect();
            })
            .AddTo(this);
        }
    }
}
