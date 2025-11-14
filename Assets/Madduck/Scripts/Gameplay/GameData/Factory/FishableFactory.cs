using System;
using Madduck.Utils;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using VContainer;

namespace Madduck.GameData
{
    public class FishableFactory : IFactory<ItemInstance>
    {
        [field: ReadOnly, HideReferenceObjectPicker,
                ShowInInspector] public ItemInstance Current { get; private set; }

        [field: HideReferenceObjectPicker,
                ShowInInspector] private readonly CompositeWeightTableInstance _weightTable;
        
        [field: HideReferenceObjectPicker,
                ShowInInspector] private readonly IModifierSource _modifierSource;
        
        [Inject]
        public FishableFactory(
            IModifierSource modifierSource,
            CompositeWeightTableInstance weightTable)
        {
            _modifierSource = modifierSource;
            _weightTable = weightTable;
        }

        public ItemInstance Create()
        {
            if (!_weightTable.TryGetRandomItem<IFishableItemData>(out var fishable))
            { 
                return null;
            }
            ItemInstance instance;
            switch (fishable)
            {
                case FishItemData fishItemData:
                    instance = new FishItemInstance(fishItemData, _modifierSource);
                    break;
                case ResourceItemData resourceItemData:
                    instance = new ResourceItemInstance(resourceItemData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fishable));
            }
            Current = instance;
            return Current;
        }
    }
    
    [Serializable]
    public class FishableFactoryMock : IFactory<ItemInstance>
    {
        [Required, 
         OdinSerialize] private IFishableItemData _testFishableItemData;
        [field: ReadOnly, HideReferenceObjectPicker,
                ShowInInspector] public ItemInstance Current { get; private set; }
        public FishableFactoryMock(){} // For inspector serialization
        public FishableFactoryMock(IFishableItemData testFishableItemData)
        {
            this._testFishableItemData = testFishableItemData;
        }
        
        public ItemInstance Create()
        {
            ItemInstance instance;
            var fishable = _testFishableItemData;
            switch (fishable)
            {
                case FishItemData fishItemData:
                    instance = new FishItemInstance(fishItemData, null);
                    break;
                case ResourceItemData resourceItemData:
                    instance = new ResourceItemInstance(resourceItemData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fishable));
            }
            Current = instance;
            return Current;
        }
    }
}