using System;
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
        
        #region Fields
        [ShowInInspector, ReadOnly] private readonly FishingBoardModel _fishingBoardModel;
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
            IsActive = _fishingBoardModel.IsActive.ToReadOnlyReactiveProperty();
            FishPosition = _fishingBoardModel.FishPosition.ToReadOnlyReactiveProperty();
            HookPosition = _fishingBoardModel.HookPosition.ToReadOnlyReactiveProperty();
            FishRotation = _fishingBoardModel.FishRotation.ToReadOnlyReactiveProperty();
            HookRotation = _fishingBoardModel.HookRotation.ToReadOnlyReactiveProperty();
            FishLineDurabilityPercent = _fishingBoardModel.FishingLineDurabilityPercent.ToReadOnlyReactiveProperty();
        }

        public void Dispose()
        {
            FishPosition.Dispose();
            HookPosition.Dispose();
            FishRotation.Dispose();
            HookRotation.Dispose();
            FishLineDurabilityPercent.Dispose();
            _fishingBoardModel.Dispose();
        }
        #endregion
    }
}