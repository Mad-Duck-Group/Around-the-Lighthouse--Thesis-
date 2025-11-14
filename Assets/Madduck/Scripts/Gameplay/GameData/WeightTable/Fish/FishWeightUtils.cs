using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    #region Data Structure

    [Serializable]
    public record FishWeightRecord : IWeightRecord<FishItemData>, IStatModifiable<FishWeightRecord>
    {
        [field: Required, 
                SerializeField] public FishItemData Item { get; internal set; }

        [field: MinValue(0f), 
                SerializeField] public UFloat Weight { get; set; } = 1f;
        [field: DisplayAsString(TextAlignment.Center), 
                ShowInInspector] public Percentage Probability { get; internal set; }
        
        public FishWeightRecord(FishItemData item, UFloat weight)
        {
            Item = item;
            Weight = weight;
        }

        public FishWeightRecord Copy() => this with {};
    }
    
    public class FishWeightFilter : IWeightFilter<FishWeightRecord>
    {
        private readonly Func<FishWeightRecord, bool> _predicate;

        public FishWeightFilter(Func<FishWeightRecord, bool> predicate)
        {
            _predicate = predicate;
        }

        public List<FishWeightRecord> Filter(List<FishWeightRecord> records)
        {
            return records
                .Where(_predicate)
                .Select(x => x.Copy())
                .ToList();
        }
    }
    
    [Serializable]
    public class FishWeightModifierData : BaseModifierData
    {
        [field: SerializeField] public FishModifierType ModifierType { get; private set; }
        [field: ShowIf(nameof(ModifierType), FishModifierType.Size),
                SerializeField] public FishSize FishSize { get; private set; }
        [field: ShowIf(nameof(ModifierType), FishModifierType.Name),
                SerializeField] public FishItemData FishItemData { get; private set; }
        
        public class Builder : ModifierDataBuilder<FishWeightModifierData>
        {
            private Builder(ModifierMethod modifierMethod) : base(modifierMethod) { }

            public static Builder CreateBuilder(ModifierMethod modifierMethod)
            {
                return new Builder(modifierMethod);
            }

            public Builder WithSize(FishSize size)
            {
                modifierData.ModifierType = FishModifierType.Size;
                modifierData.FishSize = size;
                return this;
            }

            public Builder WithName(FishItemData fishItemData)
            {
                modifierData.ModifierType = FishModifierType.Name;
                modifierData.FishItemData = fishItemData;
                return this;
            }
        }
    }

    #endregion
}