using System;
using UnityEngine;
using UniRx;

namespace UnityMultiplayerToolkit.Utility
{
    public interface IKeyInputProvider
    {
        IObservable<Unit> KeyInputAsObservable(KeyCode keycode);
        IObservable<Unit> KeyDownInputAsObservable(KeyCode keycode);
    }
}