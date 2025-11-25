using System;
using Madduck.Audio;
using Madduck.GameData;
using Madduck.Utils;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Room
{
    public class SettingsPanelViewModel : IDisposable
    {
        public ReactiveCommand SaveCommand { get; } = new();
        public ReactiveCommand BackCommand { get; } = new();
        public ReactiveCommand ResetCommand { get; } = new();
        
        public ReactiveProperty<bool> IsActive { get; } = new();
        public ReactiveProperty<bool> MasterMute { get; } = new();
        public ReactiveCommand<bool> ChangeMasterMute { get; } = new();
        public ReactiveProperty<Percentage> MasterVolume { get; } = new();
        public ReactiveCommand<Percentage> ChangeMasterVolumeCommand { get; } = new();
        public ReactiveProperty<Percentage> MouseSensitivity { get; } = new();
        public ReactiveCommand<Percentage> ChangeMouseSensitivityCommand { get; } = new();
        public ReactiveProperty<Percentage> GamepadSensitivity { get; } = new();
        public ReactiveCommand<Percentage> ChangeGamepadSensitivityCommand { get; } = new();
        
        private readonly SettingsPanelConfig _config;
        private readonly AudioManager _audioManager;
        private readonly GameSettingsManager _gameSettingsManager;
        private IDisposable _bindings;
        
        [Inject]
        public SettingsPanelViewModel(
            SettingsPanelConfig config,
            AudioManager audioManager, 
            GameSettingsManager gameSettingsManager)
        {
            _config = config;
            _audioManager = audioManager;
            _gameSettingsManager = gameSettingsManager;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            ChangeMasterMute
                .Subscribe(OnMasterMuteChanged)
                .AddTo(ref disposableBuilder);
            ChangeMasterVolumeCommand
                .Subscribe(OnMasterVolumeChanged)
                .AddTo(ref disposableBuilder);
            ChangeMouseSensitivityCommand
                .Subscribe(OnMouseSensitivityChanged)
                .AddTo(ref disposableBuilder);
            ChangeGamepadSensitivityCommand
                .Subscribe(OnGamepadSensitivityChanged)
                .AddTo(ref disposableBuilder);
            SaveCommand
                .Subscribe(_ => OnSave())
                .AddTo(ref disposableBuilder);
            ResetCommand
                .Subscribe(_ => OnReset())
                .AddTo(ref disposableBuilder);
            BackCommand
                .Subscribe(_ => OnBack())
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        public void SetActive(bool active)
        {
            if (active)
            { 
                IsActive.Value = true;
                OnReset();
            }
            else
            {
                IsActive.Value = false;
            }
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }

        private void Initialize()
        {
            var masterMute = _audioManager.AudioSettings.BusData[BusType.Master].IsMuted;
            MasterMute.OnNext(masterMute);
            var masterVolume = Percentage.FromFraction(
                Mathf.InverseLerp(
                    0,
                    1,
                    _audioManager.AudioSettings.BusData[BusType.Master].Decibel01Volume));
            MasterVolume.OnNext(masterVolume);
            var mouseSensitivity = Percentage.FromFraction(
                Mathf.InverseLerp(
                    _config.MouseSensitivityRange.x,
                    _config.MouseSensitivityRange.y,
                    _gameSettingsManager.ControlSettings.FishingBoardMouseSensitivity));
            MouseSensitivity.OnNext(mouseSensitivity);
            var gamepadSensitivity = Percentage.FromFraction(
                Mathf.InverseLerp(
                    _config.GamepadSensitivityRange.x,
                    _config.GamepadSensitivityRange.y,
                    _gameSettingsManager.ControlSettings.FishingBoardGamepadSensitivity));
            GamepadSensitivity.OnNext(gamepadSensitivity);
        }
        
        private void OnMasterMuteChanged(bool isMuted)
        {
            _audioManager.AudioSettings.BusData[BusType.Master].SetMute(isMuted);
        }
        
        private void OnMasterVolumeChanged(Percentage newValue)
        {
            var final = Mathf.Lerp(0, 1, newValue.AsFraction);
            _audioManager.AudioSettings.BusData[BusType.Master].SetVolume(final, VolumeUnit.Decibel01);
        }
        
        private void OnMouseSensitivityChanged(Percentage newValue)
        {
            var final = Mathf.Lerp(_config.MouseSensitivityRange.x, _config.MouseSensitivityRange.y, newValue.AsFraction);
            _gameSettingsManager.ControlSettings.FishingBoardMouseSensitivity = final;
        }

        private void OnGamepadSensitivityChanged(Percentage newValue)
        {
            var final = Mathf.Lerp(_config.GamepadSensitivityRange.x, _config.GamepadSensitivityRange.y, newValue.AsFraction);
            _gameSettingsManager.ControlSettings.FishingBoardGamepadSensitivity = final;
        }

        private void OnSave()
        {
            _gameSettingsManager.Save();
            _audioManager.Save();
        }
        
        private void OnReset()
        {
            _gameSettingsManager.Load();
            _audioManager.Load();
            Initialize();
        }
        
        private void OnBack()
        {
            SetActive(false);
        }
    }
}