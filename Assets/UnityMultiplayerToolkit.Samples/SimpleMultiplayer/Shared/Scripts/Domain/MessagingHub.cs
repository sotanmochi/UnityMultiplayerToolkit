using System;
using System.IO;
using UnityEngine;
using UniRx;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using UnityMultiplayerToolkit.MLAPIExtension;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Shared
{
    [AddComponentMenu("MLAPI Extension/SimpleMultiplayer/MessagingHub")]
    public class MessagingHub : MonoBehaviour
    {
        [SerializeField] MonoBehaviour _NetworkingManagerExtension;

        public IObservable<(ulong senderId, string message)> OnReceivedTextMessageAsObservable() => _OnReceivedTextMessageSubject;
        private Subject<(ulong senderId, string message)> _OnReceivedTextMessageSubject = new Subject<(ulong senderId, string message)>();

        private MLAPIServer _Server;
        private MLAPIClient _Client;

        void Awake()
        {
            INetworkingManagerExtension networkingManagerExtension = _NetworkingManagerExtension.GetComponent<INetworkingManagerExtension>();
            if (networkingManagerExtension != null)
            {
                if (networkingManagerExtension.IsServer)
                {
                    _Server = _NetworkingManagerExtension.GetComponent<MLAPIServer>();
                    RegisterNamedMessageHandlers();
                }
                else if (networkingManagerExtension.IsClient)
                {
                    _Client = _NetworkingManagerExtension.GetComponent<MLAPIClient>();
                    RegisterNamedMessageHandlers();
                }
                else
                {
                    Debug.LogError("The type of NetworkingManagerExtension is unknown.");
                }
            }
            else
            {
                Debug.LogError("NetworkingManagerExtension is null.");
            }
        }

        void OnDestroy()
        {
            UnregisterNamedMessageHandlers();
        }

        void RegisterNamedMessageHandlers()
        {
            if (_Server != null)
            {
                CustomMessagingManager.RegisterNamedMessageHandler(MessagingHubConstants.SEND_SERVER_PROCESS_DOWN_COMMAND, MessageHandler_Server_SendServerProcessDownCommand);
                CustomMessagingManager.RegisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_SERVER, MessageHandler_Server_SendTextMessageToServer);
                CustomMessagingManager.RegisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_ALL_CLIENTS, MessageHandler_Server_SendTextMessageToAllClients);
                CustomMessagingManager.RegisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_CLIENTS_EXCEPT_SELF, MessageHandler_Server_SendTextMessageExceptSelf);
            }
            else
            if (_Client != null)
            {
                CustomMessagingManager.RegisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_ALL_CLIENTS, MessageHandler_Client_SendTextMessageToAllClients);
                CustomMessagingManager.RegisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_CLIENTS_EXCEPT_SELF, MessageHandler_Client_SendTextMessageExceptSelf);
            }
        }

        void UnregisterNamedMessageHandlers()
        {
            CustomMessagingManager.UnregisterNamedMessageHandler(MessagingHubConstants.SEND_SERVER_PROCESS_DOWN_COMMAND);
            CustomMessagingManager.UnregisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_SERVER);
            CustomMessagingManager.UnregisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_ALL_CLIENTS);
            CustomMessagingManager.UnregisterNamedMessageHandler(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_CLIENTS_EXCEPT_SELF);
        }

#region Message Handlers for Server

        private void MessageHandler_Server_SendServerProcessDownCommand(ulong senderClientId, Stream dataStream)
        {
            using (PooledBitReader reader = PooledBitReader.Get(dataStream))
            {
                int timeSeconds = reader.ReadInt32Packed();
                _Server.ApplicationQuit(timeSeconds);
            }
        }

        private void MessageHandler_Server_SendTextMessageToServer(ulong senderClientId, Stream dataStream)
        {
            using (PooledBitReader reader = PooledBitReader.Get(dataStream))
            {
                string message = reader.ReadStringPacked().ToString();
                _OnReceivedTextMessageSubject.OnNext((senderClientId, message));
            }
        }

        private void MessageHandler_Server_SendTextMessageToAllClients(ulong senderClientId, Stream dataStream)
        {
            using (PooledBitReader reader = PooledBitReader.Get(dataStream))
            {
                string message = reader.ReadStringPacked().ToString();
                _OnReceivedTextMessageSubject.OnNext((senderClientId, message));

                // Server to Client
                using (PooledBitStream outputStream = PooledBitStream.Get())
                {
                    using (PooledBitWriter writer = PooledBitWriter.Get(outputStream))
                    {
                        writer.WriteUInt64Packed(senderClientId);
                        writer.WriteStringPacked(message);
                        _Server.SendMessageToAllClients(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_ALL_CLIENTS, outputStream);
                    }
                }
            }
        }

        private void MessageHandler_Server_SendTextMessageExceptSelf(ulong senderClientId, Stream dataStream)
        {
            using (PooledBitReader reader = PooledBitReader.Get(dataStream))
            {
                string message = reader.ReadStringPacked().ToString();
                _OnReceivedTextMessageSubject.OnNext((senderClientId, message));

                // Server to Client
                using (PooledBitStream outputStream = PooledBitStream.Get())
                {
                    using (PooledBitWriter writer = PooledBitWriter.Get(outputStream))
                    {
                        writer.WriteUInt64Packed(senderClientId);
                        writer.WriteStringPacked(message);
                        _Server.SendMessageToAllClientsExcept(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_CLIENTS_EXCEPT_SELF, senderClientId, dataStream);
                    }
                }           
            }
        }

#endregion

#region Message Handlers for Client

        private void MessageHandler_Client_SendTextMessageToAllClients(ulong sender, Stream dataStream)
        {
            using (PooledBitReader reader = PooledBitReader.Get(dataStream))
            {
                ulong senderClientId = reader.ReadUInt64Packed();
                string message = reader.ReadStringPacked().ToString();
                _OnReceivedTextMessageSubject.OnNext((senderClientId, message));
            }
        }

        private void MessageHandler_Client_SendTextMessageExceptSelf(ulong sender, Stream dataStream)
        {
            using (PooledBitReader reader = PooledBitReader.Get(dataStream))
            {
                ulong senderClientId = reader.ReadUInt64Packed();
                string message = reader.ReadStringPacked().ToString();
                _OnReceivedTextMessageSubject.OnNext((senderClientId, message));
            }
        }

#endregion

#region Client Method

        public void SendServerProcessDownCommand(int timeSeconds)
        {
            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    writer.WriteInt32Packed(timeSeconds);
                    _Client.SendMessageToServer(MessagingHubConstants.SEND_SERVER_PROCESS_DOWN_COMMAND, stream);
                }
            }
        }

        public void SendTextMessageToServer(string message)
        {
            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    writer.WriteStringPacked(message);
                    _Client.SendMessageToServer(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_SERVER, stream);
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
                    _Client.SendMessageToServer(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_ALL_CLIENTS, stream);
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
                    _Client.SendMessageToServer(MessagingHubConstants.SEND_TEXT_MESSAGE_TO_CLIENTS_EXCEPT_SELF, stream);
                }
            }
        }

#endregion

    }
}