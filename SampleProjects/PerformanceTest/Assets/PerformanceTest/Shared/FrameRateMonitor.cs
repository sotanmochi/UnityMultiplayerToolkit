using UnityEngine;

namespace UnityMultiplayerToolkit.PerformanceTest
{
    public class FrameRateMonitor : MonoBehaviour
    {
        public float FramePerSecond => _fps;
        private float _fps;

        private int _updateRate = 10;
        private int   _frameCount;
        private float _deltaTime;

        void Update()
        {
            Tick();
        }

        // FPS Counter Sample
        // https://baba-s.hatenablog.com/entry/2019/05/04/220500
        private void Tick()
        {
            _deltaTime += Time.unscaledDeltaTime;

            _frameCount++;

            if ( !( _deltaTime > 1f / _updateRate ) ) return;

            _fps = _frameCount / _deltaTime;

            _deltaTime  = 0;
            _frameCount = 0;
        }
    }
}
