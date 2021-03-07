using System.Text;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.CognitoIdentity;

namespace UnityMultiplayerToolkit.Infra.AWS.Lambda
{
    [System.Serializable]
    public class ConnectionConfigRequest
    {
        public string RoomName;

        public ConnectionConfigRequest(string roomName)
        {
            RoomName = roomName;
        }
    }

    [System.Serializable]
    public class ConnectionConfigResponse
    {
        public string IpAddress;
        public int Port;
        public string RoomName;
    }

    public class LambdaClient : MonoBehaviour, INetworkConnectionConfigProvider
    {
        [SerializeField] string _LambdaFunctionName = "ClientServiceLambda";
        [SerializeField] AWSConfig _Config;

        public async UniTask<bool> Initialize()
        {
            return true;
        }

        public async UniTask<ConnectionConfig> GetConnectionConfig(string roomName)
        {
            RegionEndpoint regionEndpoint = RegionEndpoint.GetBySystemName(_Config.Region);
            CognitoAWSCredentials credentials = new CognitoAWSCredentials(_Config.IdentityPoolId, regionEndpoint);

            AmazonLambdaClient lambdaClient = new AmazonLambdaClient(credentials, regionEndpoint);
            string jsonStr = JsonUtility.ToJson(new ConnectionConfigRequest(roomName));

            var response = await InvokeLambdaFunctionAsync(lambdaClient, _LambdaFunctionName, InvocationType.RequestResponse, jsonStr);
            if (response.FunctionError == null)
            {
                if (response.StatusCode == 200)
                {
                    var payload = Encoding.ASCII.GetString(response.Payload.ToArray()) + "\n";
                    var session = JsonUtility.FromJson<ConnectionConfigResponse>(payload);

                    if (session == null)
                    {
                        Debug.Log($"Error in Lambda: {payload}");
                    }
                    else
                    {
                        return new ConnectionConfig(session.IpAddress, session.Port);
                    }
                }
            }

            Debug.LogError(response.FunctionError);
            return ConnectionConfig.GetDefault();
        }

        private async UniTask<InvokeResponse> InvokeLambdaFunctionAsync(AmazonLambdaClient lambdaClient, string functionName, InvocationType invocationType, string payload = "")
        {
            var request = new InvokeRequest
            {
                FunctionName = functionName,
                InvocationType = invocationType,
                Payload = payload,
            };
            return await lambdaClient.InvokeAsync(request);
        }
    }
}
