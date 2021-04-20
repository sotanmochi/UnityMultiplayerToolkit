using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Client
{
    public class TextMessageView : MonoBehaviour
    {
        [SerializeField] InputField _Text_Message;
        [SerializeField] Button _Button_SendToServer;
        [SerializeField] Button _Button_SendToAllClients;

        public IObservable<string> SendToServerAsObservable() => _SendToServerSubject;
        private Subject<string> _SendToServerSubject = new Subject<string>();
        public IObservable<string> SendToAllClientsAsObservable() => _SendToAllClientsSubject;
        private Subject<string> _SendToAllClientsSubject = new Subject<string>();

        void Awake()
        {
            _Button_SendToServer.OnClickAsObservable().Subscribe(_ =>
            {
                _SendToServerSubject.OnNext(_Text_Message.text);
            })
            .AddTo(this);

            _Button_SendToAllClients.OnClickAsObservable().Subscribe(_ =>
            {
                _SendToAllClientsSubject.OnNext(_Text_Message.text);
            })
            .AddTo(this);
        }
    }
}
