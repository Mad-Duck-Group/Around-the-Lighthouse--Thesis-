using System;
using Madduck.GameData.Bait;
using Madduck.Utils;
using R3;
using VContainer;

namespace Madduck.Fishing.UI
{
    public class NibbleViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<Percentage> CatchChance { get; private set; }
        public ReadOnlyReactiveProperty<uint> CatchStage { get; private set; }
        public ReadOnlyReactiveProperty<BaitItemInstance> CurrentBait { get; private set; }
        
        private readonly NibbleModel _model;
        private IDisposable _bindings;
        
        [Inject]
        public NibbleViewModel(NibbleModel model)
        {
            _model = model;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            CatchChance = _model.CatchChance
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            CatchStage = _model.CatchStage
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            CurrentBait = _model.PlayerInventory.CurrentBaitView
                .ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }
        
        public void Dispose()
        {
            _bindings.Dispose();
        }
    }
}