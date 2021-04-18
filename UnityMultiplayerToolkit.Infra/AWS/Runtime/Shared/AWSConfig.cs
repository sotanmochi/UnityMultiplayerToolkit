using System;
using UnityEngine;

namespace UnityMultiplayerToolkit.Infra.AWS
{
    [Serializable]
    [CreateAssetMenu(menuName = "Unity Multiplayer Toolkit/Create AWS Config", fileName = "AWSConfig")]
    public class AWSConfig : ScriptableObject
    {
        public string Region;
        public string IdentityPoolId;
        public string LambdaFunctionName = "LambdaFunction";
    }
}
