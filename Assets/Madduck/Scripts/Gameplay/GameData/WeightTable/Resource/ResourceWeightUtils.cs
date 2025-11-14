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
    public record ResourceWeightRecord : IWeightRecord<ResourceItemData>, IStatModifiable<ResourceWeightRecord>
    {
        [field: Required, 
                SerializeField] public ResourceItemData Item { get; internal set; }

        [field: MinValue(0f), 
                SerializeField] public UFloat Weight { get; set; } = 1f;
        [field: DisplayAsString(TextAlignment.Center), 
                ShowInInspector] public Percentage Probability { get; internal set; }
        
        public ResourceWeightRecord(ResourceItemData item, UFloat weight)
        {
            Item = item;
            Weight = weight;
        }

        public ResourceWeightRecord Copy() => this with {};
    }
    
    public class ResourceWeightFilter : IWeightFilter<ResourceWeightRecord>
    {
        private readonly Func<ResourceWeightRecord, bool> _predicate;

        public ResourceWeightFilter(Func<ResourceWeightRecord, bool> predicate)
        {
            _predicate = predicate;
        }

        public List<ResourceWeightRecord> Filter(List<ResourceWeightRecord> records)
        {
            return records
                .Where(_predicate)
                .Select(x => x.Copy())
                .ToList();
        }
    }
    
    [Serializable]
    public class ResourceWeightModifierData : BaseModifierData
    {
        [field: SerializeField] public ResourceModifierType ModifierType { get; private set; }
        [field: ShowIf(nameof(ModifierType), ResourceModifierType.Type),
                SerializeField] public ResourceType ResourceType { get; private set; }
        [field: ShowIf(nameof(ModifierType), ResourceModifierType.Name),
                SerializeField] public ResourceItemData ResourceItemData { get; private set; }
        
        public class Builder : ModifierDataBuilder<ResourceWeightModifierData>
        {
            private Builder(ModifierMethod modifierMethod) : base(modifierMethod) { }

            public static Builder CreateBuilder(ModifierMethod modifierMethod)
            {
                return new Builder(modifierMethod);
            }

            public Builder WithType(ResourceType type)
            {
                modifierData.ModifierType = ResourceModifierType.Type;
                modifierData.ResourceType = type;
                return this;
            }

            public Builder WithName(ResourceItemData itemData)
            {
                modifierData.ModifierType = ResourceModifierType.Name;
                modifierData.ResourceItemData = itemData;
                return this;
            }
        }
    }

    #endregion
}