using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Client
{
    public class ServerProcessControlView : MonoBehaviour
    {
        [SerializeField] Button _Button_ServerProcessDownCommand;
        public IObservable<Unit> OnTriggerServerProcessDownCommandAsObservable() => _Button_ServerProcessDownCommand.OnClickAsObservable();
    }
}
