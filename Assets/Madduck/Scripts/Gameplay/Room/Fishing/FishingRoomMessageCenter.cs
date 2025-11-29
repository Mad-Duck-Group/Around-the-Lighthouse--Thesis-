using System;
using Madduck.Core;
using Madduck.GameData;
using Madduck.Utils;
using MessagePipe;
using R3;
using VContainer;

namespace Madduck.Room
{
    public class FishingRoomMessageCenter : IDisposable
    {
        private readonly FishingRoomManager _fishingRoomManager;
        private readonly ISubscriber<FishEmergedEvent> _fishEmergedEventSubscriber;
        private readonly ISubscriber<FishEscapedEvent> _fishEscapedEventSubscriber;
        private readonly ISubscriber<FishableCaughtEvent> _fishCaughtEventSubscriber;
        private readonly ISubscriber<LoadSceneStageEvent> _loadSceneStageEventSubscriber;
        
        private IDisposable _subscriptions;

        [Inject]
        public FishingRoomMessageCenter(
            FishingRoomManager fishingRoomManager,
            ISubscriber<FishEmergedEvent> fishEmergedEventSubscriber,
            ISubscriber<FishEscapedEvent> fishEscapedEventSubscriber,
            ISubscriber<FishableCaughtEvent> fishCaughtEventSubscriber,
            ISubscriber<LoadSceneStageEvent> loadSceneStageEventSubscriber)
        {
            _fishingRoomManager = fishingRoomManager;
            _fishEmergedEventSubscriber = fishEmergedEventSubscriber;
            _fishEscapedEventSubscriber = fishEscapedEventSubscriber;
            _fishCaughtEventSubscriber = fishCaughtEventSubscriber;
            _loadSceneStageEventSubscriber = loadSceneStageEventSubscriber;
            Subscribe();
        }
        
        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _fishEmergedEventSubscriber
                .Subscribe(OnFishEmergedEvent)
                .AddTo(ref disposableBuilder);
            _fishEscapedEventSubscriber
                .Subscribe(OnFishEscapedEvent)
                .AddTo(ref disposableBuilder);
            _fishCaughtEventSubscriber
                .Subscribe(OnFishCaughtEvent)
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
        
        private void OnFishEmergedEvent(FishEmergedEvent eventData)
        {
            _fishingRoomManager.FishEmerged(eventData);
        }
        private void OnFishEscapedEvent(FishEscapedEvent eventData)
        {
            _fishingRoomManager.FishEscaped(eventData);
        }
        private void OnFishCaughtEvent(FishableCaughtEvent eventData)
        {
            _fishingRoomManager.FishCaught(eventData);
        }

        private void OnLoadSceneStageEvent(LoadSceneStageEvent eventData)
        {
            switch (eventData.Stage)
            {
                case LoadSceneStage.StartFadeOut:
                    _fishingRoomManager.OnSceneFadeOut();
                    break;
                case LoadSceneStage.FinishLoading:
                    _fishingRoomManager.StartFishingRoom();
                    break;
            }
        }
    }
}