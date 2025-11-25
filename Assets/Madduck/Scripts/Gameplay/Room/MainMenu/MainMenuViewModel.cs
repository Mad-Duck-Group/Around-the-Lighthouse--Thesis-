using System;
using R3;
using VContainer;
using ReactiveCommand = R3.ReactiveCommand;

namespace Madduck.Room
{
    public class MainMenuViewModel : IDisposable
    {
        private readonly MainMenuManager _mainMenuManager;
        
        public ReactiveProperty<Unit> SettingClosed { get; } = new();
        public ReactiveCommand SailingButtonCommand { get; } = new();
        public ReactiveCommand SettingsButtonCommand { get; } = new();
        public ReactiveCommand QuitButtonCommand { get; } = new();

        private SettingsPanelViewModel _settingsPanelViewModel;
        private IDisposable _bindings;
        
        [Inject]
        public MainMenuViewModel(
            MainMenuManager mainMenuManager,
            SettingsPanelViewModel settingsPanelViewModel)
        {
            _mainMenuManager = mainMenuManager;
            _settingsPanelViewModel = settingsPanelViewModel;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _settingsPanelViewModel.IsActive
                .Where(x => !x)
                .Subscribe(_ => OnSettingsClosed())
                .AddTo(ref disposableBuilder);
            SailingButtonCommand
                .Subscribe(_ => OnSailingButtonClicked())
                .AddTo(ref disposableBuilder);
            SettingsButtonCommand
                .Subscribe(_ => OnSettingsButtonClicked())
                .AddTo(ref disposableBuilder);
            QuitButtonCommand
                .Subscribe(_ => OnQuitButtonClicked())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }

        private void OnSettingsClosed()
        {
            SettingClosed.OnNext(Unit.Default);
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