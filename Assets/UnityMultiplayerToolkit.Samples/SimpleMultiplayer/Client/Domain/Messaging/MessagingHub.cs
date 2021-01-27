using System;
using System.IO;
using UnityEngine;
using UniRx;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using UnityMultiplayerToolkit.MLAPIExtension;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Client
{
    [AddComponentMenu("MLAPI Extension/SimpleMultiplayer/Client_MessagingHub")]
    public class MessagingHub : MonoBehaviour
    {
        [SerializeField] MLAPIClient _MLAPIClient;

        public IObservable<(ulong senderId, string message)> OnReceivedTextMessageAsObservable() => _OnReceivedTextMessageSubject;
        private Subject<(ulong senderId, string message)> _OnReceivedTextMessageSubject = new Subject<(ulong senderId, string message)>();

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
            CustomMessagingManager.RegisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_ALL_CLIENTS, MessageHandler_SendTextMessageToAllClients);
            CustomMessagingManager.RegisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_CLIENTS_EXCEPT_SELF, MessageHandler_SendTextMessageExceptSelf);
        }

        void UnregisterNamedMessageHandlers()
        {
            CustomMessagingManager.UnregisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_ALL_CLIENTS);
            CustomMessagingManager.UnregisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_CLIENTS_EXCEPT_SELF);
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

        public void SendTextMessageToAllClients(string message)
        {
            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    writer.WriteStringPacked(message);
                    _MLAPIClient.SendMessageToServer(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_ALL_CLIENTS, stream);
                }
            }
        }

        public void SendTextMessageToClientsExceptSelf(string message)
        {
            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    writer.WriteStringPacked(message);
                    _MLAPIClient.SendMessageToServer(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_CLIENTS_EXCEPT_SELF, stream);
                }
            }
        }

#endregion

#region Receivers

        private void MessageHandler_SendTextMessageToAllClients(ulong sender, Stream dataStream)
        {
            using (PooledBitReader reader = PooledBitReader.Get(dataStream))
            {
                ulong senderClientId = reader.ReadUInt64Packed();
                string message = reader.ReadStringPacked().ToString();
                _OnReceivedTextMessageSubject.OnNext((senderClientId, message));
            }
        }

        private void MessageHandler_SendTextMessageExceptSelf(ulong sender, Stream dataStream)
        {
            using (PooledBitReader reader = PooledBitReader.Get(dataStream))
            {
                ulong senderClientId = reader.ReadUInt64Packed();
                string message = reader.ReadStringPacked().ToString();
                _OnReceivedTextMessageSubject.OnNext((senderClientId, message));
            }
        }

#endregion

    }
}
