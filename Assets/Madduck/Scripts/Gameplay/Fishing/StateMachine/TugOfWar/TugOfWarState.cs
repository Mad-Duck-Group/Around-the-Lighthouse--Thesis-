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
    public class TugOfWarState : FishingState
    {
        private readonly TugOfWarController _controller;
        private readonly IFactory<IFishableItemInstance> _fishableFactory;
        private readonly IPublisher<FishEscapedEvent> _fishEscapedEventPublisher;
        
        private IDisposable _subscription;
        private Sign _result;
        
        [Inject]
        public TugOfWarState(
            FishingStateMachine stateMachine,
            TugOfWarController controller,
            IFactory<IFishableItemInstance> fishableFactory,
            IPublisher<FishEscapedEvent> fishEscapedEventPublisher)
            : base(stateMachine)
        {
            _controller = controller;
            _fishEscapedEventPublisher = fishEscapedEventPublisher;
            _fishableFactory = fishableFactory;
        }

        public override async UniTask Enter()
        {
            await base.Enter();
            await _controller.SetActive(true);
            _subscription = Observable.FromEvent<Sign>(
                    h => _controller.OnTugOfWarResult += h,
                    h => _controller.OnTugOfWarResult -= h)
                .Subscribe(OnTugOfWarResult);
        }

        public override async UniTask Exit()
        {
            await base.Exit();
            _subscription.Dispose();
            await _controller.SetActive(false);
            if (_result is Sign.Negative)
            {
                await _controller.ReturnHook();
                
            }
            _controller.Reset();
        }
        
        private void OnTugOfWarResult(Sign result)
        {
            _result = result;
            switch (result)
            {
                case Sign.Positive:
                    DebugUtils.Log("Won Tug of War, back to FishingBoardState");
                    stateMachine.ChangeState(FishingStateType.FishingBoard);
                    break;
                case Sign.Negative:
                    DebugUtils.Log("Fish got away, back to NoneState");
                    _fishEscapedEventPublisher.Publish(new FishEscapedEvent(_fishableFactory.Current as FishItemInstance));
                    stateMachine.ChangeState(FishingStateType.None);
                    stateMachine.ResetState(FishingStateType.FishingBoard);
                    stateMachine.ResetState(FishingStateType.Reeling);
                    break;
                case Sign.Zero:
                    DebugUtils.Log("Lose Tug of War, back to FishingBoardState");
                    stateMachine.ChangeState(FishingStateType.FishingBoard);
                    break;
                default:
                    DebugUtils.LogError($"Unexpected TugOfWarResult value: {result}");
                    break;
            }
        }
    }
}