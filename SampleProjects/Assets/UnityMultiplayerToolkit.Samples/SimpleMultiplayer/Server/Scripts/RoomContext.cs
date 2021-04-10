using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Shared;
using UnityMultiplayerToolkit.MLAPIExtension;
// using UnityMultiplayerToolkit.Multiplayer.MLAPIExtension;
using UnityMultiplayerToolkit.Infra.AWS.GameLift;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer
{
    public class RoomContext : UnityEngine.MonoBehaviour
    {
        [UnityEngine.SerializeField] bool _AutoInitializeOnAwake;
        [UnityEngine.SerializeField] MessagingHub _MessagingHub;
        [UnityEngine.SerializeField] NetworkServer _NetworkServer;
        [UnityEngine.SerializeField] GameLiftServer _GameLiftServer;

        private Dictionary<string, NetworkPlayer> _NetworkPlayers = new Dictionary<string, NetworkPlayer>();

        void Awake()
        {
            if (_AutoInitializeOnAwake)
            {
                Initialize(_MessagingHub, _NetworkServer, _GameLiftServer);
            }
        }

        public void Initialize(MessagingHub messagingHub, NetworkServer networkServer, GameLiftServer gameLiftServer)
        {
            if (messagingHub == null || networkServer == null || gameLiftServer == null)
            {
                UnityEngine.Debug.LogError("RoomManager can not be initialized.");
                return;
            }

            _MessagingHub = messagingHub;
            _NetworkServer = networkServer;
            _GameLiftServer = gameLiftServer;

            _MessagingHub.OnReceivedSystemUserIdAsObservable()
            .Subscribe(networkPlayer => 
            {
                bool contained = _NetworkPlayers.ContainsKey(networkPlayer.UserId);
                bool accepted = _GameLiftServer.AcceptPlayerSession(networkPlayer.UserId);
                if (!contained && accepted)
                {
                    _NetworkPlayers.Add(networkPlayer.UserId, networkPlayer);
                }
                else
                {
                    _NetworkServer.DisconnectClient(networkPlayer.ClientId);
                }
            })
            .AddTo(this);

            _MessagingHub.OnReceivedPlayerEjectionAsObservable()
            .Subscribe(player =>
            {
                string userId = player.UserId;
                if (_NetworkPlayers.TryGetValue(userId, out NetworkPlayer networkPlayer))
                {
                    _NetworkPlayers.Remove(userId);
                    _GameLiftServer.RemovePlayerSession(networkPlayer.UserId);
                    _NetworkServer.DisconnectClient(networkPlayer.ClientId);
                }
            })
            .AddTo(this);

            _NetworkServer.OnClientDisconnectedAsObservable()
            .Subscribe(clientId => 
            {
                var networkPlayer = _NetworkPlayers.Select(kv => kv.Value).FirstOrDefault(v => v.ClientId == clientId);
                if (networkPlayer != null)
                {
                    _NetworkPlayers.Remove(networkPlayer.UserId);
                    _GameLiftServer.RemovePlayerSession(networkPlayer.UserId);
                }
            })
            .AddTo(this);
        }
    }
}