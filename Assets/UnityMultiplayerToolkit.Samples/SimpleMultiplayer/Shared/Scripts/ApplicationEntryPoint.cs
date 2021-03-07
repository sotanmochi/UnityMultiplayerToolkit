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

#if !UNITY_SERVER
            INetworkConnectionConfigProvider configProvider = FindObjectOfInterface<INetworkConnectionConfigProvider>();

            // Presenter
            var connectionPresenter = GameObject.FindObjectOfType<Client.ConnectionPresenter>();
            connectionPresenter.Construct(configProvider);
#endif

#if UNITY_SERVER
            // NetworkUtility.RemoveUpdateSystemForHeadlessServer();
            Debug.Log("NetworkUtility.RemoveUpdateSystemForHeadlessServer()");
#endif

        }

        /// <summary>
        /// 指定されたインターフェイスを実装したコンポーネントを持つオブジェクトを検索します
        /// https://baba-s.hatenablog.com/entry/2014/12/27/144311
        /// </summary>
        static T FindObjectOfInterface<T>() where T : class
        {
            foreach ( var n in GameObject.FindObjectsOfType<Component>() )
            {
                var component = n as T;
                if ( component != null )
                {
                    return component;
                }
            }
            return null;
        }
    }
}