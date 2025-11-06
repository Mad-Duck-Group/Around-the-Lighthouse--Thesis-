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
    public record WindStrengthWeightRecord : IWeightRecord<WindStrength>, IStatModifiable<WindStrengthWeightRecord>
    {
        [field: UnflagEnum,
                Required,
                SerializeField]
        public WindStrength Item { get; internal set; }

        [field: MinValue(0f),
                SerializeField]
        public UFloat Weight { get; set; } = 1f;

        [field: DisplayAsString(TextAlignment.Center),
                ShowInInspector]
        public Percentage Probability { get; internal set; }

        public WindStrengthWeightRecord Copy() => this with {};
    }

    public class WindStrengthWeightFilter : IWeightFilter<WindStrengthWeightRecord>
    {
        private readonly Func<WindStrengthWeightRecord, bool> _predicate;

        public WindStrengthWeightFilter(Func<WindStrengthWeightRecord, bool> predicate)
        {
            _predicate = predicate;
        }

        public List<WindStrengthWeightRecord> Filter(List<WindStrengthWeightRecord> records)
        {
            return records
                .Where(_predicate)
                .Select(x => x.Copy())
                .ToList();
        }
    }
    
    [Serializable]
    public class WindStrengthWeightModifierData : BaseModifierData
    {
        [field: UnflagEnum,
                SerializeField] public WindStrength WindStrength { get; private set; }
        
        public class Builder : ModifierDataBuilder<WindStrengthWeightModifierData>
        {
            private Builder(ModifierMethod modifierMethod) : base(modifierMethod) { }

            public static Builder CreateBuilder(ModifierMethod modifierMethod)
            {
                return new Builder(modifierMethod);
            }
            
            public Builder WithWindStrength(WindStrength weatherType)
            {
                modifierData.WindStrength = weatherType;
                return this;
            }
        }
    }

    #endregion
}