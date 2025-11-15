using System;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.Shared
{
    public class FishingSharedVariable : IDisposable
    {
        private readonly IHookFactory _hookFactory;
        private readonly BubbleManager _bubbleManager;
        
        public IFishableItemInstance CurrentFishable { get; set; }
        public ReadOnlyReactiveProperty<BubbleType> CurrentBubbleType { get; }
        private readonly ReactiveProperty<IBubbleView> _currentBubbleView = new(null);
        private IDisposable _subscriptions;
        
        [Inject]
        public FishingSharedVariable(
            IHookFactory hookFactory,
            BubbleManager bubbleManager)
        {
            _hookFactory = hookFactory;
            _bubbleManager = bubbleManager;
            CurrentBubbleType = _currentBubbleView
                .Select(bubbleView => bubbleView?.BubbleType ?? BubbleType.None)
                .ToReadOnlyReactiveProperty();
            Subscribe();
        }
        
        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            Observable.FromEvent<BubbleChangedEvent>(
                    h => _bubbleManager.OnBubbleChanged += h,
                    h => _bubbleManager.OnBubbleChanged -= h)
                .Subscribe(OnBubbleChanged)
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _subscriptions.Dispose();
        }
        
        private void OnBubbleChanged(BubbleChangedEvent evt)
        {
            switch (evt.IsSpawned)
            {
                case true when _currentBubbleView.Value == null:
                {
                    var hookGameObject = _hookFactory.CurrentGameObject;
                    if (!hookGameObject || !_bubbleManager.TryLandOnBubble(hookGameObject.transform.position, evt.BubbleView))
                    {
                        DebugUtils.Log("Hook is not on the bubble, ignoring bubble set.");
                        UnsetBubble();
                        return;
                    }
                    DebugUtils.Log("Setting current bubble to spawned bubble.");
                    SetBubble(evt.BubbleView);
                    break;
                }
                case false when _currentBubbleView.Value == evt.BubbleView:
                    DebugUtils.Log("Unsetting current bubble as it was popped.");
                    UnsetBubble();
                    break;
            }
        }

        public void SetBubble(IBubbleView bubble)
        {
            _currentBubbleView.Value = bubble;
        }   
        
        public void UnsetBubble()
        {
            _currentBubbleView.Value = null;
        }
    }
}