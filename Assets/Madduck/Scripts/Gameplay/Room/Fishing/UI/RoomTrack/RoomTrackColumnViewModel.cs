using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
using UnityEngine.SceneManagement;


namespace Madduck.Room
{
    public class RoomTrackColumnViewModel : IDisposable
    {
        #region Fields
        private readonly Dictionary<DayRoomKey, Sprite> _spriteMap;
        private readonly List<RoomTrackView> _rooms = new();
        private readonly DayManagerConfig _dayManagerConfig;
        private readonly ReadOnlyReactiveProperty<uint> _currentRoomIndex;
        private readonly IGenericFactory<RoomTrackView> _roomTrackFactory;
        private readonly IGenericFactory<BoatTrackView> _boatTrackFactory;
        private readonly ISubscriber<LoadSceneStageEvent> _loadSceneStageEventSubscriber;
        private readonly LoadSceneManager _loadSceneManager;
        private BoatTrackView _boatTrackView;
        private IDisposable _binding;
        #endregion

        #region Inject

        [Inject]
        public RoomTrackColumnViewModel(
            SerializableDictionary<DayRoomKey, Sprite> spriteMap,
            DayManagerConfig dayManagerConfig,
            RoomTrackViewModel roomTrackViewModel,
            IGenericFactory<RoomTrackView> roomTrackFactory,
            IGenericFactory<BoatTrackView> boatTrackFactory,
            ISubscriber<LoadSceneStageEvent> loadSceneStageEventSubscriber,
            LoadSceneManager loadSceneManager)
        {
            _dayManagerConfig = dayManagerConfig;
            _boatTrackFactory = boatTrackFactory;
            _roomTrackFactory = roomTrackFactory;
            _loadSceneManager = loadSceneManager;
            _loadSceneStageEventSubscriber = loadSceneStageEventSubscriber;
            _spriteMap = spriteMap;
            _currentRoomIndex = roomTrackViewModel.CurrentRoomIndex.ToReadOnlyReactiveProperty();
            Bind();
        }

        #endregion

        #region Bind

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _loadSceneStageEventSubscriber
                .AsObservable().ToObservable()
                .Where(x => x.Stage is LoadSceneStage.FinishLoading)
                .Subscribe(_ => OnFinishedLoading().Forget())
                .AddTo(ref disposableBuilder);
            _binding = disposableBuilder.Build();
        }

        public void Dispose()
        {
            _binding.Dispose();
        }
        #endregion
        
        #region RoomTrackControl

        private async UniTaskVoid OnFinishedLoading()
        {
            var roomCount = CalculateIndexRoom();
            for (int i = 0; i <= roomCount; i++)
            {
                CreateRoom(new DayRoomKey(DayPhaseType.Day, RoomType.Fishing));
            }

            var maxRoomCount = _dayManagerConfig.MaxRoomCount;
            for (int i = roomCount + 1; i < maxRoomCount; i++)
            {
                if (i == maxRoomCount - 1)
                {
                    CreateRoom(new DayRoomKey(DayPhaseType.Both, RoomType.Event));
                    break;
                }
                CreateRoom(new DayRoomKey(DayPhaseType.Night, RoomType.Fishing));
            }
            await UniTask.WaitForEndOfFrame();
            CreateBoatTrack();
        }
        
        private void CreateRoom(DayRoomKey dayRoomKey)
        {
            var view = _roomTrackFactory.Create();
            if (_spriteMap.TryGetValue(dayRoomKey, out var sprite))
                view.SetUp(sprite);
            _rooms.Add(view);
        }
        
        private int CalculateIndexRoom()
        {
            var percentIndex = Mathf.FloorToInt(_dayManagerConfig.MaxRoomCount * _dayManagerConfig.DayNightRatio.AsFraction) - 1;
            return percentIndex;
        }

        #endregion

        #region BoatTrackControl
        private void CreateBoatTrack()
        {
            var boatTrackView = _boatTrackFactory.Create();
            _boatTrackView = boatTrackView;

            var boatRectTransform = (RectTransform)_boatTrackView.transform;
            if (_currentRoomIndex.CurrentValue == 0)
            {
                boatRectTransform.anchoredPosition = boatRectTransform.parent.InverseTransformPoint(_rooms[0].transform.position);
                return;
            }
            var previousPos = _rooms[(int)_currentRoomIndex.CurrentValue - 1].transform.position;
            var currentPos = _rooms[(int)_currentRoomIndex.CurrentValue].transform.position;
            boatRectTransform.anchoredPosition = boatRectTransform.parent.InverseTransformPoint(previousPos);
            bool shouldNotify = _loadSceneManager.CurrentSceneType == SceneType.Loading;
            _boatTrackView.AnimateBoatTrack(currentPos, shouldNotify);
        }
        
        #endregion
    }
}
