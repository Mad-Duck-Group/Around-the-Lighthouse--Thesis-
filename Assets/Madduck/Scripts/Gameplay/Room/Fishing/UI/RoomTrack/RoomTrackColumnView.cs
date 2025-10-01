using System;
using System.Collections.Generic;
using Madduck.Core;
using Madduck.Day;
using Madduck.GameData;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using UnityEngine;
using VContainer;
using PrimeTween;


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
        private ReadOnlyReactiveProperty<uint> _currentRoomIndex;
        private readonly ISubscriber<LoadSceneStageEvent> _loadSceneStageEventSubscriber;
        private IDisposable _binding;
        private Sequence _tween;
        #endregion

        #region Inject

        [Inject]
        public RoomTrackColumnView(IGenericFactory<RoomTrackView> roomTrackFactory,
            SerializableDictionary<DayRoomKey, Sprite> spriteMap,
            DayManagerConfig dayManagerConfig,IGenericFactory<BoatTrackView> boatTrackFactory,
            RoomTrackViewModel roomTrackViewModel
            ,ISubscriber<LoadSceneStageEvent> loadSceneStageEventSubscriber)
        {
            _dayManagerConfig = dayManagerConfig;
            _boatTrackFactory = boatTrackFactory;
            _roomTrackFactory = roomTrackFactory;
            _roomTrackViewModel = roomTrackViewModel;
            _loadSceneStageEventSubscriber = loadSceneStageEventSubscriber;
            _spriteMap = spriteMap;
            ;
            Bind();
            HandleStateChanged();
            
        }

        #endregion

        #region Bind

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _currentRoomIndex = _roomTrackViewModel.CurrentRoomIndex.ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            _loadSceneStageEventSubscriber
                .AsObservable().ToObservable()
                .Where(x => x.Stage is LoadSceneStage.FinishLoading)
                .Subscribe(_ => CreateBoatTrack())
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
            if (_boatTrackView != null){UnityEngine.Object.Destroy(_boatTrackView.gameObject);}
            var boatTrackView = _boatTrackFactory.Create();
            _boatTrackView = boatTrackView;

            if (_currentRoomIndex.CurrentValue == 0)
            {
                _boatTrackView._boatRectTransform.anchoredPosition = _rooms[0]._RoomRectTransform.localPosition;
                return;
            }
            _boatTrackView._boatRectTransform.anchoredPosition = _rooms[(int)_currentRoomIndex.CurrentValue - 1]._RoomRectTransform.localPosition;
            BoatTrackAnimate((int)_currentRoomIndex.CurrentValue);
        }
        private void BoatTrackAnimate(int index)
        {
            DebugUtils.Log("index : " + index);
            var boatUI = _boatTrackView.transform;
            var roomUI = _rooms[index]._RoomRectTransform;
            Vector3 targetLocalPos = roomUI.TransformPoint(roomUI.localPosition);
            
            _tween.Stop();
            _tween = Sequence.Create()
                .Group(Tween.Position(boatUI,targetLocalPos, 2f, Ease.Linear))
                .Group(Tween.Rotation(boatUI, Quaternion.Euler(0f, 0f, 10f), 2f, Ease.InOutSine));
            
            _tween.OnComplete(() =>
            {
                boatUI.rotation = Quaternion.identity;
            });
            //DebugUtils.Log(targetRoomRect + " - " + boatRect.anchoredPosition);
        }
        private void OnRoomIndexChanged(int index)
        {
            if (index == 0){return;}
            
        }
        #endregion
        
        public void Dispose()
        {
            _binding.Dispose();
        }
    }
}
