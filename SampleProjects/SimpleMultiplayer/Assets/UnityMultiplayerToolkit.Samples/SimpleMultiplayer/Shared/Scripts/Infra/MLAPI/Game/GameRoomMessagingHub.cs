using UnityEngine;
using MLAPI.Serialization.Pooled;
using UnityMultiplayerToolkit.MLAPIExtension;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Shared.Infra
{
    public class GameRoomMessagingHub : MonoBehaviour, IGameRoomMessagingHub
    {
        [SerializeField] NetworkClient _Client;

        public void SendServerProcessDownCommand(int timeSeconds)
        {
            using (PooledNetworkBuffer stream = PooledNetworkBuffer.Get())
            {
                using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
                {
                    writer.WriteInt32Packed(timeSeconds);
                    _Client.SendMessageToServer(GameRoomMessagingHubConstants.SEND_SERVER_PROCESS_DOWN_COMMAND, stream);
                }
            }
        }

        public void SendSystemUserIdToServer(string userId)
        {
            using (PooledNetworkBuffer stream = PooledNetworkBuffer.Get())
            {
                using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
                {
                    writer.WriteStringPacked(userId);
                    _Client.SendMessageToServer(GameRoomMessagingHubConstants.SEND_SYSTEM_USERID_TO_SERVER, stream);
                }
            }
        }

        public void SendPlayerEjectionCommand(string userId)
        {
            using (PooledNetworkBuffer stream = PooledNetworkBuffer.Get())
            {
                using (PooledNetworkWriter writer = PooledNetworkWriter.Get(stream))
                {
                    writer.WriteStringPacked(userId);
                    _Client.SendMessageToServer(GameRoomMessagingHubConstants.SEND_PLAYER_EJECTION_COMMAND, stream);
                }
            }
        }
    }
}
