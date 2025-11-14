using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Madduck.Core;
using Madduck.Day;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using UnityEngine;
using VContainer;


namespace Madduck.Room
{
    public class RoomTrackColumnViewModel : IDisposable
    {
        #region Fields
        private readonly Dictionary<DayRoomKey, Sprite> _spriteMap;
        private readonly List<RoomTrackView> _rooms = new();
        private readonly DayManagerConfig _dayManagerConfig;
        private readonly ReadOnlyReactiveProperty<uint> _currentRoomIndex;
        private readonly LoadSceneManager _loadSceneManager;
        private readonly IFactory<RoomTrackView> _roomTrackFactory;
        private readonly IFactory<BoatTrackView> _boatTrackFactory;
        private readonly ISubscriber<LoadSceneStageEvent> _loadSceneStageEventSubscriber;
        private readonly IPublisher<LoadingSceneAnimationFinishedEvent> _loadingSceneAnimationFinishedPublisher;
        private BoatTrackView _boatTrackView;
        private IDisposable _binding;
        #endregion

        #region Inject

        [Inject]
        public RoomTrackColumnViewModel(
            SerializableDictionary<DayRoomKey, Sprite> spriteMap,
            DayManagerConfig dayManagerConfig,
            RoomTrackViewModel roomTrackViewModel,
            LoadSceneManager loadSceneManager,
            IFactory<RoomTrackView> roomTrackFactory,
            IFactory<BoatTrackView> boatTrackFactory,
            ISubscriber<LoadSceneStageEvent> loadSceneStageEventSubscriber,
            IPublisher<LoadingSceneAnimationFinishedEvent> loadingSceneAnimationFinishedPublisher
            )
        {
            _dayManagerConfig = dayManagerConfig;
            _boatTrackFactory = boatTrackFactory;
            _roomTrackFactory = roomTrackFactory;
            _loadSceneManager = loadSceneManager;
            _loadSceneStageEventSubscriber = loadSceneStageEventSubscriber;
            _loadingSceneAnimationFinishedPublisher = loadingSceneAnimationFinishedPublisher;
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
            CreateBoatTrack().Forget();
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
        private async UniTaskVoid CreateBoatTrack()
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
            var shouldNotify = _loadSceneManager.CurrentSceneType == SceneType.Loading;
            await _boatTrackView.AnimateBoatTrack(currentPos);
            if (shouldNotify) 
                _loadingSceneAnimationFinishedPublisher.Publish(new LoadingSceneAnimationFinishedEvent());
        }
        
        #endregion
    }
}
