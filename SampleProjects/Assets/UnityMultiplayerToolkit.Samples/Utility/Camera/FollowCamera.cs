//
// Reference:
//   http://sasanon.hatenablog.jp/entry/2017/09/17/041612
//

using UnityEngine;

namespace UnityMultiplayerToolkit.Utility
{
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform _FollowTarget;   // 注視対象
        [SerializeField] private float turnSpeed = 10.0f;   // 回転速度
        [SerializeField] private float distance = 5.0f;    // 注視対象からカメラを離す距離

        public  Quaternion hRotation;      // カメラの水平回転
        private Quaternion vRotation;      // カメラの垂直回転(見下ろし回転)

        public void SetFollowTarget(Transform followTarget)
        {
            _FollowTarget = followTarget;

            // 回転の初期化
            vRotation = Quaternion.Euler(30, 0, 0);         // 垂直回転(X軸を軸とする回転)は、30度見下ろす回転
            hRotation = Quaternion.identity;                // 水平回転(Y軸を軸とする回転)は、無回転
            transform.rotation = hRotation * vRotation;     // 最終的なカメラの回転は、垂直回転してから水平回転する合成回転

            // 位置の初期化
            // player位置から距離distanceだけ手前に引いた位置を設定します
            transform.position = _FollowTarget.position - transform.rotation * Vector3.forward * distance; 
        }

        public void RotateHorizontal(float x)
        {
            hRotation *= Quaternion.Euler(0, x * turnSpeed, 0);
        }

        void LateUpdate()
        {
            if (_FollowTarget == null)
            {
                return;
            }

            // 水平回転の更新
            if(Input.GetMouseButton(0))
            {
                RotateHorizontal(Input.GetAxis("Mouse X"));
            }

            // カメラの回転(transform.rotation)の更新
            // 方法1 : 垂直回転してから水平回転する合成回転とします
            transform.rotation = hRotation * vRotation;

            // カメラの位置(transform.position)の更新
            // player位置から距離distanceだけ手前に引いた位置を設定します(位置補正版)
            transform.position = _FollowTarget.position + new Vector3(0, 3, 0) - transform.rotation * Vector3.forward * distance; 
        }
    }
}
