using System;
using System.IO;
using UnityEngine;
using UniRx;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Server
{
    [AddComponentMenu("MLAPI Extension/SimpleMultiplayer/Server_MessagingHub")]
    public class MessagingHub : MonoBehaviour
    {
        public IObservable<string> OnReceivedTextMessageAsObservable() => _OnReceivedTextMessageSubject;
        private Subject<string> _OnReceivedTextMessageSubject = new Subject<string>();

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
            CustomMessagingManager.RegisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_SERVER, MessageHandler_SendTextMessageToServer);
        }

        void UnregisterNamedMessageHandlers()
        {
            CustomMessagingManager.UnregisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_SERVER);
        }

#region Callbacks

        private void MessageHandler_SendTextMessageToServer(ulong sender, Stream dataStream)
        {
            using (PooledBitReader reader = PooledBitReader.Get(dataStream))
            {
                string message = reader.ReadStringPacked().ToString();
                _OnReceivedTextMessageSubject.OnNext(message);
            }
        }

#endregion

    }
}
