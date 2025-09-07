using System;
using System.Collections.Generic;
using Madduck.Scripts.FishingBoard.UI.Model;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Madduck.Scripts.FishingBoard.UI.ViewModel
{
    [Serializable]
    public class FishingBoardViewModel : IDisposable
    {
        public ReadOnlyReactiveProperty<bool> IsActive { get; private set; }
        public ReadOnlyReactiveProperty<Vector2> FishPosition { get; private set; }
        public ReadOnlyReactiveProperty<Vector2> HookPosition { get; private set; }
        public ReadOnlyReactiveProperty<Quaternion> FishRotation { get; private set; }
        public ReadOnlyReactiveProperty<Quaternion> HookRotation { get; private set; }
        public ReadOnlyReactiveProperty<float> FishLineDurabilityPercent { get; private set; }
        public ReadOnlyReactiveProperty<float> FatigueLevelPercent { get; private set; }
        public ReactiveCommand<Dictionary<FishZone, CircleBoardState>> OnCircleBoardUpdated { get; private set; }
        
        #region Fields
        [ShowInInspector, ReadOnly] private readonly FishingBoardModel _fishingBoardModel;
        private IDisposable _bindings;
        #endregion

        #region Life Cycle
        [Inject]
        public FishingBoardViewModel(FishingBoardModel fishingBoardModel)
        {
            _fishingBoardModel = fishingBoardModel;
            Bind();
        }
        
        private void Bind()
        {
            var disposableBuilder = Disposable.CreateBuilder();
            IsActive = _fishingBoardModel.IsActive.ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            FishPosition = _fishingBoardModel.FishPosition.ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            HookPosition = _fishingBoardModel.HookPosition.ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            FishRotation = _fishingBoardModel.FishRotation.ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            HookRotation = _fishingBoardModel.HookRotation.ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            FishLineDurabilityPercent = _fishingBoardModel.FishingLineDurabilityPercent.ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            FatigueLevelPercent = _fishingBoardModel.FatigueLevelPercent.ToReadOnlyReactiveProperty()
                .AddTo(ref disposableBuilder);
            OnCircleBoardUpdated = 
                new ReactiveCommand<Dictionary<FishZone, CircleBoardState>>(x =>
                {
                    _fishingBoardModel.CircleBoardState = x;
                })
                .AddTo(ref disposableBuilder);
            _bindings = disposableBuilder.Build();
        }

        public void Dispose()
        {
            _bindings.Dispose();
        }
        #endregion
    }
}