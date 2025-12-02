using System;
using Cysharp.Threading.Tasks;
using Madduck.Fishing.Controller;
using Madduck.Fishing.Shared;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using VContainer;

namespace Madduck.Fishing.StateMachine
{
    public class CatchFishState : FishingState
    {
        private readonly CatchFishController _controller;
        private readonly FishingSharedVariable _sharedVariable;
        private readonly IPublisher<FishableCaughtEvent> _fishCaughtEventPublisher;
        
        private IDisposable _catchFishSubscription;
        
        [Inject]
        public CatchFishState(
            FishingStateMachine stateMachine,
            CatchFishController controller,
            FishingSharedVariable sharedVariable,
            IPublisher<FishableCaughtEvent> fishCaughtEventPublisher)
            : base(stateMachine)
        {
            _controller = controller;
            _sharedVariable = sharedVariable;
            _fishCaughtEventPublisher = fishCaughtEventPublisher;
        }
        
        public override async UniTask Enter()
        {
            await base.Enter();
            _controller.SetActive(true);
            _catchFishSubscription = Observable.FromEvent(
                    h => _controller.OnCatchFishCompleted += h,
                    h => _controller.OnCatchFishCompleted -= h)
                .Subscribe(_ => OnCatchFishCompleted());
        }
        
        public override async UniTask Exit()
        {
            await base.Exit();
            _catchFishSubscription?.Dispose();
            _controller.SetActive(false);
            _controller.Reset();
        }

        private void OnCatchFishCompleted()
        {
            DebugUtils.Log("Catch fish completed, transitioning to NoneState");
            _fishCaughtEventPublisher.Publish(new FishableCaughtEvent(_sharedVariable.CurrentFishable));
            stateMachine.ChangeState(FishingStateType.None);
            stateMachine.ResetState(FishingStateType.FishingBoard);
            stateMachine.ResetState(FishingStateType.Reeling);
        }
    }
}