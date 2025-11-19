using System;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Room
{
    public class MainMenuView : MonoBehaviour
    {
        [Title("References")]
        [Required, 
         SerializeField] private Button sailingButton;
        [Required,
         SerializeField] private Button settingsButton;
        [Required,
         SerializeField] private Button quitButton;
        
        private MainMenuViewModel _viewModel;
        private IDisposable _bindings;

        [Inject]
        public void SetUp(MainMenuViewModel viewModel)
        {
            _viewModel = viewModel;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            sailingButton.OnClickAsObservable()
                .Subscribe(_ => _viewModel.SailingButtonCommand.Execute())
                .AddTo(ref disposableBuilder);
            settingsButton.OnClickAsObservable()
                .Subscribe(_ => _viewModel.SettingsButtonCommand.Execute())
                .AddTo(ref disposableBuilder);
            quitButton.OnClickAsObservable()
                .Subscribe(_ => _viewModel.QuitButtonCommand.Execute())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        private void OnDestroy()
        {
            _bindings?.Dispose();
        }
    }
}