using System;
using Madduck.Audio;
using Madduck.Input;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace Madduck.Shared
{
    [Serializable]
    public class QteButtonFactory : IFactory<IQuickTimeEvent>
    {
        [Title("References")]
        [Required, 
         SerializeField] private QteButtonConfig config;
        [Required,
         SerializeField] private QteButtonView viewPrefab;
        [SerializeField] private Transform parent;

        private IAudioManager _audioManager;
        private IPlayerInputHandler _inputHandler;
        private IDisposable _disposables;
        
        public IQuickTimeEvent Current { get; private set; }
        
        [Inject]
        public void SetUp(
            IAudioManager audioManager,
            IPlayerInputHandler inputHandler)
        {
            _audioManager = audioManager;
            _inputHandler = inputHandler;
        }
        
        public IQuickTimeEvent Create()
        {
            var view = Object.Instantiate(viewPrefab, parent);
            var controller = new QteButtonController(
                new QteButtonConfigInstance(config), 
                _audioManager, 
                _inputHandler, 
                view);
            view.SetUp(controller);
            Current = controller;
            return controller;
        }
    }
}