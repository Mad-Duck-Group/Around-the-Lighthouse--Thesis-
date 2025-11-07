using System;
using Madduck.Shared;
using R3;
using VContainer;

namespace Madduck.Fishing.Shared
{
    public class FishingSharedVariable
    {
        public ReadOnlyReactiveProperty<BubbleType> CurrentBubbleType { get; }
        private readonly ReactiveProperty<BubbleType> _currentBubbleType = new(BubbleType.None);
        private IDisposable _currentBubbleSubscription;
        
        [Inject]
        public FishingSharedVariable()
        {
            CurrentBubbleType = _currentBubbleType.ToReadOnlyReactiveProperty();
        }

        public void SetBubble(IBubbleView bubble)
        {
            _currentBubbleType.Value = bubble.BubbleType;
            _currentBubbleSubscription = Observable.FromEvent(
                h => bubble.OnDisappeared += h,
                h => bubble.OnDisappeared -= h)
                .Subscribe(_ =>
                {
                    UnsetBubble();
                    _currentBubbleSubscription.Dispose();
                });
        }
        
        public void UnsetBubble()
        {
            _currentBubbleType.Value = BubbleType.None;
        }
    }
}