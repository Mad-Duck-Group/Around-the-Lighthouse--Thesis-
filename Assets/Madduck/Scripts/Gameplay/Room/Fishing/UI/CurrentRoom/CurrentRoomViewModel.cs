using System;
using System.Collections.Generic;
using Madduck.Day;
using Madduck.Shared;
using Madduck.Utils;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Room
{
    public class CurrentRoomViewModel : IDisposable
    {
        private readonly DayManager _dayManager;
        private readonly IGenericFactory<CurrentRoomUIFactory> _uiFactory;
        private IReadOnlyList<DayPhaseType> _roomTrack = new List<DayPhaseType>();
        private IDisposable _bindings;
        
        [Inject]
        public CurrentRoomViewModel(DayManager dayManager )
        {
            _dayManager = dayManager;
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            Observable.EveryValueChanged(_dayManager, d => d.CurrentRoomIndex)
                .Subscribe(_ => UpdateTrack())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        private void UpdateTrack()
        {
            var list = new List<DayPhaseType>();

            for (uint i = 0; i < _dayManager._config.MaxRoomCount; i++)
            {
                DayPhaseType type;
                if (i == _dayManager._config.MaxRoomCount - 1)
                {
                    type = DayPhaseType.Both;
                }
                else
                {
                    var percent = Percentage.FromFraction((float)i / (_dayManager._config.MaxRoomCount - 1));
                    type = percent <= _dayManager._config.DayNightRatio
                        ? DayPhaseType.Day
                        : DayPhaseType.Night;
                }

            }

            
        }
        public void Dispose()
        {
            _bindings?.Dispose();
        }
    }
}
