using System;
using MadDuck.Scripts.Items.Instance;
using Madduck.Scripts.Utils.Others;
using R3;
using UnityEngine;
using VContainer;

namespace Madduck.Scripts.Fishing.UI.Nibble
{
    [Serializable]
    public class NibbleModel : IDisposable
    {
        [field: SerializeField] public SerializableReactiveProperty<bool> IsActive { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<bool> IsNibbling { get; private set; }
        [field: SerializeField] public SerializableReactiveProperty<Sign> PullHookResult { get; private set; }
        [field: SerializeField] public FishItemInstance FishItemInstance { get; private set; }
        private IDisposable _bindings;
        
        [Inject]
        public NibbleModel(FishItemInstance fishItemInstance)
        {
            FishItemInstance = fishItemInstance;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            IsActive = new SerializableReactiveProperty<bool>(false)
                .AddTo(ref disposableBuilder);
            IsNibbling = new SerializableReactiveProperty<bool>(false)
                .AddTo(ref disposableBuilder);
            PullHookResult = new SerializableReactiveProperty<Sign>(Sign.Zero)
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Reset()
        {
            IsActive.Value = false;
            IsNibbling.Value = false;
            PullHookResult.Value = Sign.Zero;
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}