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
    public record WindDirectionWeightRecord : IWeightRecord<WindDirection>, IStatModifiable<WindDirectionWeightRecord>
    {
        [field: UnflagEnum, 
                Required,
                SerializeField]
        public WindDirection Item { get; internal set; }

        [field: MinValue(0f),
                SerializeField]
        public UFloat Weight { get; set; } = 1f;

        [field: DisplayAsString(TextAlignment.Center),
                ShowInInspector]
        public Percentage Probability { get; internal set; }

        public WindDirectionWeightRecord Copy() => this with {};
    }

    public class WindDirectionWeightFilter : IWeightFilter<WindDirectionWeightRecord>
    {
        private readonly Func<WindDirectionWeightRecord, bool> _predicate;

        public WindDirectionWeightFilter(Func<WindDirectionWeightRecord, bool> predicate)
        {
            _predicate = predicate;
        }

        public List<WindDirectionWeightRecord> Filter(List<WindDirectionWeightRecord> records)
        {
            return records
                .Where(_predicate)
                .Select(x => x.Copy())
                .ToList();
        }
    }
    
    [Serializable]
    public class WindDirectionWeightModifierData : BaseModifierData
    {
        [field: UnflagEnum,
                SerializeField] public WindDirection WindDirection { get; private set; }
        
        public class Builder : ModifierDataBuilder<WindDirectionWeightModifierData>
        {
            private Builder(ModifierMethod modifierMethod) : base(modifierMethod) { }

            public static Builder CreateBuilder(ModifierMethod modifierMethod)
            {
                return new Builder(modifierMethod);
            }
            
            public Builder WithWindDirection(WindDirection weatherType)
            {
                modifierData.WindDirection = weatherType;
                return this;
            }
        }
    }

    #endregion
}