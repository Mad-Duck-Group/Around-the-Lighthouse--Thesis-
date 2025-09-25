using System;
using Madduck.Shared;
using R3;
using UnityEngine;

namespace Madduck.Room
{
    public class FishCaughtViewModel : IDisposable
    {
        private readonly FishCaughtView _view;
        private readonly FishingRoomManager _fishingRoomManager;
        private uint _currentFishCount;
        private uint _maxFishCount;
        
        private IDisposable _bindings;
        
        public FishCaughtViewModel(FishCaughtView view, FishingRoomManager fishingRoomManager)
        {
            _view = view;
            _fishingRoomManager = fishingRoomManager;
            _bindings = _fishingRoomManager.CurrentFishCount.CombineLatest(_fishingRoomManager.MaxFishCount,
                    (current, max) => (current, max))
                .Subscribe(tuple =>
                {
                    _currentFishCount = tuple.current;
                    _maxFishCount = tuple.max;
                    _view.SetFishCaught(_currentFishCount, _maxFishCount);
                });
        }
       
        
        public void Dispose()
        {
           _bindings?.Dispose();
        }
    }
}
