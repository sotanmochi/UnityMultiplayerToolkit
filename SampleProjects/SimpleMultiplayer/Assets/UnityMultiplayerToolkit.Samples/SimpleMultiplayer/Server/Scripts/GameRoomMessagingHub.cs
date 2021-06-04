using System;
using System.IO;
using UnityEngine;
using UniRx;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using UnityMultiplayerToolkit.MLAPIExtension;
using UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Shared;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Server
{
    public class GameRoomMessagingHub : MonoBehaviour
    {
        [SerializeField] NetworkServer _Server;

        public IObservable<MLAPIExtension.NetworkClientUser> OnReceivedPlayerEjectionAsObservable() => _OnReceivedPlayerEjectionSubject;
        private Subject<MLAPIExtension.NetworkClientUser> _OnReceivedPlayerEjectionSubject = new Subject<MLAPIExtension.NetworkClientUser>();

        public IObservable<MLAPIExtension.NetworkClientUser> OnReceivedSystemUserIdAsObservable() => _OnReceivedSystemUserIdSubject;
        private Subject<MLAPIExtension.NetworkClientUser> _OnReceivedSystemUserIdSubject = new Subject<MLAPIExtension.NetworkClientUser>();

        void Awake()
        {
            CustomMessagingManager.RegisterNamedMessageHandler(GameRoomMessagingHubConstants.SEND_SERVER_PROCESS_DOWN_COMMAND, MessageHandler_Server_SendServerProcessDownCommand);       
            CustomMessagingManager.RegisterNamedMessageHandler(GameRoomMessagingHubConstants.SEND_SYSTEM_USERID_TO_SERVER, MessageHandler_Server_SendSystemUserIdToServer);
            CustomMessagingManager.RegisterNamedMessageHandler(GameRoomMessagingHubConstants.SEND_PLAYER_EJECTION_COMMAND, MessageHandler_Server_SendPlayerEjectionCommand);
        }

        void OnDestroy()
        {
            CustomMessagingManager.UnregisterNamedMessageHandler(GameRoomMessagingHubConstants.SEND_SERVER_PROCESS_DOWN_COMMAND);
            CustomMessagingManager.UnregisterNamedMessageHandler(GameRoomMessagingHubConstants.SEND_SYSTEM_USERID_TO_SERVER);
            CustomMessagingManager.UnregisterNamedMessageHandler(GameRoomMessagingHubConstants.SEND_PLAYER_EJECTION_COMMAND);
        }

        private void MessageHandler_Server_SendServerProcessDownCommand(ulong senderClientId, Stream dataStream)
        {
            using (PooledNetworkReader reader = PooledNetworkReader.Get(dataStream))
            {
                int timeSeconds = reader.ReadInt32Packed();
                _Server.ApplicationQuit(timeSeconds);
            }
        }

        private void MessageHandler_Server_SendSystemUserIdToServer(ulong senderClientId, Stream dataStream)
        {
            using (PooledNetworkReader reader = PooledNetworkReader.Get(dataStream))
            {
               string userId = reader.ReadStringPacked().ToString();

                MLAPIExtension.NetworkClientUser networkClientUser = new MLAPIExtension.NetworkClientUser();
                networkClientUser.ClientId = senderClientId;
                networkClientUser.UserId = userId;

                _OnReceivedSystemUserIdSubject.OnNext(networkClientUser);
            }
        }

        private void MessageHandler_Server_SendPlayerEjectionCommand(ulong senderClientId, Stream dataStream)
        {
            using (PooledNetworkReader reader = PooledNetworkReader.Get(dataStream))
            {
               string userId = reader.ReadStringPacked().ToString();

                MLAPIExtension.NetworkClientUser networkClientUser = new MLAPIExtension.NetworkClientUser();
                networkClientUser.ClientId = 0;
                networkClientUser.UserId = userId;

                _OnReceivedSystemUserIdSubject.OnNext(networkClientUser);
            }
        }
    }
}