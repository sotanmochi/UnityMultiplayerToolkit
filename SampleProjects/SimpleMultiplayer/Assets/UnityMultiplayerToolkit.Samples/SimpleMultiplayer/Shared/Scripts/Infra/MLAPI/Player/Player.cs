using UnityEngine;
using UniRx;
using MLAPI;
using MLAPI.NetworkVariable;
using UnityMultiplayerToolkit.Utility;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Shared
{
    public class Player : NetworkBehaviour, IPlayer
    {
        [SerializeField] PlayerMovement _PlayerMovement;

        public Transform Transform => transform;

        public IReadOnlyReactiveProperty<Color> Color => _ColorReactiveProperty;
        private ReactiveProperty<Color> _ColorReactiveProperty = new ReactiveProperty<Color>();

        private NetworkVariableColor _Color = new NetworkVariableColor(new NetworkVariableSettings {WritePermission = NetworkVariablePermission.OwnerOnly});

        void Awake()
        {
            _Color.OnValueChanged = (previousValue, currentValue) =>
            {
                _ColorReactiveProperty.Value = currentValue;
            };
        }

        public void Move(float dirX, float dirY, float dirZ)
        {
            _PlayerMovement.Move(dirX, dirY, dirZ);
        }

        public void SetFollowCamera(GameObject followCameraObject)
        {
            _PlayerMovement.SetFollowCamera(followCameraObject.GetComponent<FollowCamera>());
        }

        public void SetColor(Color color)
        {
            _Color.Value = color;
        }
    }
}
