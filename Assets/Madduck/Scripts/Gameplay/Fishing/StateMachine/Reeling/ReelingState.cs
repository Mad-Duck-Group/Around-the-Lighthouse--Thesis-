using System;
using Cysharp.Threading.Tasks;
using Madduck.Fishing.Controller;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using VContainer;

namespace Madduck.Fishing.StateMachine
{
    public class ReelingState : FishingState
    {
        private readonly ReelingController _controller;
        private readonly IGenericFactory<FishItemInstance> _fishFactory;
        private readonly IPublisher<FishCaughtEvent> _fishCaughtEventPublisher;
        private readonly IPublisher<FishEscapedEvent> _fishEscapedEventPublisher;
        private IDisposable _reelingResultSubscription;
        private Sign _result;
        
        [Inject]
        public ReelingState(
            FishingStateMachine stateMachine,
            ReelingController controller,
            IGenericFactory<FishItemInstance> fishFactory,
            IPublisher<FishCaughtEvent> fishCaughtEventPublisher,
            IPublisher<FishEscapedEvent> fishEscapedEventPublisher)
            : base(stateMachine)
        {
            _controller = controller;
            _fishFactory = fishFactory;
            _fishCaughtEventPublisher = fishCaughtEventPublisher;
            _fishEscapedEventPublisher = fishEscapedEventPublisher;
        }
        
        public override async UniTask Enter()
        {
            await base.Enter();
            await _controller.SetActive(true);
            _reelingResultSubscription = Observable.FromEvent<Sign>(
                    h => _controller.OnReelingResult += h,
                    h => _controller.OnReelingResult -= h)
                .Subscribe(OnReelingResult);
        }
        
        public override async UniTask Exit()
        {
            await base.Exit();
            _reelingResultSubscription.Dispose();
            await _controller.SetActive(false);
            await _controller.ReturnHook();
            if (_result is Sign.Positive)
            {
                _fishCaughtEventPublisher.Publish(new FishCaughtEvent(_fishFactory.Current));
                _controller.Reset();
            }
        }
        
        private void OnReelingResult(Sign result)
        {
            _result = result;
            switch (result)
            {
                case Sign.Positive:
                    DebugUtils.Log("Fish reeled in successfully, transitioning to NoneState");
                    stateMachine.ChangeState(FishingStateType.None);
                    stateMachine.ResetState(FishingStateType.FishingBoard);
                    break;
                case Sign.Negative:
                    DebugUtils.Log("Max fatigue attempt reached, fish escaped, transitioning to NoneState");
                    stateMachine.ChangeState(FishingStateType.None);
                    stateMachine.ResetState(FishingStateType.FishingBoard);
                    _fishEscapedEventPublisher.Publish(new FishEscapedEvent());
                    break;
                case Sign.Zero:
                    DebugUtils.Log("Fish regained energy, transitioning to FishingBoardState");
                    stateMachine.PreviousState();
                    break;
                default:
                    DebugUtils.LogError($"Unexpected ReelingResult value: {result}");
                    break;
            }
        }
    }
}