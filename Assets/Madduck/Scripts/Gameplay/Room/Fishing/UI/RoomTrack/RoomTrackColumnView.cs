using System;
using System.Collections.Generic;
using Madduck.Day;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Room
{
    public class RoomTrackColumnView : IDisposable
    {
        private  BoatTrackView _boatTrackView;
        #region fields
        private readonly IGenericFactory<RoomTrackView> _roomTrackFactory;
        private readonly IGenericFactory<BoatTrackView> _boatTrackFactory;
        private readonly Dictionary<DayRoomKey, Sprite> _spriteMap;
        private readonly List<RoomTrackView> _rooms = new();
        private readonly DayManagerConfig _dayManagerConfig;
        private readonly RoomTrackViewModel _roomTrackViewModel;
        private IDisposable _binding;
        #endregion

        #region Inject

        [Inject]
        public RoomTrackColumnView(IGenericFactory<RoomTrackView> roomTrackFactory,
            SerializableDictionary<DayRoomKey, Sprite> spriteMap,
            DayManagerConfig dayManagerConfig,IGenericFactory<BoatTrackView> boatTrackFactory,
            RoomTrackViewModel roomTrackViewModel)
        {
            _dayManagerConfig = dayManagerConfig;
            _boatTrackFactory = boatTrackFactory;
            _roomTrackFactory = roomTrackFactory;
            _roomTrackViewModel = roomTrackViewModel;
            _spriteMap = spriteMap;
            
            Bind();
            HandleStateChanged();
            
        }

        #endregion

        #region Bind

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _roomTrackViewModel.CurrentRoomIndex
                .Subscribe(index =>
                {
                    OnRoomIndexChanged((int)index);
                })
                .AddTo(ref disposableBuilder);
            _binding = disposableBuilder.Build();
        }

        #endregion
        
        #region RoomTrackControl

        private void HandleStateChanged()
        {
            foreach (var room in _rooms)
                UnityEngine.Object.Destroy(room.gameObject);
            _rooms.Clear();
            for (int i = 0; i <= CalculateIndexRoom(); i++)
            {
                CreateRoom(new DayRoomKey(DayPhaseType.Day, RoomType.Fishing));
            }

            for (int i = CalculateIndexRoom() +1; i < _dayManagerConfig.MaxRoomCount; i++)
            {
                if (i == 6)
                {
                    CreateRoom(new DayRoomKey(DayPhaseType.Both, RoomType.Event));
                    break;
                }
                CreateRoom(new DayRoomKey(DayPhaseType.Night, RoomType.Fishing));
            }
            CreateBoatTrack();
            
        }
        private void CreateRoom(DayRoomKey dayRoomKey)
        {
            var view = _roomTrackFactory.Create();
            if (_spriteMap.TryGetValue((dayRoomKey), out var sprite))
                view.SetUp(sprite);
            _rooms.Add(view);
        }
        
        private int CalculateIndexRoom()
        {
            var percentIndex = Mathf.FloorToInt(_dayManagerConfig.MaxRoomCount * _dayManagerConfig.DayNightRatio.AsFraction )-1;
            return percentIndex;
        }

        #endregion

        #region BoatTrackControl
        private void CreateBoatTrack()
        {
            var boatTrackView = _boatTrackFactory.Create();
            _boatTrackView = boatTrackView;
        }
        private void BoatTrackAnimate()
        {
             
        }
        private void OnRoomIndexChanged(int index)
        {
            if (index == 0){return;}
            if (_boatTrackView != null)
            {
                UnityEngine.Object.Destroy(_boatTrackView.gameObject);
                CreateBoatTrack();
            }
            
            //BoatTrackAnimate();
        }
        #endregion
        
        public void Dispose()
        {
            _binding.Dispose();
        }
    }
}
