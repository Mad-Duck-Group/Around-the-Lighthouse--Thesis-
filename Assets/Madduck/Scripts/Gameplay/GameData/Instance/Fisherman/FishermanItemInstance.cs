using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Madduck.Utils;
using MessagePipe;
using ObservableCollections;
using R3;
using Sirenix.OdinInspector;
using VContainer;

namespace Madduck.GameData.Fisherman
{
    [Serializable]
    public class FishermanItemInstance : ItemInstance<FishermanItemData>, IDisposable
    {
        [Title("Fisherman Stats"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _fishermanStatsTitle;
        [field: ReadOnly,
                ShowInInspector] public FishermanStats CurrentStats { get; private set; }
        
        private readonly ISubscriber<ModifierSourceEvent> _modifierUpdatedEventSubscriber;
        
        [Inject]
        public FishermanItemInstance(
            FishermanItemData itemData,
            ISubscriber<ModifierSourceEvent> modifierUpdatedEventSubscriber)
            : base(itemData)
        {
            _modifierUpdatedEventSubscriber = modifierUpdatedEventSubscriber;
            CurrentStats = new FishermanStats(itemData);
            Subscribe();
        }

        private void Subscribe()
        {
            
        }

        public void Dispose()
        {
            
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