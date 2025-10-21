using System;
using Cysharp.Threading.Tasks;
using Madduck.Fishing.Controller;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;

namespace Madduck.Fishing.StateMachine
{
    /// <summary>
    /// State of the Fishing Board mini-game.
    /// </summary>
    [Serializable]
    public class FishingBoardState : FishingState
    {
        private readonly FishingBoardController _controller;
        private readonly IPublisher<FishEscapedEvent> _fishEscapedEventPublisher;
        private IDisposable _fishingBoardResultSubscription;
        private Sign _result;
        
        public FishingBoardState(
            FishingStateMachine stateMachine,
            FishingBoardController controller,
            IPublisher<FishEscapedEvent> fishEscapedEventPublisher)
            : base(stateMachine)
        {
            _controller = controller;
            _fishEscapedEventPublisher = fishEscapedEventPublisher;
        }
        
        public override async UniTask Enter()
        {
            await base.Enter();
            await _controller.SetActive(true);
            _fishingBoardResultSubscription = Observable.FromEvent<Sign>(
                    h => _controller.OnFishingBoardResult += h,
                    h => _controller.OnFishingBoardResult -= h)
                .Subscribe(OnFishingBoardResult);
        }

        public override async UniTask Exit()
        {
            await base.Exit();
            _fishingBoardResultSubscription.Dispose();
            await _controller.SetActive(false);
            if (_result is Sign.Negative)
                await _controller.ReturnHook();
            _controller.Reset();
            _controller.ResetCircleBoardSprite();
        }

        public override void Reset()
        {
            base.Reset();
            _controller.Reset();
            _controller.ResetCircleBoardSprite();
        }

        private void OnFishingBoardResult(Sign result)
        {
            _result = result;
            switch (result)
            {
                case Sign.Negative:
                    DebugUtils.Log("Fish escaped, transitioning to NoneState");
                    stateMachine.ChangeState(FishingStateType.None);
                    _fishEscapedEventPublisher.Publish(new FishEscapedEvent());
                    break;
                case Sign.Positive:
                    DebugUtils.Log("Fish is tired, transitioning to ReelingState");
                    stateMachine.NextState();
                    break;
                case Sign.Zero:
                default:
                    DebugUtils.LogError($"Unexpected FishingBoardResult value: {result}");
                    break;
            }
        }
    }
}