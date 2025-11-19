using System;
using R3;
using VContainer;
using ReactiveCommand = Reactive.Bindings.ReactiveCommand;

namespace Madduck.Room
{
    public class MainMenuViewModel : IDisposable
    {
        private readonly MainMenuManager _mainMenuManager;
        
        public ReactiveCommand SailingButtonCommand { get; } = new();
        public ReactiveCommand SettingsButtonCommand { get; } = new();
        public ReactiveCommand QuitButtonCommand { get; } = new();

        private IDisposable _bindings;
        
        [Inject]
        public MainMenuViewModel(MainMenuManager mainMenuManager)
        {
            _mainMenuManager = mainMenuManager;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            SailingButtonCommand
                .Subscribe(onNext: OnSailingButtonClicked)
                .AddTo(ref disposableBuilder);
            SettingsButtonCommand
                .Subscribe(onNext: OnSettingsButtonClicked)
                .AddTo(ref disposableBuilder);
            QuitButtonCommand
                .Subscribe(onNext: OnQuitButtonClicked)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
        
        private void OnSailingButtonClicked()
        { 
            _mainMenuManager.GoToGameplay();
        }
        
        private void OnSettingsButtonClicked()
        {
            _mainMenuManager.OpenSettings();
        }

        private void OnQuitButtonClicked()
        {
            _mainMenuManager.QuitGame();
        }
    }
}