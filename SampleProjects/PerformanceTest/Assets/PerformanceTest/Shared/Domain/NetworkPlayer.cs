using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;

namespace UnityMultiplayerToolkit.PerformanceTest
{
    public class NetworkPlayer : NetworkBehaviour
    {
        NetworkVariable<Quaternion> _quaternion = new NetworkVariable<Quaternion>(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.OwnerOnly });

        void Awake()
        {
            _quaternion.OnValueChanged += QuaternionValueChanged;
        }

        void OnDestroy()
        {
            _quaternion.OnValueChanged -= QuaternionValueChanged;
        }

        void Update()
        {
            if (IsOwner)
            {
                transform.Rotate(0, 1, 0);
                _quaternion.Value = transform.rotation;
            }
        }

        private void QuaternionValueChanged(Quaternion oldValue, Quaternion newValue)
        {
            if (!IsOwner)
            {
                transform.rotation = newValue;
            }
        }
    }
}