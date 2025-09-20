using System;
using Madduck.Fishing.Config;
using Madduck.Fishing.Shared;
using Madduck.GameData;
using Madduck.GameData.Fisherman;
using Madduck.Utils;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.UI
{
    [Serializable]
    public class ReelingModel : IDisposable
    {
        [field: SerializeField] public SerializableReactiveProperty<UFloat> CurrentReelingProgress { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<UFloat> MaxReelingProgress { get; private set; }
        [field: SerializeField] public FishingRodItemInstance FishingRodInstance { get; private set; }
        [field: SerializeField] public FishItemInstance FishInstance { get; private set; }

        private readonly ReelingConfig _config;
        private IDisposable _bindings;
        
        [Inject]
        public ReelingModel(
            ReelingConfig config,
            FishermanItemInstance fisherman)
        {
            _config = config;
            FishingRodInstance = fisherman.CurrentFishingRod;
            Bind();
        }
        
        private void Bind()
        {
            _bindings?.Dispose();
            var disposableBuilder = Disposable.CreateBuilder();
            CurrentReelingProgress = new SerializableReactiveProperty<UFloat>(0f)
                .AddTo(ref disposableBuilder);
            MaxReelingProgress = new SerializableReactiveProperty<UFloat>(_config.MaxReelingValue)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void SetFishInstance(FishItemInstance fishItemInstance)
        {
            FishInstance = fishItemInstance;
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