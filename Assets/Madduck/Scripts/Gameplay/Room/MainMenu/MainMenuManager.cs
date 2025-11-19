using System;
using Madduck.Audio;
using Madduck.Core;
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
        private readonly IAudioManager _audioManager;

        private AudioReference _bgm;
        
        [Inject]
        public MainMenuManager(
            MainMenuConfig config,
            LoadSceneManager loadSceneManager,
            IAudioManager audioManager)
        {
            _config = config;
            _loadSceneManager = loadSceneManager;
            _audioManager = audioManager;
        }
        
        public void Start()
        {
            var randomBgm = _config.MainMenuBGMPlaylist[UnityEngine.Random.Range(0, _config.MainMenuBGMPlaylist.Count)];
            _bgm = _audioManager.PlayAudio(randomBgm, Vector3.zero);
        }

        public void GoToGameplay()
        {
            _audioManager.StopAudio(_bgm);
            _loadSceneManager.LoadScene(SceneType.Gameplay, LoadSceneMode.Single, false).Forget();
        }

        public void OpenSettings()
        {
            //TODO: Implement settings menu
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