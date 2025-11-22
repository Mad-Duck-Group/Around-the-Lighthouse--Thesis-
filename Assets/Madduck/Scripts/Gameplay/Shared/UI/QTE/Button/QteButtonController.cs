using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Audio;
using Madduck.Input;
using Madduck.Utils;
using R3;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using Time = UnityEngine.Time;

namespace Madduck.Shared
{
    [Serializable]
    public class QteButtonController : IQuickTimeEvent, IDisposable
    {
        public event Action OnSuccess;
        public event Action OnFail;
        public IQteElement CurrentElement { get; }
        public bool ChangeViewResultManually { get; set; } = false;

        public bool DestroyWhenFinished { get; set; } = true;
        
        public ReadOnlyReactiveProperty<string> CurrentControlScheme { get; }
        public ReadOnlyReactiveProperty<InputBinding> CurrentBinding { get; }
        public ReadOnlyReactiveProperty<Percentage> TimeFramePercentage { get;  }
        public ReadOnlyReactiveProperty<Percentage> RemainingPercentage { get; }

        private readonly QteButtonConfigInstance _configInstance;
        private readonly IAudioManager _audioManager;
        private readonly IPlayerInputHandler _input;
        private readonly IQteElement _view;
        private readonly ReactiveProperty<InputBinding> _currentBinding = new();
        private readonly ReactiveProperty<Percentage> _remainingPercentage = new();
        private readonly ReactiveProperty<Percentage> _timeFramePercentage = new();
        private IDisposable _bindings;
        private IDisposable _timer;
        private CancellationTokenSource _cts = new();
        private bool _timeFrameOpen;
        private bool _transitionedIn;
        [ShowInInspector] private bool _active = true;
        private float _currentTime;

        [Inject]
        public QteButtonController(
            QteButtonConfigInstance configInstance,
            IAudioManager audioManager,
            IPlayerInputHandler inputHandler,
            IQteElement view)
        {
            _configInstance = configInstance;
            _audioManager = audioManager;
            _input = inputHandler;
            _view = view;
            CurrentElement = view;
            CurrentBinding = _currentBinding.ToReadOnlyReactiveProperty();
            RemainingPercentage = _remainingPercentage.ToReadOnlyReactiveProperty();
            TimeFramePercentage = _timeFramePercentage.ToReadOnlyReactiveProperty();
            CurrentControlScheme = Observable.FromEvent<string>(
                    h => _input.OnControlSchemeChanged += h,
                    h => _input.OnControlSchemeChanged -= h)
                .ToReadOnlyReactiveProperty(_input.CurrentControlScheme);
            ChangeInputActiveState(true);
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _input.JerkBaitButton.IsDown
                .IgnoreFirstValueWhenSubscribe()
                .DistinctUntilChanged()
                .Where(x => x && _active)
                .Select(_ => _input.JerkBaitButton)
                .Subscribe(OnQteButtonDown)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
            _timer?.Dispose();
        }

        public async UniTask TransitionInElement(CancellationToken cancellationToken = default)
        {
            if (_transitionedIn) return;
            _transitionedIn = true;
            _currentBinding.Value = _input.JerkBindings.GetRandomElement();
            _remainingPercentage.Value = Percentage.Zero;
            var duration = (float)_configInstance.CurrentClosingInDuration;
            var timeFrame = (float)_configInstance.CurrentSuccessTimeFrame;
            _timeFramePercentage.Value = Percentage.Clamp01(Percentage.FromFraction(timeFrame / duration));
            await _view.TransitionIn(cancellationToken);
        }

        public void StartQuickTimeEvent()
        {
            _cts = new CancellationTokenSource();
            StartQuickTimeEventInternal(_cts.Token).Forget();
        }
        
        private async UniTaskVoid StartQuickTimeEventInternal(CancellationToken cancellationToken)
        {
            _timeFrameOpen = false;
            _remainingPercentage.Value = Percentage.Zero;
            var duration = (float)_configInstance.CurrentClosingInDuration;
            var earlyTimeFrame = (float)_configInstance.CurrentEarlySuccessTimeFrame;
            var timeFrame = (float)_configInstance.CurrentSuccessTimeFrame;
            var lateTimeFrame = (float)_configInstance.CurrentLateSuccessTimeFrame;
            _timeFramePercentage.Value = Percentage.Clamp01(Percentage.FromFraction(timeFrame / duration));
            if (!_transitionedIn)
            {
                await TransitionInElement(cancellationToken);
            }
            await UniTask.WaitForSeconds(_configInstance.CurrentStartDelay, ignoreTimeScale: true, 
                cancellationToken: cancellationToken);
            _timer = Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    _currentTime += Time.unscaledDeltaTime;
                    _remainingPercentage.Value = Percentage.FromFraction(_currentTime / duration);
                });
            await UniTask.WaitForSeconds(duration - (timeFrame + earlyTimeFrame), ignoreTimeScale: true, 
                cancellationToken: cancellationToken);
            _timeFrameOpen = true;
            await UniTask.WaitForSeconds(earlyTimeFrame + timeFrame + lateTimeFrame, ignoreTimeScale: true, 
                cancellationToken: cancellationToken);
            _timeFrameOpen = false;
            _remainingPercentage.Value = Percentage.Full;
            _timer.Dispose();
            Fail();
        }

        public void CancelQuickTimeEvent(bool success)
        {
            _cts.Cancel();
            _timer?.Dispose();
            if (success)
            {
                Success();
            }
            else
            {
                Fail();
            }
        }
        
        private void OnQteButtonDown(InputButton button)
        {
            _audioManager.PlayAudioOneShot(_configInstance.BaseConfig.QtePressSfx, Vector3.zero);
            if (!_timeFrameOpen)
            {
                Fail();
                return;
            }
            if (button.InputBinding.HasValue)
                DebugUtils.Log(
                button.InputBinding.Value.name);
            if (button.InputBinding.HasValue && button.InputBinding.Value.name == _currentBinding.Value.name)
            {
                Success();
            }
            else
            {
                Fail();
            }
        }

        private void Success()
        {
            DebugUtils.Log($"Success {_currentBinding.Value.ToDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions)}");
            _cts.Cancel();
            _timer?.Dispose();
            if (!ChangeViewResultManually) ChangeViewResult(true).Forget();
            OnSuccess?.Invoke();
            _active = false;
        }

        private void Fail()
        {
            DebugUtils.Log($"Fail {_currentBinding.Value.ToDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions)}");
            _cts.Cancel();
            _timer?.Dispose();
            if (!ChangeViewResultManually) ChangeViewResult(false).Forget();
            OnFail?.Invoke();
            _active = false;
        }

        public async UniTask ChangeViewResult(bool result, CancellationToken cancellationToken = default)
        {
            if (result)
                await _view.OnSuccess(cancellationToken);
            else
                await _view.OnFail(cancellationToken);
            await _view.TransitionOut(cancellationToken);
            if (DestroyWhenFinished) _view.Destroy();
        }
        
        public void ChangeInputActiveState(bool active)
        {
            _active = active;
            _bindings?.Dispose();
            Bind();
        }
    }
}