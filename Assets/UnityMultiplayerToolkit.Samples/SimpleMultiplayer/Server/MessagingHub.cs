using System;
using System.IO;
using UnityEngine;
using UniRx;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using UnityMultiplayerToolkit.MLAPIExtension;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Server
{
    [AddComponentMenu("MLAPI Extension/SimpleMultiplayer/Server_MessagingHub")]
    public class MessagingHub : MonoBehaviour
    {
        [SerializeField] MLAPIServer _MLAPIServer;

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
            CustomMessagingManager.RegisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_ALL_CLIENTS, MessageHandler_SendTextMessageToAllClients);
            CustomMessagingManager.RegisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_CLIENTS_EXCEPT_SELF, MessageHandler_SendTextMessageExceptSelf);
        }

        void UnregisterNamedMessageHandlers()
        {
            CustomMessagingManager.UnregisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_SERVER);
            CustomMessagingManager.UnregisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_ALL_CLIENTS);
            CustomMessagingManager.UnregisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_CLIENTS_EXCEPT_SELF);
        }

#region Callbacks

        private void MessageHandler_SendTextMessageToServer(ulong senderClientId, Stream dataStream)
        {
            using (PooledBitReader reader = PooledBitReader.Get(dataStream))
            {
                string message = reader.ReadStringPacked().ToString();
                _OnReceivedTextMessageSubject.OnNext(message);
            }
        }

        private void MessageHandler_SendTextMessageToAllClients(ulong senderClientId, Stream dataStream)
        {
            using (PooledBitReader reader = PooledBitReader.Get(dataStream))
            {
                string message = reader.ReadStringPacked().ToString();
                _OnReceivedTextMessageSubject.OnNext(message);

                // Server to Client
                using (PooledBitStream outputStream = PooledBitStream.Get())
                {
                    using (PooledBitWriter writer = PooledBitWriter.Get(outputStream))
                    {
                        writer.WriteUInt64Packed(senderClientId);
                        writer.WriteStringPacked(message);
                        _MLAPIServer.SendMessageToAllClients(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_ALL_CLIENTS, outputStream);
                    }
                }
            }
        }

        private void MessageHandler_SendTextMessageExceptSelf(ulong senderClientId, Stream dataStream)
        {
            using (PooledBitReader reader = PooledBitReader.Get(dataStream))
            {
                string message = reader.ReadStringPacked().ToString();
                _OnReceivedTextMessageSubject.OnNext(message);

                // Server to Client
                using (PooledBitStream outputStream = PooledBitStream.Get())
                {
                    using (PooledBitWriter writer = PooledBitWriter.Get(outputStream))
                    {
                        writer.WriteUInt64Packed(senderClientId);
                        writer.WriteStringPacked(message);
                        _MLAPIServer.SendMessageToAllClientsExcept(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_CLIENTS_EXCEPT_SELF, senderClientId, dataStream);
                    }
                }           
            }
        }

#endregion

    }
}
