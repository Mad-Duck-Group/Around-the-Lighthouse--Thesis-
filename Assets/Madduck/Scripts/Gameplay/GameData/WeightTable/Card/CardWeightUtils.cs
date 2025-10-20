using System;
using System.Collections.Generic;
using System.Linq;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [Serializable]
    public record CardWeightRecord : IWeightRecord<CardItemData>, IStatModifiable<CardWeightRecord>
    {
        [field: Required, 
                SerializeField] public CardItemData Item { get; internal set; }
        [field: MinValue(0f), 
                SerializeField] public UFloat Weight { get; set; } = 1f;
        [field: DisplayAsString(TextAlignment.Center), 
                ShowInInspector] public Percentage Probability { get; internal set; }

        public CardWeightRecord Copy() => this with {};
    }
    
    public class CardWeightFilter : IWeightFilter<CardWeightRecord>
    {
        private readonly Func<CardWeightRecord, bool> _predicate;

        public CardWeightFilter(Func<CardWeightRecord, bool> predicate)
        {
            _predicate = predicate;
        }

        public List<CardWeightRecord> Filter(List<CardWeightRecord> records)
        {
            return records
                .Where(_predicate)
                .Select(x => x.Copy())
                .ToList();
        }
    }
}