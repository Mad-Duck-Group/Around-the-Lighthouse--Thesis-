using System;
using Madduck.Scripts.Fishing.Config.Reeling;
using MadDuck.Scripts.Items.Instance;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Scripts.Fishing.UI.Reeling
{
    [Serializable]
    public class ReelingModel : IDisposable
    {
        [field: SerializeField] public SerializableReactiveProperty<bool> IsActive { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<float> CurrentReelingProgress { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<float> MaxReelingProgress { get; private set; }
        [field: SerializeField] public FishingRodItemInstance FishingRodInstance { get; private set; }
        [field: SerializeField] public FishItemInstance FishInstance { get; private set; }

        private readonly ReelingConfig _config;
        private IDisposable _bindings;
        
        [Inject]
        public ReelingModel(
            ReelingConfig config,
            FishingRodItemInstance fishingRodItemInstance, 
            FishItemInstance fishItemInstance)
        {
            _config = config;
            FishingRodInstance = fishingRodItemInstance;
            FishInstance = fishItemInstance;
            Bind();
        }
        
        private void Bind()
        {
            _bindings?.Dispose();
            var disposableBuilder = Disposable.CreateBuilder();
            IsActive = new SerializableReactiveProperty<bool>(false)
                .AddTo(ref disposableBuilder);
            CurrentReelingProgress = new SerializableReactiveProperty<float>(0f)
                .AddTo(ref disposableBuilder);
            MaxReelingProgress = new SerializableReactiveProperty<float>(_config.MaxReelingValue)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        public void Reset()
        {
            CurrentReelingProgress.Value = 0f;
            MaxReelingProgress.Value = _config.MaxReelingValue;
            FishInstance.CurrentFatigueCount = 0;
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}