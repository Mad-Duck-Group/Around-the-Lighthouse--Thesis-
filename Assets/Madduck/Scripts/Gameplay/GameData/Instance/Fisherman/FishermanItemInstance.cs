using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using MessagePipe;
using Sirenix.OdinInspector;

namespace Madduck.GameData.Fisherman
{
    [Serializable]
    public class FishermanItemInstance : ItemInstance<FishermanItemData>,
        IRequestHandler<ModifierRequest, ModiferResponse> 
    {
        [Title("Fisherman Stats"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _fishermanStatsTitle;
        [field: ReadOnly,
                ShowInInspector] public FishermanStats CurrentStats { get; private set; }
        [field: ReadOnly, 
                ShowInInspector] public FishingRodItemInstance CurrentFishingRod { get; private set; }
        [field: ReadOnly, 
                ShowInInspector] public List<CardItemData> CurrentCards { get; private set; }
        
        public FishermanItemInstance(FishermanItemData itemData) : base(itemData)
        {
            CurrentStats = new FishermanStats(itemData);
            CurrentCards = new List<CardItemData>(itemData.StartingCards); 
            CurrentFishingRod = new FishingRodItemInstance(ItemData.FishingRod, this);
        }

        private List<T> GetModifiers<T>() where T : BaseModifierData
        {
            return CurrentCards.SelectMany(card => card.Modifiers).OfType<T>().ToList();
        }

        private List<BaseModifierData> GetModifiers(Type modifierType)
        {
             return CurrentCards.SelectMany(card => card.Modifiers)
                 .Where(modifier => modifier.GetType() == modifierType).ToList();
        }

        public ModiferResponse Invoke(ModifierRequest request)
        {
            return new ModiferResponse(GetModifiers(request.ModifierType));
        }
    }

    [Serializable]
    public record FishermanStats : IStatModifiable<FishermanStats>
    {
        [field: DisplayAsString, InlineProperty,
                ShowInInspector] public UFloat CurrentStamina { get; set; }
        
        public FishermanStats(FishermanItemData itemData)
        {
            CurrentStamina = itemData.MaxStamina;
        }
        
        public FishermanStats Copy() => this with { };
    }
}