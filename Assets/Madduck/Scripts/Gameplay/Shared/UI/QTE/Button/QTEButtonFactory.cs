using System;
using Madduck.Input;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Madduck.Shared
{
    public interface IQTEButtonFactory : IGenericFactory<IQuickTimeEvent> { }
    
    [Serializable]
    public class QTEButtonFactory : IQTEButtonFactory
    {
        [Title("References")]
        [Required, 
         SerializeField] private QTEButtonConfig config;
        [Required,
         SerializeField] private QTEButtonView viewPrefab;
        [Required,
         SerializeField] private Transform parent;

        private IPlayerInputHandler _inputHandler;
        private IDisposable _disposables;
        
        public IQuickTimeEvent Current { get; private set; }
        
        public void SetUp(IPlayerInputHandler inputHandler)
        {
            _inputHandler = inputHandler;
        }
        
        public IQuickTimeEvent Create()
        {
            _disposables?.Dispose();
            var disposableBuilder = Disposable.CreateBuilder();
            var view = Object.Instantiate(viewPrefab, parent);
            var controller = new QTEButtonController(new QTEButtonConfigInstance(config), _inputHandler, view);
            var viewModel = new QTEButtonViewModel(controller);
            view.SetUp(viewModel);
            Current = controller;
            disposableBuilder.Add(controller);
            disposableBuilder.Add(viewModel);
            _disposables = disposableBuilder.Build();
            return controller;
        }
    }
}