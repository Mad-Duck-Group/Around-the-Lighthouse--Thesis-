using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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
    public class SettingsPanelView : MonoBehaviour, ITransitionable
    {
        [Title("References")]
        [Required,
         SerializeField] private Toggle masterMuteToggle;
        [Required,
         SerializeField] private Slider masterVolumeSlider;
        [Required,
         SerializeField] private Slider mouseSensitivitySlider;
        [Required,
         SerializeField] private Slider gamepadSensitivitySlider;
        [Required,
         SerializeField] private TMP_Text masterVolumeText;
        [Required,
         SerializeField] private TMP_Text mouseSensitivityText;
        [Required,
         SerializeField] private TMP_Text gamepadSensitivityText;
        [Required,
         SerializeField] private Button saveButton;
        [Required, 
         SerializeField] private Button resetButton;
        [Required, 
         SerializeField] private Button backButton;
        [Optional,
         SerializeField] private Button backToMainMenuButton;
        
        private SettingsPanelViewModel _viewModel;
        private IDisposable _bindings;

        [Inject]
        public void SetUp(SettingsPanelViewModel panelViewModel)
        {
            _viewModel = panelViewModel;
            masterVolumeSlider.wholeNumbers = true;
            mouseSensitivitySlider.wholeNumbers = true;
            gamepadSensitivitySlider.wholeNumbers = true;
            masterVolumeSlider.minValue = 0;
            masterVolumeSlider.maxValue = 100;
            mouseSensitivitySlider.minValue = 0;
            mouseSensitivitySlider.maxValue = 100;
            gamepadSensitivitySlider.minValue = 0;
            gamepadSensitivitySlider.maxValue = 100;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _viewModel.IsActive
                .Subscribe(OnActivate)
                .AddTo(ref disposableBuilder);
            _viewModel.MasterMute
                .Subscribe(OnInitializeMasterMute)
                .AddTo(ref disposableBuilder);
            _viewModel.MouseSensitivity
                .Subscribe(OnInitializeMouseSensitivity)
                .AddTo(ref disposableBuilder);
            _viewModel.GamepadSensitivity
                .Subscribe(OnInitializeGamepadSensitivity)
                .AddTo(ref disposableBuilder);
            _viewModel.MasterVolume
                .Subscribe(OnInitializeMasterVolume)
                .AddTo(ref disposableBuilder);
            masterMuteToggle.OnValueChangedAsObservable()
                .Subscribe(OnMasterMuteChanged)
                .AddTo(ref disposableBuilder);
            masterVolumeSlider.OnValueChangedAsObservable()
                .Subscribe(OnMasterVolumeSliderChanged)
                .AddTo(ref disposableBuilder);
            mouseSensitivitySlider.OnValueChangedAsObservable()
                .Subscribe(OnMouseSensitivitySliderChanged)
                .AddTo(ref disposableBuilder);
            gamepadSensitivitySlider.OnValueChangedAsObservable()
                .Subscribe(OnGamepadSensitivitySliderChanged)
                .AddTo(ref disposableBuilder);
            saveButton.OnClickAsObservable()
                .Subscribe(_ => OnSaveButtonClicked())
                .AddTo(ref disposableBuilder);
            resetButton.OnClickAsObservable()
                .Subscribe(_ => OnResetButtonClicked())
                .AddTo(ref disposableBuilder);
            backButton.OnClickAsObservable()
                .Subscribe(_ => OnBackButtonClicked())
                .AddTo(ref disposableBuilder);
            if (backToMainMenuButton)
            {
                backToMainMenuButton.OnClickAsObservable()
                    .Subscribe(_ => OnBackToMainMenuButtonClicked())
                    .AddTo(ref disposableBuilder);
            }
            _bindings = disposableBuilder.Build();
        }

        private void OnDestroy()
        {
            _bindings?.Dispose();
        }
        
        private void OnActivate(bool active)
        {
            if (active)
            {
                EventSystem.current.SetSelectedGameObject(masterVolumeSlider.gameObject);
                TransitionIn().Forget();
            }
            else
            {
                TransitionOut().Forget();
            }
        }
        
        private void OnInitializeMasterMute(bool isMuted)
        {
            masterMuteToggle.isOn = !isMuted;
            masterVolumeSlider.interactable = !isMuted;
        }
        
        private void OnInitializeMasterVolume(Percentage percentage)
        {
            masterVolumeSlider.value = percentage.AsPercentage;
            masterVolumeText.text = percentage.ToPercentageString("F0");
        }
        
        private void OnInitializeMouseSensitivity(Percentage percentage)
        {
            mouseSensitivitySlider.value = percentage.AsPercentage;
            mouseSensitivityText.text = percentage.ToPercentageString("F0");
        }
        
        private void OnInitializeGamepadSensitivity(Percentage percentage)
        {
            gamepadSensitivitySlider.value = percentage.AsPercentage;
            gamepadSensitivityText.text = percentage.ToPercentageString("F0");
        }
        
        private void OnMasterMuteChanged(bool toggle)
        {
            _viewModel.ChangeMasterMute.Execute(!toggle);
            masterVolumeSlider.interactable = toggle;
        }
        
        private void OnMasterVolumeSliderChanged(float value)
        {
            var percentage = Percentage.FromPercentage(value);
            _viewModel.ChangeMasterVolumeCommand.Execute(percentage);
            masterVolumeText.text = percentage.ToPercentageString("F0");
        }
        
        private void OnMouseSensitivitySliderChanged(float value)
        {
            var percentage = Percentage.FromPercentage(value);
            _viewModel.ChangeMouseSensitivityCommand.Execute(percentage);
            mouseSensitivityText.text = percentage.ToPercentageString("F0");
        }
        
        private void OnGamepadSensitivitySliderChanged(float value)
        {
            var percentage = Percentage.FromPercentage(value);
            _viewModel.ChangeGamepadSensitivityCommand.Execute(percentage);
            gamepadSensitivityText.text = percentage.ToPercentageString("F0");
        }
        
        private void OnSaveButtonClicked()
        {
            _viewModel.SaveCommand.Execute(Unit.Default);
        }
        
        private void OnResetButtonClicked()
        {
            _viewModel.ResetCommand.Execute(Unit.Default);
        }
        
        private void OnBackButtonClicked()
        {
            _viewModel.BackCommand.Execute(Unit.Default);
        }
        
        private void OnBackToMainMenuButtonClicked()
        {
            _viewModel.BackToMainMenuCommand.Execute(Unit.Default);
        }

        public async UniTask TransitionIn(CancellationToken cancellationToken = default)
        {
            gameObject.SetActive(true);
        }

        public async UniTask TransitionOut(CancellationToken cancellationToken = default)
        {
            gameObject.SetActive(false);
        }
    }
}