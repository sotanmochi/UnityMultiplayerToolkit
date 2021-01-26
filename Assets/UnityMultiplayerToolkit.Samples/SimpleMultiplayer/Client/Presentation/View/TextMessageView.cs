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

        public IObservable<string> SendToServerAsObservable() => _SendToServerSubject;
        private Subject<string> _SendToServerSubject = new Subject<string>();

        void Awake()
        {
            _Button_SendToServer.OnClickAsObservable().Subscribe(_ =>
            {
                _SendToServerSubject.OnNext(_Text_Message.text);
            })
            .AddTo(this);
        }
    }
}
