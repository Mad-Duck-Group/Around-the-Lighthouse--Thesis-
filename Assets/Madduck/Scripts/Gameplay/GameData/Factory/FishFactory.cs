using System;
using Madduck.Shared;
using MessagePipe;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Madduck.GameData
{
    
    [Serializable]
    public class FishFactory : IGenericFactory<FishItemInstance>
    {
        [field: ReadOnly, HideReferenceObjectPicker,
                ShowInInspector] public FishItemInstance Current { get; private set; }

        [field: HideReferenceObjectPicker,
                ShowInInspector] private readonly FishWeightTableInstance _weightTable;
        
        private readonly IRequestHandler<ModifierRequest, ModifierResponse> _modifierHandler;
        
        [Inject]
        public FishFactory(
            FishWeightTableInstance weightTable,
            IRequestHandler<ModifierRequest, ModifierResponse> modifierHandler)
        {
            _weightTable = weightTable;
            _modifierHandler = modifierHandler;
            var modifiers = _modifierHandler.Invoke(ModifierRequest.For<FishModifierData>()).As<FishModifierData>();
            _weightTable.PersistentModifiers.Remove("CardModifiers");
            _weightTable.PersistentModifiers.TryAdd("CardModifiers", new FishWeightModifier(modifiers));
        }

        public FishItemInstance Create()
        {
            var fishItem = _weightTable.GetRandomItem();
            Current = new FishItemInstance(fishItem);
            return Current;
        }
    }
    
    [Serializable]
    public class FishFactoryMock : IGenericFactory<FishItemInstance>
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
            Current = new FishItemInstance(testFishData);
            return Current;
        }
    }
}