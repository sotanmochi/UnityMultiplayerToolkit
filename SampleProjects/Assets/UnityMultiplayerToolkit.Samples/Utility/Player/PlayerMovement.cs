//
// Reference:
//   http://sasanon.hatenablog.jp/entry/2017/09/17/041612
//

using UnityEngine;

namespace UnityMultiplayerToolkit.Utility
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 5.0f;
        [SerializeField] float applySpeed = 0.2f;

        private Vector3 velocity;
        private FollowCamera _FollowCamera;

        public void SetFollowCamera(FollowCamera followCamera)
        {
            _FollowCamera = followCamera;
        }

        public void Move(float dirX, float dirY, float dirZ)
        {
            velocity.x = dirX;
            velocity.y = dirY;
            velocity.z = dirZ;

            velocity = velocity.normalized * moveSpeed * Time.deltaTime;

            if(velocity.magnitude > 0)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                        Quaternion.LookRotation(_FollowCamera.hRotation * velocity), applySpeed);

                transform.position += _FollowCamera.hRotation * velocity;
            }
        }
    }
}