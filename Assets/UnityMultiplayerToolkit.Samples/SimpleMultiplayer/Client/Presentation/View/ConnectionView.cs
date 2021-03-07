using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Client
{
    public class ConnectionView : MonoBehaviour
    {
        [SerializeField] InputField _InputField_RoomName;
        [SerializeField] Toggle _Toggle_IsHost;
        [SerializeField] Button _Button_StartClient;
        [SerializeField] Button _Button_StopClient;

        public string RoomName => _InputField_RoomName.text;
        public IObservable<bool> IsHostAsObservable() => _Toggle_IsHost.OnValueChangedAsObservable();
        public IObservable<Unit> OnClickStartClientAsObservable() => _Button_StartClient.OnClickAsObservable();
        public IObservable<Unit> OnClickStopClientAsObservable() => _Button_StopClient.OnClickAsObservable();
    }
}
