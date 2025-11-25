using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Audio;
using Madduck.Fishing.Config;
using Madduck.Fishing.Shared;
using Madduck.Fishing.UI;
using Madduck.GameData;
using Madduck.Input;
using Madduck.Scripts.Input;
using Madduck.Shared;
using Madduck.Utils;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.Controller
{
    public class ThrowHookController : IDisposable
    {
        #region Events

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
        
        private IDisposable _bindings;
        private CancellationTokenSource _transitionCts = new();
        private AudioReference _fishingLineCastReference;

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
            [Key(FishingStateType.ThrowHook)] ITransitionable viewTransition)
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
        }

        #endregion

        #region Bindings

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _inputHandler.Action0Button.IsDown
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .Where(x => x)
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
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings?.Dispose();
        }

        #endregion

        #region Event Handlers

        private void OnHookFirstHeld()
        {
            _commander.ThrowHookFirstHeldCommand.Execute(InputType.NonUI);
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

        #endregion

        #region Utils

        public async UniTask SetActive(bool active)
        {
            _bindings?.Dispose();
            _transitionCts.Cancel();
            _transitionCts = new CancellationTokenSource();
            if (active)
            {
                await _viewTransition.TransitionIn(cancellationToken: _transitionCts.Token);
                _inputInstructionManager.Show(_config.ThrowHookInputInstructions, stream: 0);
                Bind();
            }
            else
            {
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