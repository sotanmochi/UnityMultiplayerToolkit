using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Shared;
using UnityMultiplayerToolkit.MLAPIExtension;
// using UnityMultiplayerToolkit.Multiplayer.MLAPIExtension;
using UnityMultiplayerToolkit.Infra.AWS.GameLift;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer
{
    public class GameRoomContext : UnityEngine.MonoBehaviour
    {
        [UnityEngine.SerializeField] bool _IsLocalServer;
        [UnityEngine.SerializeField] bool _AutoInitializeOnAwake;
        [UnityEngine.SerializeField] MessagingHub _MessagingHub;
        [UnityEngine.SerializeField] NetworkServer _NetworkServer;
        [UnityEngine.SerializeField] GameLiftServer _GameLiftServer;

        private Dictionary<string, NetworkClientUser> _NetworkClientUsers = new Dictionary<string, NetworkClientUser>();

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
                UnityEngine.Debug.LogError("GameRoomContext can not be initialized.");
                return;
            }

            _MessagingHub = messagingHub;
            _NetworkServer = networkServer;
            _GameLiftServer = gameLiftServer;

            _MessagingHub.OnReceivedSystemUserIdAsObservable()
            .Subscribe(networkClientUser => 
            {
                bool contained = _NetworkClientUsers.ContainsKey(networkClientUser.UserId);

                bool accepted = true;
                if (!_IsLocalServer)
                {
                    accepted = _GameLiftServer.AcceptPlayerSession(networkClientUser.UserId);
                }

                if (!contained && accepted)
                {
                    _NetworkClientUsers.Add(networkClientUser.UserId, networkClientUser);
                }
                else
                {
                    _NetworkServer.DisconnectClient(networkClientUser.ClientId, "Cannot accept system user id");
                }
            })
            .AddTo(this);

            _MessagingHub.OnReceivedPlayerEjectionAsObservable()
            .Subscribe(player =>
            {
                string userId = player.UserId;
                if (_NetworkClientUsers.TryGetValue(userId, out NetworkClientUser networkClientUser))
                {
                    _NetworkClientUsers.Remove(userId);
                    _GameLiftServer.RemovePlayerSession(networkClientUser.UserId);
                    _NetworkServer.DisconnectClient(networkClientUser.ClientId, "PlayerEjection");
                }
            })
            .AddTo(this);

            _NetworkServer.OnClientDisconnectedAsObservable()
            .Subscribe(clientId => 
            {
                var networkClientUser = _NetworkClientUsers.Select(kv => kv.Value).FirstOrDefault(v => v.ClientId == clientId);
                if (networkClientUser != null)
                {
                    _NetworkClientUsers.Remove(networkClientUser.UserId);
                    _GameLiftServer.RemovePlayerSession(networkClientUser.UserId);
                }
            })
            .AddTo(this);
        }
    }
}