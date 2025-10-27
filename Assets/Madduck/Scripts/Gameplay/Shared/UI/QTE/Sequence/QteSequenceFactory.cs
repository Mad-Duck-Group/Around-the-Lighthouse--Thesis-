using System;
using Madduck.Input;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace Madduck.Shared
{
    [Serializable]
    public class QteSequenceFactory : IGenericFactory<IQuickTimeEvent>
    {
        [Title("References")]
        [Required, 
         SerializeField] private QteSequenceConfig config;
        [Required,
         SerializeField] private QteSequenceView viewPrefab;
        [Required,
         SerializeField] private Transform parent;
        [Required, 
         SerializeField] private QteButtonFactory buttonFactory;

        private IPlayerInputHandler _inputHandler;
        private IDisposable _disposables;
        
        public IQuickTimeEvent Current { get; private set; }
        
        [Inject]
        public void SetUp(
            IPlayerInputHandler inputHandler)
        {
            _inputHandler = inputHandler;
            buttonFactory.SetUp(inputHandler);
        }
        
        public IQuickTimeEvent Create()
        {
            _disposables?.Dispose();
            var disposableBuilder = Disposable.CreateBuilder();
            var view = Object.Instantiate(viewPrefab, parent);
            var controller = new QteSequenceController(
                new QteSequenceConfigInstance(config), 
                buttonFactory,
                _inputHandler, 
                view);
            //view.SetUp(controller);
            Current = controller;
            disposableBuilder.Add(controller);
            _disposables = disposableBuilder.Build();
            return controller;
        }
    }
}