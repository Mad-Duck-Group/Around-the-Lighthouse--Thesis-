using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Audio;
using Madduck.Fishing.Config;
using Madduck.Fishing.Shared;
using Madduck.Fishing.UI;
using Madduck.Input;
using Madduck.Shared;
using Madduck.Shared.Events;
using Madduck.Utils;
using MessagePipe;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.Controller
{
    public class ThrowHookController : IDisposable
    {
        #region Events

        public event Action OnThrowHookStarted;
        public event Action OnThrowHookCanceled;
        public event Action OnHookThrown;

        #endregion

        #region Fields

        private readonly ThrowHookConfig _config;
        private readonly ThrowHookCommander _commander;
        private readonly ThrowHookModel _model;
        private readonly BubbleManager _bubbleManager;
        private readonly InputInstructionManager _inputInstructionManager;
        private readonly FishingSharedVariable _fishingSharedVariable;
        private readonly IAudioManager _audioManager;
        private readonly IPlayerInputHandler _inputHandler;
        private readonly IHookFactory _hookFactory;
        private readonly ITransitionable _viewTransition;
        private readonly ISubscriber<BaitSelectionActivationEvent> _baitSelectionActivationSubscriber;
        
        private IDisposable _subscriptions;
        private IDisposable _bindings;
        private CancellationTokenSource _transitionCts = new();
        private AudioReference _fishingLineCastReference;
        private bool _baitSelectionActive;
        private bool _isActive;

        #endregion

        #region Injection

        [Inject]
        public ThrowHookController(
            ThrowHookConfig config,
            ThrowHookCommander commander,
            ThrowHookModel model,
            BubbleManager bubbleManager,
            InputInstructionManager inputInstructionManager,
            FishingSharedVariable fishingSharedVariable,
            IAudioManager audioManager,
            IPlayerInputHandler inputHandler,
            IHookFactory hookFactory,
            [Key(FishingStateType.ThrowHook)] ITransitionable viewTransition,
            ISubscriber<BaitSelectionActivationEvent> baitSelectionActivationSubscriber)
        {
            _config = config;
            _inputHandler = inputHandler;
            _commander = commander;
            _model = model;
            _bubbleManager = bubbleManager;
            _inputInstructionManager = inputInstructionManager;
            _fishingSharedVariable = fishingSharedVariable;
            _audioManager = audioManager;
            _hookFactory = hookFactory;
            _viewTransition = viewTransition;
            _baitSelectionActivationSubscriber = baitSelectionActivationSubscriber;
            Subscribe();
        }

        #endregion

        #region Bindings

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _baitSelectionActivationSubscriber
                .Subscribe(OnBaitSelectionActivationEvent)
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _inputHandler.Action0Button.IsDown
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .Where(x => x && !_baitSelectionActive)
                .Subscribe(_ => OnHookFirstHeld())
                .AddTo(ref disposableBuilder);
            _inputHandler.Action0Button.IsHeld
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .EveryUpdateWhen(x => x && !_model.HookThrown.Value)
                .Subscribe(_ => OnHookHeld())
                .AddTo(ref disposableBuilder);
            _inputHandler.Action0Button.IsUp
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ => OnHookRelease())
                .AddTo(ref disposableBuilder);
            _model.HookThrown
                .DistinctUntilChanged()
                .Where(x => x)
                .Subscribe(_ => OnThrownHook())
                .AddTo(ref disposableBuilder);
            _model.HookThrownFirstHeld
                .Where(x => !x)
                .Subscribe(_ => OnThrowHookCanceled?.Invoke())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _audioManager.StopAudio(_fishingLineCastReference);
            _subscriptions.Dispose();
            _bindings?.Dispose();
        }

        #endregion

        #region Event Handlers

        private void OnHookFirstHeld()
        {
            _commander.ThrowHookFirstHeldCommand.Execute(InputType.NonUI);
            OnThrowHookStarted?.Invoke();
        }

        private void OnHookHeld()
        {
            _commander.ThrowHookHeldCommand.Execute(InputType.NonUI);
        }
        
        private void OnHookRelease()
        {
            _commander.ThrowHookReleaseCommand.Execute(InputType.NonUI);
        }

        private void OnThrownHook()
        {
            _inputInstructionManager.Show(Array.Empty<InputInstruction>(), stream: 0);
            OnHookThrown?.Invoke();
            _fishingLineCastReference = _audioManager.PlayAudio(_config.FishingLineCastSfx, Vector3.zero);
        }
        
        private void OnBaitSelectionActivationEvent(BaitSelectionActivationEvent eventData)
        { 
            _baitSelectionActive = eventData.isActive;
        }

        #endregion

        #region Utils

        public async UniTask SetActive(bool active)
        {
            _bindings?.Dispose();
            _transitionCts.Cancel();
            _transitionCts = new CancellationTokenSource();
            if (active)
            {
                if (_isActive) return;
                _isActive = true;
                await _viewTransition.TransitionIn(cancellationToken: _transitionCts.Token);
                _inputInstructionManager.Show(_config.ThrowHookInputInstructions, stream: 0);
                Bind();
            }
            else
            {
                if (!_isActive) return;
                _isActive = false;
                await _viewTransition.TransitionOut(cancellationToken: _transitionCts.Token);
            }
        }
        
        public async UniTask ThrowHook()
        {
            var projectile = _hookFactory.Create();
            var throwPercent = _model.ThrowHookPercent.CurrentValue;
            await projectile.Throw(throwPercent);
            projectile.StartWave();
            _audioManager.PlayAudioOneShot(_config.HookHitWaterSfx, Vector3.zero);
            _audioManager.StopAudio(_fishingLineCastReference);
            if (_bubbleManager.TryLandOnBubble(_hookFactory.CurrentGameObject.transform.position, out var bubble))
            {
                _fishingSharedVariable.SetBubble(bubble);
                return;
            }
            _fishingSharedVariable.UnsetBubble();
        }
        
        public void Reset()
        {
            _model.Reset();
            _commander.Reset();
        }

        #endregion
    }
}