using UnityEngine;
using MLAPI.Serialization.Pooled;
using UnityMultiplayerToolkit.MLAPIExtension;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Client
{
    [AddComponentMenu("MLAPI Extension/SimpleMultiplayer/Client_MessagingHub")]
    public class MessagingHub : MonoBehaviour
    {
        [SerializeField] MLAPIClient _MLAPIClient;

        void Awake()
        {
            RegisterNamedMessageHandlers();
        }

        void OnDestroy()
        {
            UnregisterNamedMessageHandlers();
        }

        void RegisterNamedMessageHandlers()
        {

        }

        void UnregisterNamedMessageHandlers()
        {

        }

#region Client to Server

        public void SendTextMessageToServer(string message)
        {
            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    writer.WriteStringPacked(message);
                    _MLAPIClient.SendMessageToServer(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_SERVER, stream);
                }
            }
        }

#endregion

#region Server to Client


#endregion

    }
}
