
using System.Collections.Generic;
using UnityEngine;

namespace UnityMultiplayerToolkit.Samples.Utility
{
    public static class ApplicationInitializer
    {
        /// <summary>
        /// Awakeより前に実行される
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitializeBeforeSceneLoad()
        {
            Debug.Log("<color=orange>ApplicationInitializer.InitializeBeforeSceneLoad()</color>");

            var initializables = FindObjectsOfInterface<IInitializableBeforeSceneLoad>();
            foreach (var i in initializables)
            {
                i.InitializeBeforeSceneLoad();
            }
        }

        /// <summary>
        /// 指定されたインターフェイスを実装したコンポーネントを持つオブジェクトを検索します
        /// https://baba-s.hatenablog.com/entry/2014/12/27/144311
        /// </summary>
        public static T FindObjectOfInterface<T>() where T : class
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

        public static List<T> FindObjectsOfInterface<T>() where T : class
        {
            List<T> list = new List<T>();
            foreach (var n in GameObject.FindObjectsOfType<Component>() )
            {
                var component = n as T;
                if (component != null )
                {
                    list.Add(component);
                }
            }
            return list;
        }
    }
}
