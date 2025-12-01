using System;
using Madduck.Core;
using Madduck.Input;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Room
{
    public class FishingRoomPauseHandler : IDisposable
    {
        private readonly FishingRoomManager _fishingRoomManager;
        private readonly SettingsPanelViewModel _settingsPanelViewModel;
        private readonly IPlayerInputHandler _inputHandler;
        private readonly ISubscriber<LoadSceneStageEvent> _loadSceneStageEventSubscriber;
        
        private IDisposable _subscriptions;
        private float _timeScaleBeforePause = 1f;
        private bool _cursorVisibleBeforePause;
        private bool _canPause;
        private CursorLockMode _cursorLockStateBeforePause = CursorLockMode.None;
        
        [Inject]
        public FishingRoomPauseHandler(
            FishingRoomManager fishingRoomManager,
            SettingsPanelViewModel settingsPanelViewModel,
            IPlayerInputHandler inputHandler,
            ISubscriber<LoadSceneStageEvent> loadSceneStageEventSubscriber)
        {
            _fishingRoomManager = fishingRoomManager;
            _settingsPanelViewModel = settingsPanelViewModel;
            _inputHandler = inputHandler;
            _loadSceneStageEventSubscriber = loadSceneStageEventSubscriber;
            Subscribe();
        }
        
        private void Subscribe()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _inputHandler.PauseGameButton.IsDown
                .IgnoreFirstValueWhenSubscribe()
                .Where(x => x && _canPause)
                .Subscribe(_ =>
                {
                    var gameState = GameConstants.CurrentGameState.CurrentValue;
                    OnTogglePause(!gameState.Equals(GameState.Paused));
                })
                .AddTo(ref disposableBuilder);
            _settingsPanelViewModel.IsActive
                .IgnoreFirstValueWhenSubscribe()
                .Where(x => !x)
                .Subscribe(_ => OnPanelClosed())
                .AddTo(ref disposableBuilder);
            Observable.FromEvent(
                    h => _settingsPanelViewModel.OnRequestBackToMainMenu += h, 
                    h => _settingsPanelViewModel.OnRequestBackToMainMenu -= h)
                .Subscribe(_ => OnRequestBackToMainMenu())
                .AddTo(ref disposableBuilder);
            _loadSceneStageEventSubscriber
                .Subscribe(OnLoadSceneStageChanged)
                .AddTo(ref disposableBuilder);
            _subscriptions = disposableBuilder.Build();
        }
            
        public void Dispose()
        {
            _subscriptions.Dispose();
        }
        
        private void OnTogglePause(bool active)
        {
            if (active)
            {
                GameConstants.SetGameState(GameState.Paused);
                _timeScaleBeforePause = Time.timeScale;
                _cursorLockStateBeforePause = Cursor.lockState;
                _cursorVisibleBeforePause = Cursor.visible;
                Time.timeScale = 0;
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                _inputHandler.SetActiveInput(false);
                _settingsPanelViewModel.SetActive(true);
            }
            else
            { 
                _settingsPanelViewModel.SetActive(false);
                OnPanelClosed();
            }
        }

        private void OnPanelClosed()
        {
            GameConstants.SetGameState(GameState.Normal);
            Time.timeScale = _timeScaleBeforePause;
            Cursor.lockState = _cursorLockStateBeforePause;
            Cursor.visible = _cursorVisibleBeforePause;
            _inputHandler.SetActiveInput(true);
        }
        
        private void OnRequestBackToMainMenu()
        {
            GameConstants.SetGameState(GameState.Normal);
            Time.timeScale = _timeScaleBeforePause;
            _fishingRoomManager.ToMainMenu();
        }

        private void OnLoadSceneStageChanged(LoadSceneStageEvent loadSceneStageEvent)
        {
            _canPause = loadSceneStageEvent.Stage is LoadSceneStage.FinishFadeIn;
        }
    }
}