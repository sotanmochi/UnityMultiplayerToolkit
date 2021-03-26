using System;
using UnityEngine;

namespace UnityMultiplayerToolkit.Infra.AWS
{
    [Serializable]
    [CreateAssetMenu(menuName = "AWS for Unity/Create Config", fileName = "AWSConfig")]
    public class AWSConfig : ScriptableObject
    {
        public string Region;
        public string IdentityPoolId;
    }
}
