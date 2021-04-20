using System;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Shared
{
    public interface IGameRoomMessagingHub
    {
        void SendServerProcessDownCommand(int timeSeconds);
        void SendSystemUserIdToServer(string userId);
        void SendPlayerEjectionCommand(string userId);
    }

    public static class GameRoomMessagingHubConstants
    {
        public static readonly string SEND_SERVER_PROCESS_DOWN_COMMAND = "SendServerProcessDownCommand";
        public static readonly string SEND_SYSTEM_USERID_TO_SERVER = "SendSystemUserIdToServer";
        public static readonly string SEND_PLAYER_EJECTION_COMMAND = "SendPlayerEjectionCommand";
    }
}