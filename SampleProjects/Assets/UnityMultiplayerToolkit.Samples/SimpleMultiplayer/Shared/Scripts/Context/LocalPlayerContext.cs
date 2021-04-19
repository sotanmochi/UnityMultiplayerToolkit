using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;
using UnityMultiplayerToolkit.Utility;

namespace UnityMultiplayerToolkit.Samples.SimpleMultiplayer.Shared
{
    public class LocalPlayerContext : MonoBehaviour
    {
        [SerializeField] GameObject _PlayerManagerObject;
        [SerializeField] GameObject _KeyInputProviderObject;
        [SerializeField] FollowCamera _FollowCamera;

        private IPlayerManager _PlayerManager;
        private IPlayer _LocalPlayer;
        private IKeyInputProvider _KeyInputProvider;

        private CompositeDisposable _Disposable;

        void Awake()
        {
            _PlayerManager = _PlayerManagerObject.GetComponent<IPlayerManager>();
            _KeyInputProvider = _KeyInputProviderObject.GetComponent<IKeyInputProvider>();

            _PlayerManager.OnSpawnedLocalPlayerAsObservable()
            .Subscribe(player => 
            {
                player.SetFollowCamera(_FollowCamera.gameObject);
                _LocalPlayer = player;
                _FollowCamera.SetFollowTarget(_LocalPlayer.Transform);
            })
            .AddTo(this);

            _PlayerManager.OnDespawnedLocalPlayerAsObservable()
            .Subscribe(_ => 
            {
                Debug.Log("DespawnedLocalPlayer");
                _LocalPlayer = null;
            })
            .AddTo(this);
        }

        void OnEnable()
        {
            _Disposable = new CompositeDisposable();

            _KeyInputProvider.KeyInputAsObservable(KeyCode.W)
            .Where(_ => !EventSystem.current.IsPointerOverGameObject())
            .Subscribe(_ => 
            {
                if (_LocalPlayer != null)
                {
                    _LocalPlayer.Move(0, 0, 1);
                }
            })
            .AddTo(_Disposable);

            _KeyInputProvider.KeyInputAsObservable(KeyCode.A)
            .Where(_ => !EventSystem.current.IsPointerOverGameObject())
            .Subscribe(_ => 
            {
                if (_LocalPlayer != null)
                {
                    _LocalPlayer.Move(-1, 0, 0);
                }
            })
            .AddTo(_Disposable);

            _KeyInputProvider.KeyInputAsObservable(KeyCode.S)
            .Where(_ => !EventSystem.current.IsPointerOverGameObject())
            .Subscribe(_ => 
            {
                if (_LocalPlayer != null)
                {
                    _LocalPlayer.Move(0, 0, -1);
                }
            })
            .AddTo(_Disposable);

            _KeyInputProvider.KeyInputAsObservable(KeyCode.D)
            .Where(_ => !EventSystem.current.IsPointerOverGameObject())
            .Subscribe(_ => 
            {
                if (_LocalPlayer != null)
                {
                    _LocalPlayer.Move(1, 0, 0);
                }
            })
            .AddTo(_Disposable);
        }

        void OnDisable()
        {
            _Disposable?.Dispose();
            _Disposable = null;
        }
    }
}
