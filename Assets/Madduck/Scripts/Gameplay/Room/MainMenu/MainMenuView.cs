using System;
using Cysharp.Threading.Tasks;
using FMODUnity;
using Madduck.Audio;
using Madduck.Utils;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace Madduck.Room
{
    public class MainMenuView : MonoBehaviour
    {
        [Title("References")]
        [Required, 
         SerializeField] private MainMenuButtonView sailingButton;
        [Required,
         SerializeField] private MainMenuButtonView settingsButton;
        [Required,
         SerializeField] private MainMenuButtonView quitButton;
        [Required,
         SerializeField] private TMP_Text versionText;
        
        private MainMenuViewModel _viewModel;
       
        private IDisposable _bindings;

        [Inject]
        public void SetUp(
            MainMenuViewModel viewModel,
            IAudioManager audioManager)
        {
            _viewModel = viewModel;
            sailingButton.SetUp(audioManager);
            settingsButton.SetUp(audioManager);
            quitButton.SetUp(audioManager);
        }

        private void Start()
        {
            versionText.text = $"{Application.version}";
            TransitionInButtons().Forget();
        }

        private async UniTaskVoid TransitionInButtons()
        {
            sailingButton.TransitionIn().Forget();
            await UniTask.WaitForSeconds(0.25f);
            settingsButton.TransitionIn().Forget();
            await UniTask.WaitForSeconds(0.25f);
            await quitButton.TransitionIn();
            EventSystem.current.SetSelectedGameObject(sailingButton.gameObject);
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.SettingClosed
                .IgnoreFirstValueWhenSubscribe()
                .Subscribe(_ => OnSettingsClosed())
                .AddTo(ref disposableBuilder);
            sailingButton.Button.OnClickAsObservable()
                .SubscribeAwait((_, _) => OnSailingButtonClicked(), AwaitOperation.Drop)
                .AddTo(ref disposableBuilder);
            settingsButton.Button.OnClickAsObservable()
                .SubscribeAwait((_, _) => OnSettingsButtonClicked(), AwaitOperation.Drop)
                .AddTo(ref disposableBuilder);
            quitButton.Button.OnClickAsObservable()
                .SubscribeAwait((_, _) => OnQuitButtonClicked(), AwaitOperation.Drop)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        private void OnSettingsClosed()
        {
            settingsButton.TransitionIn().Forget();
            EventSystem.current.SetSelectedGameObject(sailingButton.gameObject);
        }
        
        private async UniTask OnSailingButtonClicked()
        {
            await sailingButton.TransitionOut();
            _viewModel.SailingButtonCommand.Execute(Unit.Default);
        }

        private async UniTask OnSettingsButtonClicked()
        {
            await settingsButton.TransitionOut();
            _viewModel.SettingsButtonCommand.Execute(Unit.Default);
        }
        
        private async UniTask OnQuitButtonClicked()
        {
            await quitButton.TransitionOut();
            _viewModel.QuitButtonCommand.Execute(Unit.Default);
        }

        private void OnDestroy()
        {
            _bindings?.Dispose();
        }
    }
}