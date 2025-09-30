using System;
using System.Collections.Generic;
using Madduck.Day;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Room
{
    public class RoomTrackColumnView : IDisposable
    {
        private readonly IGenericFactory<RoomTrackView> _roomTrackFactory;
        private readonly Dictionary<DayRoomKey, Sprite> _spriteMap;
        private readonly List<RoomTrackView> _rooms = new();
        private readonly RoomTrackViewModel _roomTrackViewModel;
        private IDisposable _binding;

        [Inject]
        public RoomTrackColumnView( RoomTrackViewModel roomTrackViewModel
            ,IGenericFactory<RoomTrackView> roomTrackFactory,
            SerializableDictionary<DayRoomKey, Sprite> spriteMap)
        {
            _roomTrackViewModel = roomTrackViewModel;
            _roomTrackFactory = roomTrackFactory;
            _spriteMap = spriteMap;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _roomTrackViewModel.OnStateChanged += HandleStateChanged;
        }
        private void HandleStateChanged(DayStateChangedEvent state)
        {
            foreach (var room in _rooms)
                UnityEngine.Object.Destroy(room.gameObject);
            _rooms.Clear();
            // 0–2 fishing day
            for (int i = 0; i <= 2; i++)
                CreateRoom(new DayRoomKey(DayPhaseType.Day, RoomType.Fishing));

            // 3–5 fishing night
            for (int i = 3; i <= 5; i++)
                CreateRoom(new DayRoomKey(DayPhaseType.Night, RoomType.Fishing));

            // 6 event 
            CreateRoom(new DayRoomKey(DayPhaseType.Both, RoomType.Event));
        }
        private void CreateRoom(DayRoomKey dayRoomKey)
        {
            var view = _roomTrackFactory.Create();
            if (_spriteMap.TryGetValue((dayRoomKey), out var sprite))
                view.SetUp(sprite);
            _rooms.Add(view);
        }
        
        public void Dispose()
        {
            _binding.Dispose();
        }
    }
}
