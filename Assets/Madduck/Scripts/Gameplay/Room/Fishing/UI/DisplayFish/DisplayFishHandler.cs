using System;
using Madduck.GameData;
using MessagePipe;
using R3;
using VContainer;

namespace Madduck.Room.DisplayFish
{
    public class DisplayFishHandler : IDisposable
    {
        private readonly ISubscriber<FishCaughtEvent> _fishCaughtSubscriber;
        private IDisposable _subscriptions;
        
        [Inject]
        public DisplayFishHandler(
            ISubscriber<FishCaughtEvent> fishCaughtSubscriber)
        {
            _fishCaughtSubscriber = fishCaughtSubscriber;
            Subscribe();
        }

        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _fishCaughtSubscriber.Subscribe(OnFishCaught)
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _subscriptions.Dispose();
        }

        private void OnFishCaught(FishCaughtEvent eventData)
        {
            
        }
    }
}