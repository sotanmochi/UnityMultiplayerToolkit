using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer
{
    public class ConnectionView : MonoBehaviour
    {
        [SerializeField] Toggle _Toggle_IsHost;
        [SerializeField] Button _Button_StartClient;
        [SerializeField] Button _Button_StopClient;

        public IObservable<bool> IsHostAsObservable() => _Toggle_IsHost.OnValueChangedAsObservable();
        public IObservable<Unit> OnClickStartClientAsObservable() => _Button_StartClient.OnClickAsObservable();
        public IObservable<Unit> OnClickStopClientAsObservable() => _Button_StopClient.OnClickAsObservable();
    }
}
