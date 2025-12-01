using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Madduck.Core;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using VContainer;
using VContainer.Unity;

namespace Madduck.Fishing.StateMachine
{
    [Serializable]
    public class FishingStateMachine : Utils.StateMachine, IDisposable
    {
        [Title("Debug")]
        [DisplayAsString]
        [ShowInInspector] private FishingStateType _currentStateType = FishingStateType.None;
        [ReadOnly]
        [ShowInInspector] private Dictionary<FishingStateType, FishingState> _states = new();
        [Button("Test Next State")]
        private void TestNextState() => NextState();
        [Button("Test Previous State")]
        private void TestPreviousState() => PreviousState();
        
        private readonly ISubscriber<FishingRoomStartedEvent> _fishingRoomStartedEventSubscriber;
        private readonly ISubscriber<LoadSceneStageEvent> _loadSceneStageEventSubscriber;
        private readonly IPublisher<FishingStateEvent> _fishingStateEventPublisher;
        
        private IDisposable _subscriptions;

        [Inject]
        public FishingStateMachine(
            ISubscriber<FishingRoomStartedEvent> fishingRoomStartedEventSubscriber,
            ISubscriber<LoadSceneStageEvent> loadSceneStageEventSubscriber,
            IPublisher<FishingStateEvent> fishingStateEventPublisher)
        {
            _fishingRoomStartedEventSubscriber = fishingRoomStartedEventSubscriber;
            _loadSceneStageEventSubscriber = loadSceneStageEventSubscriber;
            _fishingStateEventPublisher = fishingStateEventPublisher;
            Subscribe();
        }

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _fishingRoomStartedEventSubscriber
                .Subscribe(_ => StartStateMachine())
                .AddTo(ref disposableBuilder);
            _loadSceneStageEventSubscriber
                .Subscribe(OnLoadSceneStageEvent)
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }
        
        private void StartStateMachine()
        {
            ChangeState(FishingStateType.None);
        }
        
        public void AddState(FishingStateType stateType, FishingState state)
        {
            if (!_states.TryAdd(stateType, state))
            {
                DebugUtils.LogWarning($"State {stateType} already exists in FishingStateMachine.");
            }
        }

        public void NextState()
        {
            var nextStateType = _currentStateType + 1;
            var maxStateType = EnumUtils.Max<FishingStateType>();
            if (nextStateType > maxStateType)
            {
                DebugUtils.LogWarning("Already in the last state, cannot go to next state.");
                return;
            }
            ChangeState(nextStateType);
        }
        
        public void PreviousState()
        {
            var previousStateType = _currentStateType - 1;
            var minStateType = EnumUtils.Min<FishingStateType>();
            if (previousStateType < minStateType)
            {
                DebugUtils.LogWarning("Already in the first state, cannot go to previous state.");
                return;
            }
            ChangeState(previousStateType);
        }
        
        public void ChangeState(FishingStateType stateType)
        {
            if (_states.TryGetValue(stateType, out var nextState))
            {
                _currentStateType = stateType;
                ChangeState(nextState).ContinueWith(() =>
                {
                    DebugUtils.Log("FishingStateMachine changed to state: " + stateType);
                    _fishingStateEventPublisher.Publish(new FishingStateEvent(stateType));
                });
            }
            else
            {
                DebugUtils.LogError($"State {stateType} does not exist in FishingStateMachine.");
            }
        }

        public void ResetState(FishingStateType stateType)
        {
            if (_states.TryGetValue(stateType, out var state))
            {
                state.Reset();
            }
            else
            {
                DebugUtils.LogError($"State {stateType} does not exist in FishingStateMachine.");
            }
        }

        private void OnLoadSceneStageEvent(LoadSceneStageEvent evt)
        {
            if (evt.Stage is not LoadSceneStage.StartFadeOut) return;
            ChangeState(FishingStateType.None);
        }
    }
}