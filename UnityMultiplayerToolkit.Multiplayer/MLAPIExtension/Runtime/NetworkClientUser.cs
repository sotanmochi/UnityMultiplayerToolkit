using System;
using UnityMultiplayerToolkit.Shared;

namespace UnityMultiplayerToolkit.MLAPIExtension
{
    public class NetworkClientUser
    {
        public NetworkClientUser(){}
        public string UserId { get; set; }
        public ulong ClientId { get; set; }
    }
}