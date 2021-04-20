using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace UnityMultiplayerToolkit.Utility
{
    public class KeyInputProvider : MonoBehaviour, IKeyInputProvider
    {
        public IReadOnlyList<KeyCode> ObservableKeyCodeList => _ObservableKeyCodeList.AsReadOnly();

        public IObservable<Unit> KeyInputAsObservable(KeyCode keycode)
        {
            if (_KeySubjects.ContainsKey(keycode))
            {
                return _KeySubjects[keycode];
            }
            return null;
        }

        public IObservable<Unit> KeyDownInputAsObservable(KeyCode keycode)
        {
            if (_KeyDownSubjects.ContainsKey(keycode))
            {
                return _KeyDownSubjects[keycode];
            }
            return null;
        }

        private Dictionary<KeyCode, Subject<Unit>> _KeySubjects = new Dictionary<KeyCode, Subject<Unit>>();
        private Dictionary<KeyCode, Subject<Unit>> _KeyDownSubjects = new Dictionary<KeyCode, Subject<Unit>>();

        public KeyInputProvider()
        {
            foreach (var keycode in _ObservableKeyCodeList)
            {
                _KeySubjects.Add(keycode, new Subject<Unit>());
                _KeyDownSubjects.Add(keycode, new Subject<Unit>());
            }
        }

        void Awake()
        {
            Observable.EveryUpdate()
            .Where(_ => Input.anyKey)
            .Subscribe(_ =>
            {
                foreach (var keycode in _KeySubjects.Keys)
                {
                    if (Input.GetKey(keycode))
                    {
                        // Debug.Log("GetKey: " + keycode);
                        _KeySubjects[keycode].OnNext(Unit.Default);
                    }
                }
            })
            .AddTo(this);

            Observable.EveryUpdate()
            .Where(_ => Input.anyKeyDown)
            .Subscribe(_ =>
            {
                foreach (var keycode in _KeySubjects.Keys)
                {
                    if (Input.GetKeyDown(keycode))
                    {
                        // Debug.Log("GetKeyDown: " + keycode);
                        _KeyDownSubjects[keycode].OnNext(Unit.Default);
                    }
                }
            })
            .AddTo(this);
        }

        private List<KeyCode> _ObservableKeyCodeList = new List<KeyCode>()
        {
            KeyCode.A,
            KeyCode.B,
            KeyCode.C,
            KeyCode.D,
            KeyCode.E,
            KeyCode.F,
            KeyCode.G,
            KeyCode.H,
            KeyCode.I,
            KeyCode.J,
            KeyCode.K,
            KeyCode.L,
            KeyCode.M,
            KeyCode.N,
            KeyCode.O,
            KeyCode.P,
            KeyCode.Q,
            KeyCode.R,
            KeyCode.S,
            KeyCode.T,
            KeyCode.U,
            KeyCode.V,
            KeyCode.W,
            KeyCode.X,
            KeyCode.Y,
            KeyCode.Z,
            KeyCode.Alpha0,
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
            KeyCode.F1,
            KeyCode.F2,
            KeyCode.F3,
            KeyCode.F4,
            KeyCode.F5,
            KeyCode.F6,
            KeyCode.F7,
            KeyCode.F8,
            KeyCode.F9,
            KeyCode.F10,
            KeyCode.F11,
            KeyCode.F12,
        };
    }
}