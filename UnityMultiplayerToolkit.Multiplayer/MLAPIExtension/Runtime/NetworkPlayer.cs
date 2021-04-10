using System;
using UnityMultiplayerToolkit.Shared;

namespace UnityMultiplayerToolkit.MLAPIExtension
{
    public class NetworkPlayer : IPlayer
    {
        public NetworkPlayer(){}
        public string UserId { get; set; }
        public ulong ClientId { get; set; }
    }
}