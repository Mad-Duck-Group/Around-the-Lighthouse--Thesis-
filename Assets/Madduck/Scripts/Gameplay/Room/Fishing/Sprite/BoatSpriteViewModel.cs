using System;
using Madduck.RoomPreset;
using PrimeTween;
using R3;
using VContainer;

namespace Madduck.Room
{
    public class BoatSpriteViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<TweenSettings<float>> TweenSettings => 
            _currentTweenSettings.Select(x => x).ToReadOnlyReactiveProperty();
        private readonly RoomPresetManager _roomPresetManager;
        private IDisposable _bindings;
        
        private readonly ReactiveProperty<TweenSettings<float>> _currentTweenSettings = new();

        [Inject]
        public BoatSpriteViewModel(RoomPresetManager roomPresetManager)
        {
            _roomPresetManager = roomPresetManager;
            Bind();
        }

        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            _roomPresetManager.CurrentRoomPreset
                .Where(x => x)
                .Subscribe(OnRoomPresetChanged)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }

        private void OnRoomPresetChanged(RoomPreset.RoomPreset preset)
        {
            _currentTweenSettings.Value = preset.WaveTweenSettings;
        }
    }
}