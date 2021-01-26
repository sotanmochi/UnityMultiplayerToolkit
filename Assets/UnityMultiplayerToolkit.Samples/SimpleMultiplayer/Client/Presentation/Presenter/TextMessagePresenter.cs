using UnityEngine;
using UniRx;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Client
{
    public class TextMessagePresenter : MonoBehaviour
    {
        [SerializeField] TextMessageView _TextMessageView;
        [SerializeField] MessagingHub _MessagingHub;

        void Awake()
        {
            _TextMessageView.SendToServerAsObservable().Subscribe(message => 
            {
                _MessagingHub.SendTextMessageToServer(message);
            })
            .AddTo(this);
        }
    }
}
