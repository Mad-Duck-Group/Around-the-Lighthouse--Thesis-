using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using VContainer;

namespace Madduck.GameData
{
    public class ItemPopUpHandler : IDisposable
    {
        [Title("Debug")] 
        [ShowInInspector] private readonly Dictionary<Type, IPopUpManager> _popUpManagers = new();
        
        private readonly ISubscriber<FishCaughtEvent> _fishCaughtEventSubscriber;
        
        private IDisposable _subscriptions;
        private IDisposable _popUpHiddenSubscription;
        private readonly Stack<IPopUpObject> _popUpStack = new(); 
        private CancellationTokenSource _popUpCts = new();
        
        [Inject]
        public ItemPopUpHandler(
            FishItemPopUpManager fishItemPopUpManager,
            ISubscriber<FishCaughtEvent> fishCaughtEventSubscriber)
        {
            _popUpManagers.Add(typeof(FishItemPopUpObject), fishItemPopUpManager);
            _fishCaughtEventSubscriber = fishCaughtEventSubscriber;
            Subscribe();
        }
        
        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _fishCaughtEventSubscriber.Subscribe(OnFishCaught)
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }

        public void Dispose()
        {
            _subscriptions.Dispose();
        }

        private void OnFishCaught(FishCaughtEvent fishCaughtEvent)
        {
            var fishItemPopUpObject = new FishItemPopUpObject(fishCaughtEvent.FishItemInstance);
            _popUpStack.Push(fishItemPopUpObject);
            ShowNextPopUp();
        }

        private void ShowNextPopUp()
        {
            _popUpHiddenSubscription?.Dispose();
            if (_popUpStack.Count == 0) return;
            var popUpObject = _popUpStack.Pop();
            _popUpCts.Cancel();
            _popUpCts = new CancellationTokenSource();
            var type = popUpObject.GetType();
            if (!_popUpManagers.TryGetValue(type, out var manager))
            {
                DebugUtils.LogError($"No manager found for type {type}");
                return;
            }
            _popUpHiddenSubscription = Observable.FromEvent(
                    h => manager.OnPopUpHidden += h,
                    h => manager.OnPopUpHidden -= h)
                .Subscribe(_ => ShowNextPopUp());
            manager.ShowPopUp(popUpObject, _popUpCts.Token).Forget();
        }
    }
}