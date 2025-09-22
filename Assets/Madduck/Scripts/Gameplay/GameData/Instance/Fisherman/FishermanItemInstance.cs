using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using MessagePipe;
using Sirenix.OdinInspector;
using VContainer;

namespace Madduck.GameData.Fisherman
{
    [Serializable]
    public class FishermanItemInstance : ItemInstance<FishermanItemData>, IModifierProvider
    {
        [Title("Fisherman Stats"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _fishermanStatsTitle;
        [field: ReadOnly,
                ShowInInspector] public FishermanStats CurrentStats { get; private set; }
        [field: ReadOnly, 
                ShowInInspector] public FishingRodItemInstance CurrentFishingRod { get; private set; }
        [field: ReadOnly, 
                ShowInInspector] public List<CardItemInstance> CurrentCards { get; private set; }
        
        private readonly IPublisher<ModifierUpdatedEvent> _modifierUpdatedEventPublisher;
        
        [Inject]
        public FishermanItemInstance(
            FishermanItemData itemData,
            IPublisher<ModifierUpdatedEvent> modifierUpdatedEventPublisher)
            : base(itemData)
        {
            _modifierUpdatedEventPublisher = modifierUpdatedEventPublisher;
            CurrentStats = new FishermanStats(itemData);
            CurrentCards = new List<CardItemInstance>(itemData.StartingCards.Select(x => new CardItemInstance(x))); 
            CurrentFishingRod = new FishingRodItemInstance(ItemData.FishingRod, this);
        }

        public void NotifyModifierUpdate()
        {
            _modifierUpdatedEventPublisher.Publish(new ModifierUpdatedEvent(this));
        }

        public Dictionary<ModifierId, List<T>> GetModifiers<T>() where T : BaseModifierData
        {
            var dictionary = new Dictionary<ModifierId, List<T>>();
            foreach (var card in CurrentCards)
            {
                var list = new List<T>();
                foreach (var modifier in card.ItemData.Modifiers)
                {
                    if (modifier is T mod)
                    {
                        list.Add(mod);
                    }
                }
                if (list.Count > 0)
                    dictionary.Add(new ModifierId(card.InstanceGuid, card.ItemData.name), list);
            }
            return dictionary;
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