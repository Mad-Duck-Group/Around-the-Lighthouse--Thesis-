using System;
using System.Threading;
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
        private readonly FishingSharedVariable _sharedVariable;
        private readonly InputInstructionManager _inputInstructionManager;
        private readonly IAudioManager _audioManager;
        private readonly IPlayerInputHandler _inputHandler;
        private readonly IHookFactory _hookFactory;
        private readonly IFactory<IQuickTimeEvent> _qteButtonFactory;
        private readonly IFishSpriteFactory _fishSpriteFactory;
        private readonly ISpineAnimator<PlayerAnimationKey> _playerAnimator;

        private IDisposable _bindings;
        private IDisposable _startSlowMo;
        private IDisposable _slowMoTimer;
        private float _slowMoProgress;
        private DisposableBag _qteSubscription;
        private CancellationTokenSource _slowMoCts = new();
        private bool _isCatching;
        private const string ThrowEventName = "After_Throw";


        [Inject]
        public CatchFishController(
            CatchFishConfig config,
            FishingSharedVariable sharedVariable,
            InputInstructionManager inputInstructionManager,
            IAudioManager audioManager,
            IPlayerInputHandler inputHandler,
            IHookFactory hookFactory,
            [Key(FishingStateType.CatchFish)] IFactory<IQuickTimeEvent> qteButtonFactory,
            IFishSpriteFactory fishSpriteFactory,
            ISpineAnimator<PlayerAnimationKey> playerAnimator)
        {
            _config = config;
            _sharedVariable = sharedVariable;
            _inputInstructionManager = inputInstructionManager;
            _audioManager = audioManager;
            _inputHandler = inputHandler;
            _hookFactory = hookFactory;
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
                .Where(x => x && !_isCatching)
                .Subscribe(_ => OnPullHookUp())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        public void Dispose()
        {
            _bindings?.Dispose();
            _startSlowMo?.Dispose();
            _qteSubscription.Dispose();
            _slowMoCts.Cancel();
            _slowMoTimer?.Dispose();
            Time.timeScale = 1f;
        }

        private void OnPullHookUp()
        {
            _isCatching = true;
            DramaticReturn().Forget();
        }

        public void SetActive(bool active)
        {
            _bindings?.Dispose();
            if (active)
            {
                Bind();
                _isCatching = true; //NOTE: Auto catch for all
                if (_sharedVariable.CurrentFishable is FishItemInstance)
                {
                    DramaticReturn().Forget();
                    return;
                }

                Return().Forget();
            }
            else
            {
                _slowMoCts.Cancel();
                _slowMoCts = new CancellationTokenSource();
                _slowMoTimer?.Dispose();
                Time.timeScale = 1f;
                _inputInstructionManager.Show(Array.Empty<InputInstruction>(), stream: 0);
            }
        }

        public void Reset()
        {
            _isCatching = false;
            _slowMoCts.Cancel();
            _slowMoCts = new CancellationTokenSource();
            _slowMoTimer?.Dispose();
            Time.timeScale = 1f;
        }

        private async UniTask Return()
        {
            await _playerAnimator.Set(PlayerAnimationKey.GotFish, 0, false).WaitUntilEvent(ThrowEventName);
            _audioManager.PlayAudioOneShot(_config.PullHookUpSfx, Vector3.zero);
            var hook = _hookFactory.Current;
            await hook.Return();
            _hookFactory.DestroyHook();
            OnCatchFishCompleted?.Invoke();
        }

        private async UniTask DramaticReturn()
        {
            if (_sharedVariable.CurrentFishable is not FishItemInstance currentFish)
            {
                DebugUtils.LogError("Current fish is null or not a FishItemInstance!");
                return;
            }

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
                    StartSlowMo(_slowMoCts.Token).Forget();
                });
            if (isBoss)
            {
                sprite.TransitionOut().ContinueWith(() => { _fishSpriteFactory.DestroyFishSprite(); }).Forget();
            }

            await hook.DramaticReturn();
            if (!isBoss)
            {
                sprite.Detach();
                _hookFactory.DestroyHook();
                OnCatchFishCompleted?.Invoke();
                _audioManager.PlayAudioOneShot(_config.FishFlopSfx, Vector3.zero);
                await sprite.FadeOut();
                _fishSpriteFactory.DestroyFishSprite();
                return;
            }

            _hookFactory.DestroyHook();
            OnCatchFishCompleted?.Invoke();
        }

        private async UniTask StartSlowMo(CancellationToken cancellationToken)
        {
            var qte = _qteButtonFactory.Create();
            var tcs = new UniTaskCompletionSource();
            await qte.TransitionInElement(cancellationToken);
            _inputInstructionManager.Show(_config.QteInputInstructions, stream: 0);
            qte.StartQuickTimeEvent();
            SubscribeQte(qte, tcs); 
            var settings = _config.SlowMoSettings;
            CreateSlowMoTimer(settings.startValue, settings.endValue);
            await tcs.Task;
            _qteSubscription.Dispose();
            _slowMoTimer.Dispose();
            if (cancellationToken.IsCancellationRequested) return;
            CreateSlowMoTimer(Time.timeScale, 1f);
        }

        private void SubscribeQte(IQuickTimeEvent qte, UniTaskCompletionSource tcs) 
        {
            _qteSubscription = new DisposableBag();
            Observable.FromEvent(
                    h => qte.OnSuccess += h,
                    h => qte.OnSuccess -= h)
                .Subscribe(_ =>
                {
                    if (_sharedVariable.CurrentFishable is not FishItemInstance currentFish)
                    {
                        DebugUtils.LogError("Current fish is null or not a FishItemInstance!");
                        return;
                    }
                    currentFish.UpgradeFishQuality();
                    tcs.TrySetResult();
                })
                .AddTo(ref _qteSubscription);
            Observable.FromEvent(
                    h => qte.OnFail += h,
                    h => qte.OnFail -= h)
                .Subscribe(_ =>
                {
                    if (_sharedVariable.CurrentFishable is not FishItemInstance currentFish)
                    {
                        DebugUtils.LogError("Current fish is null or not a FishItemInstance!");
                        return;
                    }
                    currentFish.DowngradeFishQuality();
                    tcs.TrySetResult();
                })
                .AddTo(ref _qteSubscription);
        }

        private void CreateSlowMoTimer(float start, float end)
        {
            var settings = _config.SlowMoSettings;
            _slowMoTimer = Observable.EveryUpdate(UnityFrameProvider.Update)
                .Where(_ => GameConstants.CurrentGameState.CurrentValue is not GameState.Paused)
                .Subscribe(_ =>
                {
                    _slowMoProgress += Time.unscaledDeltaTime;
                    Time.timeScale = Mathf.Lerp(start, end,
                        Easing.Evaluate(Mathf.Clamp01(_slowMoProgress / settings.settings.duration),
                            settings.settings.ease));
                    if (_slowMoProgress < settings.settings.duration) return;
                    _slowMoProgress = 0f;
                    _slowMoTimer.Dispose();
                });
        }
    }
}