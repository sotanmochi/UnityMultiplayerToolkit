using UnityEngine;
using UnityMultiplayerToolkit.Samples.Utility;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer
{
    public class ApplicationInitializer : MonoBehaviour, IInitializableBeforeSceneLoad
    {
        public void InitializeBeforeSceneLoad()
        {
            UnityEngine.QualitySettings.vSyncCount = 0;
            UnityEngine.Application.targetFrameRate = 60;
            UnityEngine.Application.runInBackground = true;
        }
    }
}
