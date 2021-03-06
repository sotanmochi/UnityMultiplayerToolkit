using UnityEngine;
using UniRx;
using UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Shared;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Client
{
    public class ServerProcessControlPresenter : MonoBehaviour
    {
        [SerializeField] ServerProcessControlView _ServerProcessControlView;
        [SerializeField] MessagingHub _MessagingHub;

        void Awake()
        {
            _ServerProcessControlView.OnTriggerServerProcessDownCommandAsObservable()
            .Subscribe(_ => 
            {
                _MessagingHub.SendServerProcessDownCommand(30);
            })
            .AddTo(this);
        }
    }
}
