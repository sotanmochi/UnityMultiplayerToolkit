using UnityEngine;
using UniRx;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Shared
{
    public interface IPlayer
    {
        Transform Transform { get; }
        IReadOnlyReactiveProperty<Color> Color { get; }
        void Move(float dirX, float dirY, float dirZ);
        void SetFollowCamera(GameObject followCameraObject);
        void SetColor(Color color);
    }
}