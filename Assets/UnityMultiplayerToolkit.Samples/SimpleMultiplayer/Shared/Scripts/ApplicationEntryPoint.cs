using UnityEngine;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer
{
    public class ApplicationEntryPoint : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // static void InitializeBeforeSceneLoad()
        static void Main()
        {
            Debug.Log("<color=green>ApplicationEntryPoint.InitializeBeforeSceneLoad()</color>");

            Application.runInBackground = true;
            Application.targetFrameRate = 60;

#if UNITY_SERVER
            // NetworkUtility.RemoveUpdateSystemForHeadlessServer();
            Debug.Log("NetworkUtility.RemoveUpdateSystemForHeadlessServer()");
#endif

        }
    }
}