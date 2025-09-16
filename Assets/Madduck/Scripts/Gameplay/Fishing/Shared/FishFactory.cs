using System;
using Madduck.GameData;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Madduck.Fishing.Shared
{
    public interface IFishFactory
    {
        public FishItemInstance CurrentFish { get; }
        public FishItemInstance GetNewFish();
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

        public FishItemInstance GetNewFish()
        {
            var fishItem = _weightTable.GetRandomItem();
            CurrentFish = new FishItemInstance(fishItem);
            return CurrentFish;
        }
    }
    
    [Serializable]
    public class TestFishFactory : IFishFactory
    {
        [Required, 
         SerializeField] private FishItemData testFishData;
        [field: ReadOnly, HideReferenceObjectPicker,
                ShowInInspector] public FishItemInstance CurrentFish { get; private set; }
        public FishItemInstance GetNewFish()
        {
            CurrentFish = new FishItemInstance(testFishData);
            return CurrentFish;
        }
    }
}