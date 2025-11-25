using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Audio;
using Madduck.Fishing.Config;
using Madduck.Fishing.Shared;
using Madduck.Fishing.UI;
using Madduck.GameData;
using Madduck.Input;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.Controller
{
    public class NibbleController : IDisposable
    {
        #region Events

        public event Action<Sign> OnPullHookResult;

        #endregion

        #region Fields

        private readonly NibbleConfig _config;
        private readonly NibbleModel _model;
        private readonly BubbleManager _bubbleManager;
        private readonly InputInstructionManager _inputInstructionManager;
        private readonly FishingSharedVariable _sharedVariable;
        private readonly IAudioManager _audioManager;
        private readonly IPlayerInputHandler _inputHandler;
        private readonly IHookFactory _hookFactory;
        private readonly IFactory<IFishableItemInstance> _fishableFactory;
        private readonly IFishSpriteFactory _fishSpriteFactory;
        private readonly IFishEyesFactory _fishEyesFactory;
        private readonly IFactory<IQuickTimeEvent> _qteButtonFactory;
        private readonly ITransitionable _viewTransition;
        private readonly ISpineAnimator<PlayerAnimationKey> _playerAnimator;
        private readonly IPublisher<FishEmergedEvent> _fishEmergedEventPublisher;
        
        private IDisposable _bindings;
        private IDisposable _qteIntervalTimer;
        private CancellationTokenSource _transitionCts = new();
        private CancellationTokenSource _fishBiteCts = new();
        private const string ThrowEventName = "After_Throw";
        private const string DestroyHookEventName = "Ending";
        private int _currentStageIndex;
        private bool _qteActive;
        private bool _fishBiting;
        private bool _hookPulled;

        private readonly Dictionary<int, Percentage> _currentStageChance = new()
        {
            { 0, Percentage.Zero },
            { 1, Percentage.Zero }
        };

        #endregion

        #region Injection

        [Inject]
        public NibbleController(
            NibbleConfig config,
            NibbleModel model, 
            BubbleManager bubbleManager,
            InputInstructionManager inputInstructionManager,
            FishingSharedVariable sharedVariable,
            IAudioManager audioManager,
            IPlayerInputHandler inputHandler,
            IHookFactory hookFactory,
            IFactory<IFishableItemInstance> fishableFactory,
            IFishSpriteFactory fishSpriteFactory,
            IFishEyesFactory fishEyesFactory,
            [Key(FishingStateType.Nibble)] IFactory<IQuickTimeEvent> qteButtonFactory,
            [Key(FishingStateType.Nibble)] ITransitionable viewTransition,
            ISpineAnimator<PlayerAnimationKey> playerAnimator,
            IPublisher<FishEmergedEvent> fishEmergedEventPublisher)
        {
            _config = config;
            _inputHandler = inputHandler;
            _model = model;
            _bubbleManager = bubbleManager;
            _inputInstructionManager = inputInstructionManager;
            _sharedVariable = sharedVariable;
            _audioManager = audioManager;
            _hookFactory = hookFactory;
            _fishableFactory = fishableFactory;
            _fishEyesFactory = fishEyesFactory;
            _qteButtonFactory = qteButtonFactory;
            _fishSpriteFactory = fishSpriteFactory;
            _viewTransition = viewTransition;
            _playerAnimator = playerAnimator;
            _fishEmergedEventPublisher = fishEmergedEventPublisher;
        }

        #endregion

        #region Bindings

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _inputHandler.Action0Button.IsDown
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .Where(x => x && !_qteActive && _fishBiting)
                .Subscribe(_ => OnPullHook())
                .AddTo(ref disposableBuilder);
            _inputHandler.Action1Button.IsDown
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .Where(x => x && !_qteActive && !_fishBiting)
                .Subscribe(_ => OnCancel())
                .AddTo(ref disposableBuilder);
            _model.PullHookResult
                .Where(x => x is not Sign.Zero && !_hookPulled)
                .SubscribeAwait((result, _) => OnPullHookResultChanged(result), AwaitOperation.Drop)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _qteIntervalTimer?.Dispose();
            _bindings?.Dispose();
        }

        #endregion

        #region Event Handlers

        private void OnPullHook()
        {
            _hookPulled = true;
            if (_sharedVariable.CurrentFishable is not FishItemInstance)
            {
                OnPullHookResultChanged(Sign.Zero).Forget();
                return;
            }
            OnPullHookResultChanged(Sign.Positive).Forget();
        }

        private void OnCancel()
        {
            OnPullHookResultChanged(Sign.Negative).Forget();
        }
        
        private async UniTask OnPullHookResultChanged(Sign result)
        {
            _inputInstructionManager.Show(Array.Empty<InputInstruction>(), stream: 0);
            TransitionOutFishEyes();
            _fishBiteCts.Cancel();
            _qteIntervalTimer?.Dispose();
            _hookFactory.Current.StopNibble();
            switch (result)
            {
                case Sign.Positive:
                {
                    var fishSprite = _fishSpriteFactory.Create();
                    fishSprite.SetUp(_hookFactory.CurrentGameObject.transform, _sharedVariable.CurrentFishable as FishItemInstance);
                    fishSprite.Animator.Set(FishSpriteAnimationKey.Idle, 0, true);
                    _hookFactory.Current.Alert(false).Forget();
                    _bubbleManager.PauseAllBubbles();
                    var currentFish = (FishItemInstance)_fishableFactory.Current;
                    _fishEmergedEventPublisher.Publish(new FishEmergedEvent(currentFish));
                    // await UniTask.WhenAll(
                    //     fishSprite.TransitionIn(),
                    //     _hookFactory.Current.MoveY(Percentage.Full));
                    _hookFactory.Current.StopWave();
                    await fishSprite.TransitionIn();
                    _audioManager.PlayAudioOneShot(_config.FishEmergedSfx, Vector3.zero);
                    await _hookFactory.Current.MoveY(Percentage.Full);
                    await _hookFactory.Current.MoveX(Percentage.Full);
                    break;
                }
                case Sign.Zero:
                    _hookFactory.Current.Alert(false).Forget();
                    _bubbleManager.PauseAllBubbles();
                    //await _hookFactory.Current.MoveY(Percentage.Full);
                    break;
            }
            OnPullHookResult?.Invoke(result);
        }

        #endregion

        #region Utils

        public void Reset()
        {
            _model.Reset();
        }

        public async UniTask ReturnHook()
        {
            _playerAnimator.Set(PlayerAnimationKey.Reeling, 0, true);
            var reelingSfx = _audioManager.PlayAudio(_config.ReelingSfx, Vector3.zero);
            await _hookFactory.Current.ReelBack();
            _audioManager.StopAudio(reelingSfx);
            _audioManager.PlayAudioOneShot(_config.PullHookSfx, Vector3.zero);
            var track = _playerAnimator.Set(PlayerAnimationKey.PullHookUp, 0, false);
            await track.WaitUntilEvent(ThrowEventName);
            await UniTask.WhenAny(_hookFactory.Current.Return(), 
                track.WaitUntilEvent(DestroyHookEventName));
            _hookFactory.DestroyHook();
            await track.WaitUntilComplete();
        }
        
        public async UniTask SetActive(bool active)
        {
            _bindings?.Dispose();
            _qteIntervalTimer?.Dispose();
            _transitionCts.Cancel();
            _fishBiteCts.Cancel();
            _transitionCts = new CancellationTokenSource();
            _fishBiteCts = new CancellationTokenSource();
            if (active)
            {
                await _viewTransition.TransitionIn(cancellationToken: _transitionCts.Token);
                _inputInstructionManager.Show(_config.CancelInputInstructions, stream: 0);
                _currentStageChance[0] = _model.FishingRod.CurrentStats.CurrentNibbleBaseSuccessChances[0];
                _currentStageChance[1] = _model.FishingRod.CurrentStats.CurrentNibbleBaseSuccessChances[1];
                _currentStageIndex = 0;
                _model.CatchChance.Value = _currentStageChance[0];
                _model.CatchStage.Value = (uint)_currentStageIndex;
                _fishBiting = false;
                _qteActive = false;
                _hookPulled = false;
                Bind();
                StartQteTimer();
            }
            else
            {
                await _viewTransition.TransitionOut(cancellationToken: _transitionCts.Token);
            }
        }

        private void TransitionOutFishEyes()
        {
            _fishEyesFactory.Current?.TransitionOut()
                .ContinueWith(() => _fishEyesFactory.DestroyFishEyes()).Forget();
        }

        #region QTE
        private void StartQteTimer()
        {
            _inputInstructionManager.Show(_config.CancelInputInstructions, stream: 0);
            _qteIntervalTimer = Observable.Timer(TimeSpan.FromSeconds(_config.QteIntervalRange.RandomBetweenRange()))
                .Subscribe(_ => NewQte());
        }

        private void NewQte()
        {
            var qte = _qteButtonFactory.Create();
            qte.OnSuccess += OnQteSuccess;
            qte.OnFail += OnQteFail;
            qte.TransitionInElement().ContinueWith(() =>
            {
                qte.StartQuickTimeEvent();
                _qteActive = true;
                _inputInstructionManager.Show(_config.QteInputInstructions, stream: 0);
            });
        }

        private void OnQteSuccess()
        {
            UniTask.WaitForEndOfFrame().ContinueWith(() =>
            {
                _qteActive = false;
            });
            _qteButtonFactory.Current.OnSuccess -= OnQteSuccess;
            _hookFactory.Current.Nibble(2);
            var bubbleType = _sharedVariable.CurrentBubbleType.CurrentValue;
            _currentStageChance[_currentStageIndex] +=
                _model.FishingRod.CurrentStats.CurrentBubbleNibbleBonuses[bubbleType];
            _currentStageChance[_currentStageIndex] = Percentage.Clamp01(_currentStageChance[_currentStageIndex]);
            _model.CatchChance.Value = _currentStageChance[_currentStageIndex];
            var result = Percentage.TryRoll(_currentStageChance[_currentStageIndex]);
            DebugUtils.Log($"Index {_currentStageIndex} Nibble Chance: {_currentStageChance[_currentStageIndex]} Roll Result: {result}");
            switch (_currentStageIndex)
            {
                case 0 when result:
                    DebugUtils.Log("Nibble Stage 1 Success");
                    var fishable = _fishableFactory.Create();
                    _sharedVariable.CurrentFishable = fishable;
                    switch (fishable)
                    {
                        case FishItemInstance fish:
                        {
                            DebugUtils.Log("Got Fish!");
                            _currentStageChance[_currentStageIndex] = _model.FishingRod.CurrentStats.CurrentNibbleBaseSuccessChances[0];
                            _currentStageIndex++;
                            _model.CatchStage.Value = (uint)_currentStageIndex;
                            _model.CatchChance.Value = _currentStageChance[_currentStageIndex];
                            _model.SetFishInstance(fish);
                            var fishEyes = _fishEyesFactory.Create();
                            fishEyes.SetUp(_hookFactory.CurrentGameObject.transform);
                            fishEyes.TransitionIn();
                            break;
                        }
                        case ResourceItemInstance resource:
                            DebugUtils.Log("Got Trash!");
                            _hookFactory.Current.Alert(true);
                            _audioManager.PlayAudioOneShot(_config.FishBiteSfx, Vector3.zero);
                            StartFishBiteTimer(_fishBiteCts.Token).Forget();
                            return;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                case 1 when result:
                    _fishEyesFactory.Current.Bite()
                        .ContinueWith(() =>
                    {
                        _fishEyesFactory.DestroyFishEyes();
                        _audioManager.PlayAudioOneShot(_config.FishBiteSfx, Vector3.zero);
                        _hookFactory.Current.Alert(true);
                        StartFishBiteTimer(_fishBiteCts.Token).Forget();
                    });
                    return;
            }
            StartQteTimer();
        }

        private void OnQteFail()
        {
            UniTask.WaitForEndOfFrame().ContinueWith(() =>
            {
                _qteActive = false;
            });
            _qteButtonFactory.Current.OnFail -= OnQteFail;
            var bubbleType = _sharedVariable.CurrentBubbleType.CurrentValue;
            _currentStageChance[_currentStageIndex] -=
                _model.FishingRod.CurrentStats.CurrentBubbleNibblePenalties[bubbleType];
            _currentStageChance[_currentStageIndex] = Percentage.Clamp01(_currentStageChance[_currentStageIndex]);
            _model.CatchChance.Value = _currentStageChance[_currentStageIndex];
            switch (_currentStageIndex)
            {
                case 0:
                    break;
                case 1 when _currentStageChance[_currentStageIndex] <= Percentage.Zero:
                    _currentStageChance[_currentStageIndex] = _model.FishingRod.CurrentStats.CurrentNibbleBaseSuccessChances[1];
                    _currentStageIndex--;
                    _model.CatchStage.Value = (uint)_currentStageIndex;
                    _model.CatchChance.Value = _currentStageChance[_currentStageIndex];
                    TransitionOutFishEyes();
                    break;
            }
            StartQteTimer();
        }
        #endregion

        #region Fish Bite

        private async UniTaskVoid StartFishBiteTimer(CancellationToken token)
        {
            _fishBiting = true;
            _inputInstructionManager.Show(_config.CatchInputInstructions, stream: 0);
            await UniTask.WaitForSeconds(_model.FishingRod.CurrentStats.CurrentFishBiteTimeFrame, cancellationToken: token);
            _fishBiting = false;
            DebugUtils.Log("Fish got away with the bait");
            _hookFactory.Current.Alert(false, token).Forget();
            OnPullHookResultChanged(Sign.Negative).Forget();
        }

        #endregion
        #endregion
    }
}