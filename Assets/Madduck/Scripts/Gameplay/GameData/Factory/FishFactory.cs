using System;
using Madduck.Shared;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Madduck.GameData
{
    public interface IFishFactory : IGenericFactory<FishItemInstance>
    {
        public FishItemInstance CurrentFish { get; }
    }
    
    [Serializable]
    public class FishFactory : IFishFactory
    {
        [field: ReadOnly, HideReferenceObjectPicker,
                ShowInInspector] public FishItemInstance CurrentFish { get; private set; }

        [field: HideReferenceObjectPicker,
                ShowInInspector] private readonly FishWeightTableInstance _weightTable;
        
        [Inject]
        public FishFactory(FishWeightTableInstance weightTable)
        {
            _weightTable = weightTable;
        }

        public FishItemInstance Create()
        {
            var fishItem = _weightTable.GetRandomItem();
            CurrentFish = new FishItemInstance(fishItem);
            return CurrentFish;
        }
    }
    
    [Serializable]
    public class FishFactoryMock : IFishFactory
    {
        [Required, 
         SerializeField] private FishItemData testFishData;
        [field: ReadOnly, HideReferenceObjectPicker,
                ShowInInspector] public FishItemInstance CurrentFish { get; private set; }
        public FishFactoryMock(){} // For inspector serialization
        public FishFactoryMock(FishItemData testFishData)
        {
            this.testFishData = testFishData;
        }
        
        public FishItemInstance Create()
        {
            CurrentFish = new FishItemInstance(testFishData);
            return CurrentFish;
        }
    }
}