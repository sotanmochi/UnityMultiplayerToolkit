using UnityEngine;

namespace UnityMultiplayerToolkit.PerformanceTest
{
    public class ApplicationInitializer : MonoBehaviour
    {
        void Awake()
        {
            UnityEngine.QualitySettings.vSyncCount = 0;
            UnityEngine.Application.targetFrameRate = 60;
            UnityEngine.Application.runInBackground = true;
        }
    }
}
