using UnityEngine;
using UniRx;
using UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Shared;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Client
{
    public class ServerProcessControlPresenter : MonoBehaviour
    {
        [SerializeField] ServerProcessControlView _ServerProcessControlView;
        [SerializeField] GameObject _GameRoomMessagingHubObject;

        private IGameRoomMessagingHub _MessagingHub;

        void Awake()
        {
            _MessagingHub = _GameRoomMessagingHubObject.GetComponent<IGameRoomMessagingHub>();

            _ServerProcessControlView.OnTriggerServerProcessDownCommandAsObservable()
            .Subscribe(_ => 
            {
                _MessagingHub.SendServerProcessDownCommand(30);
            })
            .AddTo(this);
        }
    }
}
