using System;
using Cysharp.Threading.Tasks;
using Madduck.Audio;
using Madduck.Fishing.Config;
using Madduck.Fishing.Shared;
using Madduck.GameData;
using Madduck.Input;
using Madduck.Shared;
using Madduck.Utils;
using PrimeTween;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.Controller
{
    public class CatchFishController : IDisposable
    {
        public event Action OnCatchFishCompleted;
        
        private readonly CatchFishConfig _config;
        private readonly IAudioManager _audioManager;
        private readonly IPlayerInputHandler _inputHandler;
        private readonly IHookFactory _hookFactory;
        private readonly IGenericFactory<FishItemInstance> _fishFactory;
        private readonly IGenericFactory<IQuickTimeEvent> _qteButtonFactory;
        private readonly IFishSpriteFactory _fishSpriteFactory;
        private readonly ISpineAnimator<PlayerAnimationKey> _playerAnimator;
        
        private IDisposable _bindings;
        private IDisposable _startSlowMo;
        private DisposableBag _qteSubscription;
        private Sequence _slowMoSequence;
        private bool _isCatchingFish;
        private const string ThrowEventName = "After_Throw";
        
        
        [Inject]
        public CatchFishController(
            CatchFishConfig config,
            IAudioManager audioManager,
            IPlayerInputHandler inputHandler,
            IHookFactory hookFactory,
            IGenericFactory<FishItemInstance> fishFactory,
            [Key(FishingStateType.CatchFish)] IGenericFactory<IQuickTimeEvent> qteButtonFactory,
            IFishSpriteFactory fishSpriteFactory,
            ISpineAnimator<PlayerAnimationKey> playerAnimator)
        {
            _config = config;
            _audioManager = audioManager;
            _inputHandler = inputHandler;
            _hookFactory = hookFactory;
            _fishFactory = fishFactory;
            _qteButtonFactory = qteButtonFactory;
            _fishSpriteFactory = fishSpriteFactory;
            _playerAnimator = playerAnimator;
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _inputHandler.Action0Button.IsDown
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .Where(x => x && !_isCatchingFish)
                .Subscribe(_ => OnPullHookUp())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings?.Dispose();
            _startSlowMo?.Dispose();
            _qteSubscription.Dispose();
            _slowMoSequence.Complete();
            Time.timeScale = 1f;
        }

        private void OnPullHookUp()
        {
            //TODO: Check for fish type
            _isCatchingFish = true;
            ReturnHook().Forget();
        }
        
        public void SetActive(bool active)
        {
            _bindings?.Dispose();
            if (active)
            {
                Bind();
            }
        }

        public void Reset()
        {
            _isCatchingFish = false;
            Time.timeScale = 1f;  
        }
        
        private async UniTask ReturnHook()
        {
            var currentFish = _fishFactory.Current;
            var isBoss = currentFish.ItemData.EnemyType is FishEnemyType.Boss;
            var sprite = _fishSpriteFactory.Current;
            if (isBoss) sprite.Detach();
            await _playerAnimator.Set(PlayerAnimationKey.GotFish, 0, false).WaitUntilEvent(ThrowEventName);
            _audioManager.PlayAudioOneShot(_config.PullHookUpSfx, Vector3.zero);
            var hook = _hookFactory.Current;
            _startSlowMo = Observable.FromEvent<Percentage>(
                    h => hook.OnDramaticReturnProgress += h,
                    h => hook.OnDramaticReturnProgress -= h)
                .Where(x => x >= _config.SlowMoThreshold)
                .Subscribe(_ =>
                {
                    _startSlowMo.Dispose();
                    StartSlowMo().Forget();
                });
            await UniTask.WhenAll(
                hook.DramaticReturn(),
                isBoss ? sprite.TransitionOut() : UniTask.CompletedTask);
            if (!isBoss)
            {
                sprite.Detach();
                _hookFactory.DestroyHook();
                OnCatchFishCompleted?.Invoke();
                await sprite.TransitionOut();
                _fishSpriteFactory.DestroyFishSprite();
                return;
            }
            _hookFactory.DestroyHook();
            OnCatchFishCompleted?.Invoke();
        }

        private async UniTaskVoid StartSlowMo()
        {
            var qte = _qteButtonFactory.Create();
            var tcs = new UniTaskCompletionSource();
            qte.StartQuickTimeEvent();
            _qteSubscription = new DisposableBag();
            Observable.FromEvent(
                    h => qte.OnSuccess += h,
                    h => qte.OnSuccess -= h)
                .Subscribe(_ =>
                {
                    var currentFish = _fishFactory.Current;
                    currentFish.UpgradeFishQuality();
                    tcs.TrySetResult();
                })
                .AddTo(ref _qteSubscription);
            Observable.FromEvent(
                    h => qte.OnFail += h,
                    h => qte.OnFail -= h)
                .Subscribe(_ =>
                {
                    var currentFish = _fishFactory.Current;
                    currentFish.DowngradeFishQuality();
                    tcs.TrySetResult();
                })
                .AddTo(ref _qteSubscription);
            var settings = _config.SlowMoSettings;   
            _slowMoSequence = Sequence.Create(useUnscaledTime: true)   
                .Group(Tween.GlobalTimeScale(settings));
            await tcs.Task;
            _qteSubscription.Dispose();
            _slowMoSequence = Sequence.Create(useUnscaledTime: true)                              
                .Group(Tween.GlobalTimeScale(settings.WithDirection(false)));
        }
    }
}