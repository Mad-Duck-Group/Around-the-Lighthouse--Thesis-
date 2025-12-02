using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Madduck.Audio;
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
    public class RoomTrackColumnController : IDisposable
    {
        #region Fields
        
        private readonly RoomTrackConfig _roomTrackConfig;
        private readonly DayManagerConfig _dayManagerConfig;
        private readonly LoadSceneManager _loadSceneManager;
        private readonly IAudioManager _audioManager;
        private readonly IFactory<RoomTrackView> _roomTrackFactory;
        private readonly IFactory<BoatTrackView> _boatTrackFactory;
        private readonly ISubscriber<LoadSceneStageEvent> _loadSceneStageEventSubscriber;
        private readonly IPublisher<LoadingSceneAnimationFinishedEvent> _loadingSceneAnimationFinishedPublisher;
        
        private readonly ReadOnlyReactiveProperty<uint> _currentRoomIndex;
        private readonly List<RoomTrackView> _rooms = new();
        private BoatTrackView _boatTrackView;
        private IDisposable _binding;
        private AudioReference _boatEngineAudioRef;
        #endregion

        #region Inject

        [Inject]
        public RoomTrackColumnController(
            RoomTrackConfig roomTrackConfig,
            DayManagerConfig dayManagerConfig,
            RoomTrackViewModel roomTrackViewModel,
            LoadSceneManager loadSceneManager,
            IAudioManager audioManager,
            IFactory<RoomTrackView> roomTrackFactory,
            IFactory<BoatTrackView> boatTrackFactory,
            ISubscriber<LoadSceneStageEvent> loadSceneStageEventSubscriber,
            IPublisher<LoadingSceneAnimationFinishedEvent> loadingSceneAnimationFinishedPublisher)
        {
            _roomTrackConfig = roomTrackConfig;
            _dayManagerConfig = dayManagerConfig;
            _boatTrackFactory = boatTrackFactory;
            _roomTrackFactory = roomTrackFactory;
            _loadSceneManager = loadSceneManager;
            _audioManager = audioManager;
            _loadSceneStageEventSubscriber = loadSceneStageEventSubscriber;
            _loadingSceneAnimationFinishedPublisher = loadingSceneAnimationFinishedPublisher;
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
            _audioManager.StopAudio(_boatEngineAudioRef);
            _binding.Dispose();
        }
        #endregion
        
        #region RoomTrackControl

        private async UniTaskVoid OnFinishedLoading()
        {
            var roomCount = CalculateIndexRoom();
            for (int i = 0; i <= roomCount; i++)
            {
                CreateRoom(new DayRoomKey(DayPhaseType.Day, RoomType.Fishing), i);
            }

            var maxRoomCount = _dayManagerConfig.MaxRoomCount;
            for (int i = roomCount + 1; i < maxRoomCount; i++)
            {
                var state = CalculateRoomState(i);
                if (i == maxRoomCount - 1)
                {
                    CreateRoom(new DayRoomKey(DayPhaseType.Both, RoomType.Event),i);
                    break;
                }
                CreateRoom(new DayRoomKey(DayPhaseType.Night, RoomType.Fishing),i);
            }
            await UniTask.WaitForEndOfFrame();
            CreateBoatTrack().Forget();
        }
        
        private void CreateRoom(DayRoomKey roomKey,int index)
        {
            
            var state = CalculateRoomState(index);
            DayRoomSprite sprite = null;
            if (state == RoomHistoryState.Past)
                _roomTrackConfig.PastSprites.TryGetValue(roomKey, out sprite);
            else
                _roomTrackConfig.FutureSprites.TryGetValue(roomKey, out sprite);
            var view = _roomTrackFactory.Create();
            view.SetUp(sprite.iconSprite, sprite.auraSprite);
            _rooms.Add(view);
        }
        
        private int CalculateIndexRoom()
        {
            var percentIndex = Mathf.FloorToInt(_dayManagerConfig.MaxRoomCount * _dayManagerConfig.DayNightRatio.AsFraction) - 1;
            return percentIndex;
        }
        private RoomHistoryState CalculateRoomState(int roomIndex)
        {
            var current = (int)_currentRoomIndex.CurrentValue;
            
            if (roomIndex < current)
            {
                return RoomHistoryState.Past;
            }
            return RoomHistoryState.Future;
            
            
        }
        #endregion

        #region BoatTrackControl
        private async UniTaskVoid CreateBoatTrack()
        {
            var boatTrackView = _boatTrackFactory.Create();
            _boatTrackView = boatTrackView;
            var shouldNotify = _loadSceneManager.CurrentSceneType == SceneType.Loading;
            var boatRectTransform = (RectTransform)_boatTrackView.transform;
            if (_currentRoomIndex.CurrentValue == 0)
            {
                boatRectTransform.anchoredPosition = boatRectTransform.parent.InverseTransformPoint(_rooms[0].transform.position);
                if (shouldNotify) 
                    _loadingSceneAnimationFinishedPublisher.Publish(new LoadingSceneAnimationFinishedEvent());
                return;
            }
            var previousPos = _rooms[(int)_currentRoomIndex.CurrentValue - 1].transform.position;
            var currentPos = _rooms[(int)_currentRoomIndex.CurrentValue].transform.position;
            boatRectTransform.anchoredPosition = boatRectTransform.parent.InverseTransformPoint(previousPos);
            _boatEngineAudioRef = _audioManager.PlayAudio(_roomTrackConfig.BoatEngineSound, Vector3.zero);
            await _boatTrackView.AnimateBoatTrack(currentPos);
            _audioManager.StopAudio(_boatEngineAudioRef);
            if (shouldNotify) 
                _loadingSceneAnimationFinishedPublisher.Publish(new LoadingSceneAnimationFinishedEvent());
        }
        
        #endregion
    }
}
