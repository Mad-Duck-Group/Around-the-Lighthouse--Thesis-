using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Input;
using Madduck.Utils;
using R3;
using Redcode.Extensions;
using UnityEngine.InputSystem;
using VContainer;
using Time = UnityEngine.Time;

namespace Madduck.Shared
{
    public class QTEButtonController : IQuickTimeEvent, IDisposable
    {
        public event Action OnSuccess;
        public event Action OnFail;
        
        public ReadOnlyReactiveProperty<InputBinding> CurrentBinding { get; }
        public ReadOnlyReactiveProperty<Percentage> RemainingPercentage { get; }

        private readonly QTEButtonConfigInstance _configInstance;
        private readonly IPlayerInputHandler _input;
        private readonly IQTEButtonView _view;
        private readonly ReactiveProperty<InputBinding> _currentBinding = new();
        private readonly ReactiveProperty<Percentage> _remainingPercentage = new();
        private IDisposable _bindings;
        private IDisposable _timer;
        private CancellationTokenSource _cts = new();
        private bool _timeFrameOpen;
        private bool _active = true;
        private float _currentTime;

        [Inject]
        public QTEButtonController(
            QTEButtonConfigInstance configInstance,
            IPlayerInputHandler inputHandler,
            IQTEButtonView view)
        {
            _configInstance = configInstance;
            _input = inputHandler;
            _view = view;
            CurrentBinding = _currentBinding.ToReadOnlyReactiveProperty();
            RemainingPercentage = _remainingPercentage.ToReadOnlyReactiveProperty();
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _input.JerkBaitButton.IsDown
                .IgnoreFirstValueWhenSubscribe()
                .Where(x => x && _active)
                .Select(_ => _input.JerkBaitButton)
                .Subscribe(OnJerkBaitButtonDown)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
            _timer?.Dispose();
        }

        public void StartQuickTimeEvent()
        {
            _currentBinding.Value = _input.JerkBindings.GetRandomElement();
            _cts = new CancellationTokenSource();
            StartQuickTimeEventInternal(_cts.Token).Forget();
        }
        
        private async UniTaskVoid StartQuickTimeEventInternal(CancellationToken cancellationToken)
        {
            _timeFrameOpen = false;
            _remainingPercentage.Value = Percentage.Zero;
            await _view.TransitionIn(cancellationToken);
            await UniTask.WaitForSeconds(_configInstance.CurrentStartDelay, cancellationToken: cancellationToken);
            _timer = Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    _currentTime += Time.deltaTime;
                    _remainingPercentage.Value = Percentage.FromFraction(_currentTime / _configInstance.CurrentClosingInDuration);
                });
            var halfTimeFrame = (float)_configInstance.CurrentSuccessTimeFrame / 2f;
            await UniTask.WaitForSeconds(_configInstance.CurrentClosingInDuration - halfTimeFrame, cancellationToken: cancellationToken);
            _timeFrameOpen = true;
            await UniTask.WaitForSeconds(_configInstance.CurrentSuccessTimeFrame, cancellationToken: cancellationToken);
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

        private void OnJerkBaitButtonDown(InputButton button)
        {
            if (!_timeFrameOpen)
            {
                Fail();
                return;
            }
            if (button.InputBinding.HasValue && button.InputBinding.Value == _currentBinding.Value)
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
            _cts.Cancel();
            _timer?.Dispose();
            ChangeViewResult(true).Forget();
            OnSuccess?.Invoke();
            _active = false;
        }

        private void Fail()
        {
            _cts.Cancel();
            _timer?.Dispose();
            ChangeViewResult(false).Forget();
            OnFail?.Invoke();
            _active = false;
        }

        private async UniTaskVoid ChangeViewResult(bool result)
        {
            if (result)
                await _view.OnSuccess();
            else
                await _view.OnFail();
            await _view.TransitionOut();
            _view.Destroy();
        }
    }
}