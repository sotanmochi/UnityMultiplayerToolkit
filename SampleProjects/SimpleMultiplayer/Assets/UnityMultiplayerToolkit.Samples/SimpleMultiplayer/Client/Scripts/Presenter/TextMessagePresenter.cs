using UnityEngine;
using UniRx;
using UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Shared;

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

            _TextMessageView.SendToAllClientsAsObservable().Subscribe(message => 
            {
                _MessagingHub.SendTextMessageToAllClients(message);
            })
            .AddTo(this);

            _MessagingHub.OnReceivedTextMessageAsObservable().Subscribe(item => 
            {
                Debug.Log("[SimpleMultiplayer] Sender ID: " + item.senderId + ", TextMessage: " + item.message);
            })
            .AddTo(this);
        }
    }
}
