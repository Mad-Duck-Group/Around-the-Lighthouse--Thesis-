using System;
using Madduck.Shared;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Room
{
    public class FishCountViewModel : IDisposable
    {
        private readonly FishCountView _view;
        private readonly FishingRoomManager _fishingRoomManager;
        public ReadOnlyReactiveProperty<uint> CurrentFishCount { get; }
        public ReadOnlyReactiveProperty<uint> MaxFishCount { get; }
        
        private CompositeDisposable _disposables = new ();
        
        [Inject]
        public FishCountViewModel( FishingRoomManager fishingRoomManager)
        {
            
            _fishingRoomManager = fishingRoomManager;
            
            CurrentFishCount = _fishingRoomManager.CurrentFishCount
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);

            MaxFishCount = _fishingRoomManager.MaxFishCount
                .ToReadOnlyReactiveProperty()
                .AddTo(_disposables);
            
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
