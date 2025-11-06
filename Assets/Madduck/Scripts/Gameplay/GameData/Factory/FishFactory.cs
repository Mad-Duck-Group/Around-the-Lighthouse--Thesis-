using System;
using Madduck.Shared;
using Madduck.Utils;
using MessagePipe;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Madduck.GameData
{
    
    [Serializable]
    public class FishFactory : IGenericFactory<FishItemInstance>, IDisposable
    {
        [field: ReadOnly, HideReferenceObjectPicker,
                ShowInInspector] public FishItemInstance Current { get; private set; }

        [field: HideReferenceObjectPicker,
                ShowInInspector] private readonly FishWeightTableInstance _weightTable;
        
        [field: HideReferenceObjectPicker,
                ShowInInspector] private readonly IModifierSource _modifierSource;
        
        [Inject]
        public FishFactory(
            IModifierSource modifierSource,
            FishWeightTableInstance weightTable)
        {
            _modifierSource = modifierSource;
            _weightTable = weightTable;
        }

        public FishItemInstance Create()
        {
            var fishItem = _weightTable.GetRandomItem();
            Current = new FishItemInstance(fishItem, _modifierSource);
            return Current;
        }

        public void Dispose()
        {
            Current?.Dispose();
        }
    }
    
    [Serializable]
    public class FishFactoryMock : IGenericFactory<FishItemInstance>, IDisposable
    {
        [Required, 
         SerializeField] private FishItemData testFishData;
        [field: ReadOnly, HideReferenceObjectPicker,
                ShowInInspector] public FishItemInstance Current { get; private set; }
        public FishFactoryMock(){} // For inspector serialization
        public FishFactoryMock(FishItemData testFishData)
        {
            this.testFishData = testFishData;
        }
        
        public FishItemInstance Create()
        {
            Current = new FishItemInstance(testFishData, new ModifierSourceMock());
            return Current;
        }

        public void Dispose()
        {
            Current?.Dispose();
        }
    }
}