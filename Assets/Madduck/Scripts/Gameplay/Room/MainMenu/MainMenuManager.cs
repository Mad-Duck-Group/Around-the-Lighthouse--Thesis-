using System;
using Madduck.Audio;
using Madduck.Core;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Madduck.Room
{
    [Serializable]
    public class MainMenuManager : IStartable
    {
        private readonly MainMenuConfig _config;
        private readonly LoadSceneManager _loadSceneManager;
        private readonly SettingsPanelViewModel _settingsPanelViewModel;
        private readonly IAudioManager _audioManager;

        private AudioReference _bgm;
        private bool _isSwitchingScene;
        private IDisposable _bindings;
        
        [Inject]
        public MainMenuManager(
            MainMenuConfig config,
            LoadSceneManager loadSceneManager,
            SettingsPanelViewModel settingsPanelViewModel,
            IAudioManager audioManager)
        {
            _config = config;
            _loadSceneManager = loadSceneManager;
            _settingsPanelViewModel = settingsPanelViewModel;
            _audioManager = audioManager;
        }
        
        public void Start()
        {
            _isSwitchingScene = false;
            var randomBgm = _config.MainMenuBGMPlaylist[UnityEngine.Random.Range(0, _config.MainMenuBGMPlaylist.Count)];
            _bgm = _audioManager.PlayAudio(randomBgm, Vector3.zero);
        }

        public void GoToGameplay()
        {
            if (_isSwitchingScene) return;
            _isSwitchingScene = true;
            _audioManager.StopAudio(_bgm);
            _loadSceneManager.LoadScene(SceneType.Gameplay, LoadSceneMode.Single, false).Forget();
        }

        public void OpenSettings()
        {
            _settingsPanelViewModel.SetActive(true);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}